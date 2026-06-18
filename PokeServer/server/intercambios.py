# ---------------------------------------------------------------------------
# INTERCAMBIOS DE POKÉMON
# ---------------------------------------------------------------------------
# Flujo:
#   1. POST /trade_requests/<rival_id>          → envía solicitud al rival
#   2. POST /trade_requests/<msg_id>/respond    → rival acepta/rechaza
#      Si acepta → crea documento en Trades con status=pending
#   3. GET  /trades/<trade_id>                  → consulta estado del trade
#   4. POST /trades/<trade_id>/offer            → cada jugador elige su pokémon
#      Cuando ambos ofrecen → status=offered
#   5. POST /trades/<trade_id>/confirm          → cada jugador confirma
#      Cuando ambos confirman → ejecuta el intercambio real en PokemonUser
#      + comprueba evolución por intercambio en el pokémon RECIBIDO
#   6. POST /trades/<trade_id>/cancel           → cualquiera puede cancelar
# ---------------------------------------------------------------------------

from flask import request, jsonify
from bson import ObjectId
import datetime


# ---------------------------------------------------------------------------
# HELPER: evolución por intercambio
# ---------------------------------------------------------------------------

def _intentar_evolucion_intercambio(poke_doc, pokedex, pokemon_user):
    """
    Comprueba si el pokémon tiene método de evolución 'intercambio'.
    Si es así, actualiza el documento en PokemonUser con los datos
    de la evolución (nombre, tipos, estadísticas, etc.).

    Devuelve (evolucionado: bool, nombre_evolucion: str).
    """
    evolucion = poke_doc.get("evolucion") or {}
    metodo    = (evolucion.get("metodo") or "").lower()

    if metodo != "intercambio":
        return False, ""

    evo_nombre     = evolucion.get("nombre", "")
    evo_pokemon_id = evolucion.get("pokemon_id", "")

    # Buscar la forma evolucionada en la Pokédex
    evo_pdex = pokedex.find_one(
        {"$or": [
            {"pokemon_id": evo_pokemon_id},
            {"nombre":     {"$regex": f"^{evo_nombre}$", "$options": "i"}},
        ]}
    )
    if not evo_pdex:
        # No encontrado en la Pokédex → no evolucionamos pero no rompemos el intercambio
        print(f"[TRADE-EVO] Pokédex entry not found for '{evo_pokemon_id}' / '{evo_nombre}'")
        return False, evo_nombre

    # Construir el update con los datos de la evolución
    stats_base  = evo_pdex.get("estadisticas_base", poke_doc.get("estadisticas_base", {}))
    nuevo_nombre = evo_pdex.get("nombre", evo_nombre)
    tipo1        = ""
    tipo2        = ""
    tipos_lista  = evo_pdex.get("tipos", [])
    if len(tipos_lista) > 0:
        tipo1 = tipos_lista[0]
    if len(tipos_lista) > 1:
        tipo2 = tipos_lista[1]

    nuevo_numero = evo_pdex.get("numero_pokedex", poke_doc.get("numero_pokedex", 0))

    evo_update = {
        "PokemonId":        nuevo_numero,
        "numero_pokedex":   nuevo_numero,
        "Nombre":           nuevo_nombre,
        "TipoPrincipal":    tipo1,
        "TipoSecundario":   tipo2,
        "estadisticas_base": stats_base,
        "CurrentHp":        int(stats_base.get("ps", poke_doc.get("CurrentHp", 0))),
        "evolucion":        evo_pdex.get("evolucion"),   # próxima evo (si la hay)
    }

    pokemon_user.update_one(
        {"_id": poke_doc["_id"]},
        {"$set": evo_update}
    )

    print(f"[TRADE-EVO] {poke_doc.get('Nombre')} → {nuevo_nombre} (intercambio)")
    return True, nuevo_nombre


# ---------------------------------------------------------------------------
# RUTAS
# ---------------------------------------------------------------------------

def register_trade_routes(app, usuarios, mensajes, pokemon_user, trades, pokedex, token_required, gf):
    """
    Registra todas las rutas de intercambio.
    Requiere 'pokedex' para la comprobación de evolución.
    """

    # ── 1. ENVIAR SOLICITUD ────────────────────────────────────────────────
    @app.post("/trade_requests/<rival_id>")
    @token_required
    def make_trade_request(current_user, rival_id):
        try:
            rival = usuarios.find_one({"_id": ObjectId(rival_id)})
            if not rival:
                return jsonify({"error": "Usuario no encontrado"}), 404

            if str(current_user["_id"]) == rival_id:
                return jsonify({"error": "No puedes intercambiar contigo mismo"}), 400

            doc = {
                "_id":       ObjectId(),
                "from":      str(gf(current_user, "Username", "username", default="")),
                "from_id":   str(current_user["_id"]),
                "to":        str(rival_id),
                "title":     "Trade Request",
                "text":      gf(current_user, "Username", "username", default="")
                             + " quiere intercambiar un Pokémon contigo. ¿Aceptas?",
                "Fecha":     datetime.datetime.utcnow().isoformat(),
                "type":      "trade_request",
                "responded": False,
            }
            mensajes.insert_one(doc)
            doc["_id"] = str(doc["_id"])
            return jsonify(doc), 201
        except Exception:
            import traceback; traceback.print_exc()
            return jsonify({"error": "Error interno del servidor"}), 500

    # ── 2. RESPONDER SOLICITUD ─────────────────────────────────────────────
    @app.post("/trade_requests/<msg_id>/respond")
    @token_required
    def respond_trade_request(current_user, msg_id):
        try:
            data     = request.json or {}
            accepted = bool(data.get("accepted", False))

            msg = mensajes.find_one({
                "_id":       ObjectId(msg_id),
                "to":        str(current_user["_id"]),
                "type":      "trade_request",
                "responded": False,
            })
            if not msg:
                return jsonify({"error": "Solicitud no encontrada o ya respondida"}), 404

            mensajes.update_one(
                {"_id": ObjectId(msg_id)},
                {"$set": {"responded": True, "accepted": accepted}}
            )

            sender_id = msg.get("from_id", "")

            if not accepted:
                mensajes.insert_one({
                    "_id":       ObjectId(),
                    "from":      str(gf(current_user, "Username", "username", default="?")),
                    "from_id":   str(current_user["_id"]),
                    "to":        sender_id,
                    "title":     "Trade Rejected",
                    "text":      gf(current_user, "Username", "username", default="?")
                                 + " ha rechazado tu solicitud de intercambio.",
                    "Fecha":     datetime.datetime.utcnow().isoformat(),
                    "type":      "trade_rejected",
                    "responded": False,
                })
                return jsonify({"msg": "Solicitud rechazada"}), 200

            # ── ACEPTADO: crear documento Trade ───────────────────────────
            trade_id = ObjectId()
            trades.insert_one({
                "_id":               trade_id,
                "player1_id":        sender_id,
                "player2_id":        str(current_user["_id"]),
                "player1_name":      msg.get("from", ""),
                "player2_name":      str(gf(current_user, "Username", "username", default="")),
                "status":            "pending",
                "player1_pokemon":   None,
                "player2_pokemon":   None,
                "player1_confirmed": False,
                "player2_confirmed": False,
                "created_at":        datetime.datetime.utcnow().isoformat(),
            })

            trade_id_str = str(trade_id)

            mensajes.insert_one({
                "_id":       ObjectId(),
                "from":      str(gf(current_user, "Username", "username", default="?")),
                "from_id":   str(current_user["_id"]),
                "to":        sender_id,
                "title":     "Trade Accepted",
                "text":      gf(current_user, "Username", "username", default="?")
                             + " ha aceptado tu solicitud de intercambio.",
                "Fecha":     datetime.datetime.utcnow().isoformat(),
                "type":      "trade_response",
                "trade_id":  trade_id_str,
                "responded": False,
            })

            return jsonify({"msg": "Intercambio aceptado", "trade_id": trade_id_str}), 200

        except Exception:
            import traceback; traceback.print_exc()
            return jsonify({"error": "Error interno del servidor"}), 500

    # ── 3. CONSULTAR ESTADO ────────────────────────────────────────────────
    @app.get("/trades/<trade_id>")
    @token_required
    def get_trade(current_user, trade_id):
        try:
            trade = trades.find_one({"_id": ObjectId(trade_id)})
            if not trade:
                return jsonify({"error": "Intercambio no encontrado"}), 404

            uid = str(current_user["_id"])
            if trade["player1_id"] != uid and trade["player2_id"] != uid:
                return jsonify({"error": "No eres parte de este intercambio"}), 403

            trade["_id"] = str(trade["_id"])

            for slot in ("player1_pokemon", "player2_pokemon"):
                poke_id = trade.get(slot)
                if poke_id:
                    try:
                        poke = pokemon_user.find_one({"_id": ObjectId(poke_id)})
                        if poke:
                            poke["_id"] = str(poke["_id"])
                            trade[slot + "_detail"] = poke
                    except Exception:
                        pass

            return jsonify(trade), 200
        except Exception:
            import traceback; traceback.print_exc()
            return jsonify({"error": "Error interno del servidor"}), 500

    # ── 4. OFRECER POKÉMON ─────────────────────────────────────────────────
    @app.post("/trades/<trade_id>/offer")
    @token_required
    def offer_pokemon(current_user, trade_id):
        """
        Body: { "pokemon_id": "<ObjectId del PokemonUser>" }
        """
        try:
            data       = request.json or {}
            pokemon_id = data.get("pokemon_id", "").strip()
            if not pokemon_id:
                return jsonify({"error": "Falta pokemon_id"}), 400

            trade = trades.find_one({"_id": ObjectId(trade_id)})
            if not trade:
                return jsonify({"error": "Intercambio no encontrado"}), 404

            uid = str(current_user["_id"])
            if trade["player1_id"] != uid and trade["player2_id"] != uid:
                return jsonify({"error": "No eres parte de este intercambio"}), 403

            if trade["status"] not in ("pending", "offered"):
                return jsonify({"error": f"El intercambio ya está en estado '{trade['status']}'"}), 409

            poke = pokemon_user.find_one({"_id": ObjectId(pokemon_id), "UserId": uid})
            if not poke:
                return jsonify({"error": "Pokémon no encontrado o no te pertenece"}), 404

            slot = "player1_pokemon" if trade["player1_id"] == uid else "player2_pokemon"
            trades.update_one({"_id": ObjectId(trade_id)}, {"$set": {slot: pokemon_id}})

            updated = trades.find_one({"_id": ObjectId(trade_id)})
            if updated["player1_pokemon"] and updated["player2_pokemon"]:
                trades.update_one({"_id": ObjectId(trade_id)}, {"$set": {"status": "offered"}})

            return jsonify({"msg": "Pokémon ofrecido", "trade_id": trade_id}), 200

        except Exception:
            import traceback; traceback.print_exc()
            return jsonify({"error": "Error interno del servidor"}), 500

    # ── 5. CONFIRMAR INTERCAMBIO ───────────────────────────────────────────
    @app.post("/trades/<trade_id>/confirm")
    @token_required
    def confirm_trade(current_user, trade_id):
        """
        Cuando ambos confirman:
          1. Se intercambian los propietarios en PokemonUser.
          2. Tras el cambio de dueño, se comprueba si el pokémon RECIBIDO
             tiene método de evolución 'intercambio'; si es así, evoluciona
             automáticamente en la BD.
          3. Se envían mensajes de notificación (incluyendo la evolución si ocurrió).
        """
        try:
            trade = trades.find_one({"_id": ObjectId(trade_id)})
            if not trade:
                return jsonify({"error": "Intercambio no encontrado"}), 404

            uid = str(current_user["_id"])
            if trade["player1_id"] != uid and trade["player2_id"] != uid:
                return jsonify({"error": "No eres parte de este intercambio"}), 403

            if trade["status"] != "offered":
                return jsonify({"error": "Ambos jugadores deben haber ofrecido un Pokémon primero"}), 409

            confirm_slot = "player1_confirmed" if trade["player1_id"] == uid else "player2_confirmed"
            trades.update_one({"_id": ObjectId(trade_id)}, {"$set": {confirm_slot: True}})

            updated = trades.find_one({"_id": ObjectId(trade_id)})

            if updated["player1_confirmed"] and updated["player2_confirmed"]:
                # ── A. Datos del intercambio ──────────────────────────────
                p1_poke_id = updated["player1_pokemon"]   # ofrecido por player1
                p2_poke_id = updated["player2_pokemon"]   # ofrecido por player2
                p1_id      = updated["player1_id"]
                p2_id      = updated["player2_id"]
                p1_name    = updated["player1_name"]
                p2_name    = updated["player2_name"]

                # ── B. Cambiar propietario ────────────────────────────────
                # p1_poke va a player2; p2_poke va a player1
                pokemon_user.update_one(
                    {"_id": ObjectId(p1_poke_id)},
                    {"$set": {"UserId": p2_id, "Username": p2_name}}
                )
                pokemon_user.update_one(
                    {"_id": ObjectId(p2_poke_id)},
                    {"$set": {"UserId": p1_id, "Username": p1_name}}
                )

                # ── C. Comprobar evolución por intercambio ────────────────
                # El pokémon que recibe player1 es p2_poke (antes de player2)
                # El pokémon que recibe player2 es p1_poke (antes de player1)
                evoluciones = {}  # { user_id: nombre_evolucion }

                p2_poke_doc = pokemon_user.find_one({"_id": ObjectId(p2_poke_id)})
                if p2_poke_doc:
                    evolucionado, evo_nombre = _intentar_evolucion_intercambio(
                        p2_poke_doc, pokedex, pokemon_user
                    )
                    if evolucionado:
                        evoluciones[p1_id] = evo_nombre   # player1 recibe este poke

                p1_poke_doc = pokemon_user.find_one({"_id": ObjectId(p1_poke_id)})
                if p1_poke_doc:
                    evolucionado, evo_nombre = _intentar_evolucion_intercambio(
                        p1_poke_doc, pokedex, pokemon_user
                    )
                    if evolucionado:
                        evoluciones[p2_id] = evo_nombre   # player2 recibe este poke

                # ── D. Actualizar estado del trade ────────────────────────
                trades.update_one(
                    {"_id": ObjectId(trade_id)},
                    {"$set": {
                        "status":       "done",
                        "completed_at": datetime.datetime.utcnow().isoformat(),
                        "evoluciones":  evoluciones,
                    }}
                )

                # ── E. Notificar a ambos jugadores ────────────────────────
                for recipient_id, other_name in ((p1_id, p2_name), (p2_id, p1_name)):
                    evo_texto = ""
                    if recipient_id in evoluciones:
                        evo_texto = f" Además, ¡tu nuevo Pokémon ha evolucionado a {evoluciones[recipient_id]}!"

                    mensajes.insert_one({
                        "_id":       ObjectId(),
                        "from":      "Sistema",
                        "from_id":   "",
                        "to":        recipient_id,
                        "title":     "¡Intercambio exitoso!",
                        "text":      f"Tu intercambio con {other_name} se ha completado con éxito."
                                     f"{evo_texto} ¡Revisa tu equipo!",
                        "Fecha":     datetime.datetime.utcnow().isoformat(),
                        "type":      "trade_done",
                        "trade_id":  trade_id,
                        "evolucion": evoluciones.get(recipient_id),
                        "responded": False,
                    })

                return jsonify({
                    "msg":        "¡Intercambio completado con éxito!",
                    "status":     "done",
                    "evoluciones": evoluciones,   # { user_id: nombre_nueva_forma }
                }), 200

            return jsonify({"msg": "Confirmación registrada, esperando al otro jugador"}), 200

        except Exception:
            import traceback; traceback.print_exc()
            return jsonify({"error": "Error interno del servidor"}), 500

    # ── 6. CANCELAR INTERCAMBIO ────────────────────────────────────────────
    @app.post("/trades/<trade_id>/cancel")
    @token_required
    def cancel_trade(current_user, trade_id):
        try:
            trade = trades.find_one({"_id": ObjectId(trade_id)})
            if not trade:
                return jsonify({"error": "Intercambio no encontrado"}), 404

            uid = str(current_user["_id"])
            if trade["player1_id"] != uid and trade["player2_id"] != uid:
                return jsonify({"error": "No eres parte de este intercambio"}), 403

            if trade["status"] == "done":
                return jsonify({"error": "El intercambio ya se completó"}), 409

            trades.update_one(
                {"_id": ObjectId(trade_id)},
                {"$set": {"status": "cancelled", "cancelled_by": uid}}
            )

            other_id = trade["player2_id"] if trade["player1_id"] == uid else trade["player1_id"]
            mensajes.insert_one({
                "_id":       ObjectId(),
                "from":      str(gf(current_user, "Username", "username", default="?")),
                "from_id":   uid,
                "to":        other_id,
                "title":     "Intercambio cancelado",
                "text":      gf(current_user, "Username", "username", default="?")
                             + " ha cancelado el intercambio.",
                "Fecha":     datetime.datetime.utcnow().isoformat(),
                "type":      "trade_cancelled",
                "responded": False,
            })

            return jsonify({"msg": "Intercambio cancelado"}), 200

        except Exception:
            import traceback; traceback.print_exc()
            return jsonify({"error": "Error interno del servidor"}), 500

    # ── LISTAR INTERCAMBIOS DEL USUARIO ───────────────────────────────────
    @app.get("/trades")
    @token_required
    def list_trades(current_user):
        try:
            uid = str(current_user["_id"])
            lista = list(trades.find({
                "$or": [{"player1_id": uid}, {"player2_id": uid}]
            }).sort("created_at", -1).limit(50))
            for t in lista:
                t["_id"] = str(t["_id"])
            return jsonify(lista), 200
        except Exception:
            import traceback; traceback.print_exc()
            return jsonify({"error": "Error interno del servidor"}), 500
