from flask import Flask, request, jsonify
from flask_cors import CORS
from pymongo.mongo_client import MongoClient
from pymongo.server_api import ServerApi
from pymongo.errors import OperationFailure
import jwt
import datetime
import random
import string
import re
from functools import wraps
from bson import ObjectId
from werkzeug.security import generate_password_hash, check_password_hash

app = Flask(__name__)
CORS(app)

app.config["SECRET_KEY"] = "super_secret_key"  # cámbiala en producción

# -------------------------
# CONEXIÓN MONGODB
# -------------------------
import os
uri = os.environ.get(
    "MONGO_URI",
    "mongodb+srv://marcosemiliorodriguezmartin_db_user:gDfjWHYHIqMJ346V@pokecasino.asaeily.mongodb.net/?retryWrites=true&w=majority&appName=PokeCasino"
)

try:
    client = MongoClient(uri, server_api=ServerApi("1"))
    db = client["PokemonPyDB"]
    usuarios      = db["usuarios"]
    premios_col   = db["premios"]
    pokedex_col   = db["pokedex"]
    pokemon_user_col = db["PokemonUser"]
    medallas_col  = db["medallas_user"]
    print("Colecciones disponibles:", db.list_collection_names())
except OperationFailure as e:
    if e.code == 8000 or "bad auth" in str(e).lower():
        print("ERROR: Credenciales de MongoDB Atlas incorrectas. Revisa la URI o usa la variable MONGO_URI.")
    else:
        print(f"ERROR de MongoDB: {e}")
    exit(1)


# -------------------------
# HELPERS JWT / ROLES
# -------------------------

def generate_token(user_id, rol):
    payload = {
        "user_id": str(user_id),
        "rol": rol,
        "exp": datetime.datetime.utcnow() + datetime.timedelta(hours=2)
    }
    return jwt.encode(payload, app.config["SECRET_KEY"], algorithm="HS256")


def token_required(f):
    @wraps(f)
    def decorated(*args, **kwargs):
        auth_header = request.headers.get("Authorization", None)
        if not auth_header or not auth_header.startswith("Bearer "):
            return jsonify({"error": "Token requerido"}), 401

        token = auth_header.split(" ")[1]
        try:
            data = jwt.decode(token, app.config["SECRET_KEY"], algorithms=["HS256"])
            user_id = data["user_id"]
            try:
                current_user = usuarios.find_one({"_id": ObjectId(user_id)})
            except Exception:
                current_user = None
            if not current_user:
                current_user = usuarios.find_one({"_id": user_id})
            if not current_user:
                return jsonify({"error": "Usuario no encontrado"}), 401
        except jwt.ExpiredSignatureError:
            return jsonify({"error": "Token expirado"}), 401
        except jwt.InvalidTokenError:
            return jsonify({"error": "Token inválido"}), 401

        return f(current_user, *args, **kwargs)
    return decorated


def admin_required(f):
    @wraps(f)
    def decorated(current_user, *args, **kwargs):
        if current_user.get("rol") != "admin":
            return jsonify({"error": "Permisos insuficientes"}), 403
        return f(current_user, *args, **kwargs)
    return decorated


def _serialize(doc):
    """Convierte _id ObjectId a string y elimina password."""
    if doc is None:
        return None
    doc["_id"] = str(doc["_id"])
    doc.pop("password", None)
    return doc


# -------------------------
# AUTH
# -------------------------

@app.post("/auth/register")
def register():
    data = request.json or {}
    username = data.get("username", "").strip()
    email    = data.get("email", "").strip()
    password = data.get("password", "")

    if not username or not email or not password:
        return jsonify({"error": "Faltan campos obligatorios"}), 400

    if usuarios.find_one({"username_lower": username.lower()}):
        return jsonify({"error": "El nombre de usuario ya existe"}), 400
    if usuarios.find_one({"email_lower": email.lower()}):
        return jsonify({"error": "El email ya está registrado"}), 400

    nuevo = {
        "username":       username,
        "username_lower": username.lower(),
        "nombre":         data.get("nombre", ""),
        "apellido":       data.get("apellido", ""),
        "edad":           int(data.get("edad", 0)),
        "email":          email,
        "email_lower":    email.lower(),
        "password":       generate_password_hash(password),
        "rol":            "usuario",
        "fichas":         300,
        "pokes":          0
    }
    usuarios.insert_one(nuevo)
    return jsonify({"mensaje": "Usuario registrado correctamente"}), 201


@app.post("/auth/login")
def login():
    try:
        data       = request.json or {}
        user_input = (data.get("email") or data.get("username") or "").strip()
        password   = data.get("password", "")

        if not user_input or not password:
            return jsonify({"error": "Faltan credenciales"}), 400

        usuario = usuarios.find_one({
            "$or": [
                {"email_lower":    user_input.lower()},
                {"username_lower": user_input.lower()}
            ]
        })

        if not usuario or not check_password_hash(usuario["password"], password):
            return jsonify({"error": "Usuario o contraseña incorrectos"}), 401

        token = generate_token(usuario["_id"], usuario.get("rol", "usuario"))

        return jsonify({
            "mensaje":  "Login correcto",
            "token":    token,
            "id":       str(usuario["_id"]),
            "username": usuario["username"],
            "email":    usuario["email"],
            "rol":      usuario.get("rol", "usuario"),
            "fichas":   usuario.get("fichas", 0),
            "pokes":    usuario.get("pokes", 0)
        }), 200
    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error en el servidor"}), 500


@app.get("/auth/me")
@token_required
def me(current_user):
    return jsonify(_serialize(current_user)), 200


# -------------------------
# CONTRASEÑA
# -------------------------

@app.post("/auth/cambiar_password")
@token_required
def cambiar_password(current_user):
    """El usuario cambia su propia contraseña verificando la actual."""
    data            = request.json or {}
    password_actual = data.get("password_actual", "")
    nueva           = data.get("nueva_password", "")

    if not password_actual or not nueva:
        return jsonify({"error": "Faltan campos"}), 400

    if not check_password_hash(current_user["password"], password_actual):
        return jsonify({"error": "Contraseña actual incorrecta"}), 401

    if len(nueva) < 6:
        return jsonify({"error": "La nueva contraseña debe tener al menos 6 caracteres"}), 400

    usuarios.update_one(
        {"_id": current_user["_id"]},
        {"$set": {"password": generate_password_hash(nueva)}}
    )
    return jsonify({"mensaje": "Contraseña actualizada correctamente"}), 200


@app.post("/auth/recuperar_password")
def recuperar_password():
    """Genera una contraseña aleatoria y la envía por email (simulado en logs)."""
    data     = request.json or {}
    email    = data.get("email", "").strip()
    username = data.get("username", "").strip()

    if not email or not username:
        return jsonify({"error": "Se requieren email y username"}), 400

    usuario = usuarios.find_one({"email_lower": email.lower()})
    if not usuario:
        return jsonify({"error": "No se encontró ningún usuario con ese email"}), 404

    if usuario.get("username", "").lower() != username.lower():
        return jsonify({"error": "El username no coincide con el email proporcionado"}), 400

    nueva = _generar_password_aleatoria()
    usuarios.update_one(
        {"_id": usuario["_id"]},
        {"$set": {"password": generate_password_hash(nueva)}}
    )

    # TODO: integrar envío de email real (smtplib / SendGrid)
    print(f"[EMAIL] Para {email}: tu nueva contraseña es '{nueva}'")

    return jsonify({"mensaje": "Se ha enviado una nueva contraseña al correo indicado"}), 200


def _generar_password_aleatoria(longitud=10):
    chars = string.ascii_letters + string.digits
    return "".join(random.choice(chars) for _ in range(longitud))


# -------------------------
# CRUD USUARIOS (admin)
# -------------------------

@app.get("/usuarios")
@token_required
def listar_usuarios(current_user):
    lista = list(usuarios.find())
    return jsonify([_serialize(u) for u in lista]), 200


@app.get("/usuarios/<id>")
@token_required
def obtener_usuario(current_user, id):
    try:
        obj_id = ObjectId(id)
    except Exception:
        return jsonify({"error": "ID inválido"}), 400
    u = usuarios.find_one({"_id": obj_id})
    if not u:
        return jsonify({"error": "Usuario no encontrado"}), 404
    return jsonify(_serialize(u)), 200


@app.post("/usuarios")
@token_required
@admin_required
def crear_usuario(current_user):
    data = request.json or {}
    required = ["nombre", "apellido", "username", "email", "password", "edad", "rol"]
    if not all(k in data for k in required):
        return jsonify({"error": "Faltan campos"}), 400

    if usuarios.find_one({"email_lower": data["email"].lower()}) or \
       usuarios.find_one({"username_lower": data["username"].lower()}):
        return jsonify({"error": "Email o username ya registrado"}), 400

    usuario = {
        "username":       data["username"],
        "username_lower": data["username"].lower(),
        "nombre":         data["nombre"],
        "apellido":       data["apellido"],
        "email":          data["email"],
        "email_lower":    data["email"].lower(),
        "edad":           int(data["edad"]),
        "password":       generate_password_hash(data["password"]),
        "rol":            data["rol"],
        "fichas":         int(data.get("fichas", 0)),
        "pokes":          int(data.get("pokes", 0))
    }
    usuarios.insert_one(usuario)
    return jsonify({"msg": "Usuario creado"}), 201


@app.put("/usuarios/<id>")
@token_required
@admin_required
def modificar_usuario(current_user, id):
    try:
        obj_id = ObjectId(id)
    except Exception:
        return jsonify({"error": "ID inválido"}), 400

    data   = request.json or {}
    update = {k: data[k] for k in ["nombre", "apellido", "email", "rol", "fichas", "pokes", "edad", "username"] if k in data}

    if not update:
        return jsonify({"error": "No se enviaron datos para actualizar"}), 400

    # Sincronizar campos _lower si vienen
    if "email" in update:
        update["email_lower"] = update["email"].lower()
    if "username" in update:
        update["username_lower"] = update["username"].lower()

    r = usuarios.update_one({"_id": obj_id}, {"$set": update})
    if r.matched_count == 0:
        return jsonify({"error": "Usuario no encontrado"}), 404
    return jsonify({"msg": "Usuario actualizado"}), 200


@app.delete("/usuarios/<id>")
@token_required
@admin_required
def eliminar_usuario(current_user, id):
    try:
        obj_id = ObjectId(id)
    except Exception:
        return jsonify({"error": "ID inválido"}), 400
    r = usuarios.delete_one({"_id": obj_id})
    if r.deleted_count == 0:
        return jsonify({"error": "Usuario no encontrado"}), 404
    return jsonify({"msg": "Usuario eliminado"}), 200


@app.put("/usuarios/<id>/reset_password")
@token_required
@admin_required
def reset_password(current_user, id):
    try:
        obj_id = ObjectId(id)
    except Exception:
        return jsonify({"error": "ID inválido"}), 400

    nueva = (request.json or {}).get("password", "")
    if not nueva:
        # Si no se pasa contraseña, se genera una por defecto y se "envía por email"
        nueva = _generar_password_aleatoria()
        usuario = usuarios.find_one({"_id": obj_id})
        if not usuario:
            return jsonify({"error": "Usuario no encontrado"}), 404
        print(f"[EMAIL] Para {usuario.get('email')}: contraseña reseteada a '{nueva}'")

    r = usuarios.update_one({"_id": obj_id}, {"$set": {"password": generate_password_hash(nueva)}})
    if r.matched_count == 0:
        return jsonify({"error": "Usuario no encontrado"}), 404
    return jsonify({"msg": "Contraseña reseteada"}), 200


# -------------------------
# POKÉMON DEL USUARIO
# -------------------------

@app.get("/usuarios/<id>/pokemon")
@token_required
def pokemon_de_usuario(current_user, id):
    """Lista todos los Pokémon de un usuario concreto."""
    lista = list(pokemon_user_col.find({"UserId": id}))
    for p in lista:
        p["_id"] = str(p["_id"])
    return jsonify(lista), 200


@app.get("/usuarios/mis_pokemon")
@token_required
def mis_pokemon(current_user):
    """Lista los Pokémon del usuario autenticado."""
    user_id = str(current_user["_id"])
    lista   = list(pokemon_user_col.find({"UserId": user_id}))
    for p in lista:
        p["_id"] = str(p["_id"])
    return jsonify(lista), 200


@app.post("/pokemon/obtener")
@token_required
def obtener_pokemon(current_user):
    """
    Lógica de obtención / subida de nivel / evolución.
    Body: { pokemon_id, nombre, tipo1, tipo2, current_hp }
    Replica la lógica de PokemonUserService.ObtenerPokemon del cliente .NET.
    """
    try:
        data       = request.json or {}
        pokemon_id = int(data.get("pokemon_id"))
        nombre     = data.get("nombre", "")
        tipo1      = data.get("tipo1", "")
        tipo2      = data.get("tipo2", "")
        current_hp = int(data.get("current_hp", 0))
        user_id    = str(current_user["_id"])
        username   = current_user.get("username", "")

        existente = pokemon_user_col.find_one({"UserId": user_id, "PokemonId": pokemon_id})

        resultado = {
            "movimiento_aprendido":               None,
            "movimiento_aprendido_directamente":  False,
            "movimiento_evolucion":               None,
            "movimiento_evolucion_directamente":  False,
            "evoluciono":                         False,
            "nombre_evolucion":                   None,
            "pokemon":                            None
        }

        # ---------- NUEVO ----------
        if existente is None:
            pokedex_doc = pokedex_col.find_one({"numero_pokedex": pokemon_id})
            moveset = []
            if pokedex_doc and pokedex_doc.get("movimientos"):
                for m in pokedex_doc["movimientos"]:
                    if m.get("metodo") == "nivel" and m.get("nivel") == 1 and len(moveset) < 4:
                        moveset.append(m["nombre"])

            nuevo = {
                "UserId":           user_id,
                "Username":         username,
                "PokemonId":        pokemon_id,
                "numero_pokedex":   pokemon_id,
                "Nombre":           nombre,
                "TipoPrincipal":    tipo1,
                "TipoSecundario":   tipo2,
                "Nivel":            1,
                "Cantidad":         1,
                "FechaObtenido":    datetime.datetime.utcnow().isoformat(),
                "HiddenPowerSeed":  random.randint(0, 15),
                "HiddenPowerPower": (random.randint(31, 70) + random.randint(31, 70)) // 2,
                "CurrentHp":        current_hp,
                "MoveSet":          moveset,
                "AbilityId":        None,
                "ItemId":           None,
                "Status":           None
            }
            pokemon_user_col.insert_one(nuevo)
            _recalcular_pokes(user_id)
            nuevo["_id"] = str(nuevo["_id"])
            resultado["pokemon"] = nuevo
            return jsonify(resultado), 201

        # ---------- EXISTENTE: subida de nivel ----------
        id_original = existente["PokemonId"]
        existente["Cantidad"] += 1
        existente["Nivel"]    += 1
        existente["Username"]  = username

        if existente.get("HiddenPowerSeed", 0) == 0 and existente.get("HiddenPowerPower", 0) == 0:
            existente["HiddenPowerSeed"]  = random.randint(0, 15)
            existente["HiddenPowerPower"] = (random.randint(31, 70) + random.randint(31, 70)) // 2
        if existente.get("CurrentHp", 0) == 0:
            existente["CurrentHp"] = current_hp
        if existente.get("MoveSet") is None:
            existente["MoveSet"] = []

        pokedex_doc = pokedex_col.find_one({"numero_pokedex": pokemon_id})

        # 1. Movimiento nuevo por nivel
        if pokedex_doc and pokedex_doc.get("movimientos"):
            mov_nuevo = next(
                (m for m in pokedex_doc["movimientos"]
                 if m.get("metodo") == "nivel"
                 and m.get("nivel") == existente["Nivel"]
                 and m["nombre"] not in existente["MoveSet"]),
                None
            )
            if mov_nuevo:
                resultado["movimiento_aprendido"] = mov_nuevo["nombre"]
                if len(existente["MoveSet"]) < 4:
                    existente["MoveSet"].append(mov_nuevo["nombre"])
                    resultado["movimiento_aprendido_directamente"] = True
                else:
                    resultado["movimiento_aprendido_directamente"] = False

        # 2. Evolución
        evo = pokedex_doc.get("evolucion") if pokedex_doc else None
        if (evo and evo.get("metodo") == "subida_nivel"
                and evo.get("nivel") is not None
                and existente["Nivel"] >= evo["nivel"]):

            datos_evo = pokedex_col.find_one({"nombre": evo["nombre"]})
            existente["Nombre"]       = evo["nombre"]
            existente["PokemonId"]    = datos_evo["numero_pokedex"] if datos_evo else existente["PokemonId"]
            existente["numero_pokedex"] = existente["PokemonId"]

            if datos_evo and datos_evo.get("tipos"):
                tipos = datos_evo["tipos"]
                existente["TipoPrincipal"]  = tipos[0] if len(tipos) > 0 else existente["TipoPrincipal"]
                existente["TipoSecundario"] = tipos[1] if len(tipos) > 1 else None
            if datos_evo and datos_evo.get("estadisticas_base"):
                existente["CurrentHp"] = datos_evo["estadisticas_base"].get("ps", existente["CurrentHp"])

            resultado["evoluciono"]       = True
            resultado["nombre_evolucion"] = evo["nombre"]

            if datos_evo and datos_evo.get("movimientos"):
                mov_evo = next(
                    (m for m in datos_evo["movimientos"]
                     if m.get("metodo") == "nivel"
                     and m.get("nivel") == 1
                     and m["nombre"] not in existente["MoveSet"]),
                    None
                )
                if mov_evo:
                    resultado["movimiento_evolucion"] = mov_evo["nombre"]
                    if len(existente["MoveSet"]) < 4:
                        existente["MoveSet"].append(mov_evo["nombre"])
                        resultado["movimiento_evolucion_directamente"] = True
                    else:
                        resultado["movimiento_evolucion_directamente"] = False

        # Persistir usando id original como filtro
        pk_id = existente.pop("_id")
        pokemon_user_col.replace_one(
            {"UserId": user_id, "PokemonId": id_original},
            existente
        )
        existente["_id"] = str(pk_id)
        _recalcular_pokes(user_id)
        resultado["pokemon"] = existente
        return jsonify(resultado), 200

    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error en el servidor"}), 500


@app.put("/pokemon/movimiento")
@token_required
def aplicar_movimiento(current_user):
    """
    Sustituye un movimiento del moveset de un Pokémon.
    Body: { pokemon_id, indice_a_borrar (-1 para añadir), movimiento_nuevo }
    """
    try:
        data          = request.json or {}
        pokemon_id    = int(data.get("pokemon_id"))
        indice        = int(data.get("indice_a_borrar", -1))
        mov_nuevo     = data.get("movimiento_nuevo", "")
        user_id       = str(current_user["_id"])

        if not mov_nuevo:
            return jsonify({"error": "Falta movimiento_nuevo"}), 400

        poke = pokemon_user_col.find_one({"UserId": user_id, "PokemonId": pokemon_id})
        if not poke:
            return jsonify({"error": "Pokémon no encontrado"}), 404

        moveset = poke.get("MoveSet", [])
        if 0 <= indice < len(moveset):
            moveset[indice] = mov_nuevo
        else:
            if len(moveset) < 4:
                moveset.append(mov_nuevo)
            else:
                return jsonify({"error": "El moveset está lleno, indica un índice a reemplazar"}), 400

        pokemon_user_col.update_one(
            {"UserId": user_id, "PokemonId": pokemon_id},
            {"$set": {"MoveSet": moveset}}
        )
        return jsonify({"msg": "Movimiento aplicado", "moveset": moveset}), 200

    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error en el servidor"}), 500


def _recalcular_pokes(user_id):
    count = pokemon_user_col.count_documents({"UserId": user_id})
    usuarios.update_one({"_id": ObjectId(user_id)}, {"$set": {"pokes": count}})


# -------------------------
# MEDALLAS
# -------------------------

@app.post("/medallas/otorgar")
@token_required
def otorgar_medalla(current_user):
    """Otorga una medalla al usuario autenticado si no la tiene ya."""
    data    = request.json or {}
    tipo    = data.get("tipo", "").strip()
    user_id = str(current_user["_id"])

    if not tipo:
        return jsonify({"error": "Falta el tipo de medalla"}), 400

    existente = medallas_col.find_one({"UserId": user_id, "Tipo": tipo})
    if existente:
        return jsonify({"error": "El usuario ya tiene esta medalla"}), 409

    medallas_col.insert_one({
        "UserId": user_id,
        "Tipo":   tipo,
        "Fecha":  datetime.datetime.utcnow().isoformat()
    })
    return jsonify({"msg": f"Medalla '{tipo}' otorgada correctamente"}), 201


@app.get("/medallas")
@token_required
def mis_medallas(current_user):
    """Lista las medallas del usuario autenticado."""
    user_id = str(current_user["_id"])
    lista   = list(medallas_col.find({"UserId": user_id}))
    for m in lista:
        m["_id"] = str(m["_id"])
    return jsonify(lista), 200


# -------------------------
# CASINO
# -------------------------

SYMBOLS = ["Bar", "Meowth", "Koffing", "Arbok", "Cherry", "Seven"]

PAYOUTS = {
    "Seven":  300,
    "Bar":    100,
    "Meowth":  15,
    "Koffing": 15,
    "Arbok":   15,
    "Cherry":   8
}


def comprobar_ganar(tablero, apuesta):
    lineas       = []
    payout_total = 0

    fila0 = tablero[0][0] == tablero[1][0] == tablero[2][0]
    fila1 = tablero[0][1] == tablero[1][1] == tablero[2][1]
    fila2 = tablero[0][2] == tablero[1][2] == tablero[2][2]
    diag1 = tablero[0][0] == tablero[1][1] == tablero[2][2]
    diag2 = tablero[0][2] == tablero[1][1] == tablero[2][0]

    if apuesta >= 1 and fila1:
        lineas.append(SYMBOLS[tablero[1][1]])
    if apuesta == 3 and fila0:
        lineas.append(SYMBOLS[tablero[1][0]])
    if apuesta == 3 and fila2:
        lineas.append(SYMBOLS[tablero[1][2]])
    if apuesta >= 2 and diag1:
        lineas.append(SYMBOLS[tablero[1][1]])
    if apuesta >= 2 and diag2:
        lineas.append(SYMBOLS[tablero[1][1]])

    for simbolo in lineas:
        payout_total += PAYOUTS.get(simbolo, 0)

    return payout_total, lineas


@app.post("/casino/jugar")
@token_required
def jugar(current_user):
    try:
        data    = request.json or {}
        apuesta = int(data.get("apuesta", 1))
        tablero = data.get("tablero")

        if apuesta not in [1, 2, 3]:
            return jsonify({"error": "Apuesta inválida (1, 2 o 3)"}), 400
        if not tablero:
            return jsonify({"error": "Falta el tablero"}), 400
        if current_user["fichas"] < apuesta:
            return jsonify({"error": "No tienes fichas suficientes"}), 400

        payout, lineas  = comprobar_ganar(tablero, apuesta)
        fichas_final    = current_user["fichas"] - apuesta + payout

        usuarios.update_one(
            {"_id": current_user["_id"]},
            {"$set": {"fichas": fichas_final}}
        )

        return jsonify({
            "tablero":          tablero,
            "simbolos":         SYMBOLS,
            "apuesta":          apuesta,
            "payout":           payout,
            "lineas_ganadoras": lineas,
            "fichas_final":     fichas_final
        }), 200

    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error en el servidor"}), 500


# -------------------------
# PREMIOS / TIENDA
# -------------------------

@app.get("/premios")
@token_required
def listar_premios(current_user):
    lista = list(premios_col.find({}, {"_id": 0}))
    return jsonify(lista), 200


@app.post("/premios/comprar/<int:pokemon_id>")
@token_required
def comprar_pokemon(current_user, pokemon_id):
    premio  = premios_col.find_one({"pokemon_id": pokemon_id})
    if not premio:
        return jsonify({"error": "Pokémon no disponible como premio"}), 404

    if current_user["fichas"] < premio["precio"]:
        return jsonify({"error": "No tienes suficientes fichas"}), 400

    poke_data = pokedex_col.find_one({"numero_pokedex": pokemon_id})
    if not poke_data:
        return jsonify({"error": "Pokémon no encontrado en la Pokédex"}), 404

    user_id  = str(current_user["_id"])
    username = current_user.get("username", "")
    tipos    = poke_data.get("tipos", [])

    nuevo_pokemon = {
        "UserId":           user_id,
        "Username":         username,
        "PokemonId":        pokemon_id,
        "numero_pokedex":   pokemon_id,
        "Nombre":           poke_data.get("nombre", ""),
        "TipoPrincipal":    tipos[0] if len(tipos) > 0 else "",
        "TipoSecundario":   tipos[1] if len(tipos) > 1 else None,
        "Nivel":            1,
        "Cantidad":         1,
        "FechaObtenido":    datetime.datetime.utcnow().isoformat(),
        "HiddenPowerSeed":  random.randint(0, 15),
        "HiddenPowerPower": (random.randint(31, 70) + random.randint(31, 70)) // 2,
        "CurrentHp":        poke_data.get("estadisticas_base", {}).get("ps", 0),
        "MoveSet":          [],
        "AbilityId":        None,
        "ItemId":           None,
        "Status":           None
    }

    # Añadir movimientos de nivel 1
    for m in poke_data.get("movimientos", []):
        if m.get("metodo") == "nivel" and m.get("nivel") == 1 and len(nuevo_pokemon["MoveSet"]) < 4:
            nuevo_pokemon["MoveSet"].append(m["nombre"])

    pokemon_user_col.insert_one(nuevo_pokemon)
    usuarios.update_one(
        {"_id": current_user["_id"]},
        {"$inc": {"fichas": -premio["precio"]}}
    )
    _recalcular_pokes(user_id)

    nuevo_pokemon["_id"] = str(nuevo_pokemon["_id"])
    return jsonify({"msg": "Pokémon obtenido", "pokemon": nuevo_pokemon}), 200


# -------------------------
# POKÉDEX
# -------------------------

@app.get("/pokedex")
@token_required
def get_pokedex(current_user):
    data = list(pokedex_col.find({}, {"_id": 0}))
    return jsonify(data), 200


@app.get("/pokedex/<int:pokemon_id>")
@token_required
def get_pokemon(current_user, pokemon_id):
    doc = pokedex_col.find_one({"numero_pokedex": pokemon_id}, {"_id": 0})
    if not doc:
        return jsonify({"error": "Pokémon no encontrado"}), 404
    return jsonify(doc), 200


# -------------------------
# MAIN
# -------------------------

if __name__ == "__main__":
    app.run(debug=True)
