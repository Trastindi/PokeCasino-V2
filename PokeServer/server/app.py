from pyexpat.errors import messages

from flask import Flask, request, jsonify
from flask_cors import CORS
from pymongo.mongo_client import MongoClient
from pymongo.server_api import ServerApi
from pymongo.errors import OperationFailure
import bcrypt
import jwt
import datetime
import random
import string
import os
from functools import wraps
from bson import ObjectId

app = Flask(__name__)
CORS(app)
app.config["SECRET_KEY"] = "super_secret_key"  # cambiar en producción

# ---------------------------------------------------------------------------
# UTILIDADES
# ---------------------------------------------------------------------------

def gf(doc, *keys, default=None):
    """Devuelve el valor del primer key que exista en el documento."""
    if doc is None:
        return default
    for k in keys:
        if k in doc:
            return doc[k]
    return default


def hash_password(plain: str) -> str:
    return bcrypt.hashpw(plain.encode("utf-8"), bcrypt.gensalt()).decode("utf-8")


def verify_password(plain: str, hashed: str) -> bool:
    try:
        return bcrypt.checkpw(plain.encode("utf-8"), hashed.encode("utf-8"))
    except Exception:
        return False


def _random_password(n=10) -> str:
    return "".join(random.choice(string.ascii_letters + string.digits) for _ in range(n))


def _serialize(doc):
    """Elimina campos sensibles y convierte _id a str."""
    if doc is None:
        return None
    doc = dict(doc)
    doc["_id"] = str(doc["_id"])
    doc.pop("password", None)
    doc.pop("Password", None)
    return doc


# ---------------------------------------------------------------------------
# MONGODB
# ---------------------------------------------------------------------------

_uri = os.environ.get(
    "MONGO_URI",
    "mongodb+srv://marcosemiliorodriguezmartin_db_user:gDfjWHYHIqMJ346V@pokecasino.asaeily.mongodb.net/?retryWrites=true&w=majority&appName=PokeCasino"
)

try:
    _client          = MongoClient(_uri, server_api=ServerApi("1"))
    _db              = _client["PokemonDB"]
    naturalezas      = _db["Naturalezas"]
    menssages        = _db["Menssages"]
    movimientos      = _db["Movimientos"]
    objetos_pokemon  = _db["ObjetosPoke"]
    pokedex          = _db["Pokedex"]
    maquinas_ocultas = _db["MaquinasOcultas"]
    lideres_gimnasio = _db["LideresGimnasio"]
    habilidades      = _db["Habilidades"]
    objetos_evo      = _db["ObjetosEvo"]
    tabla_tipos      = _db["TablaTipos"]
    historico_tiradas = _db["HistoricoTiradas"]
    battles          = _db["Battles"]
    usuarios         = _db["Users"]
    pokemon_user     = _db["PokemonUser"]
    zonas            = _db["Zonas"]
    tipos            = _db["Tipos"]
    maquinas_tecnicas = _db["MaquinasTecnicas"]
    print("Colecciones:", _db.list_collection_names())
except OperationFailure as e:
    print(f"ERROR MongoDB: {e}")
    exit(1)


# ---------------------------------------------------------------------------
# JWT / DECORADORES
# ---------------------------------------------------------------------------

def _make_token(user_id, rol: str) -> str:
    payload = {
        "user_id": str(user_id),
        "rol":     rol,
        "exp":     datetime.datetime.utcnow() + datetime.timedelta(hours=2),
    }
    return jwt.encode(payload, app.config["SECRET_KEY"], algorithm="HS256")


def token_required(f):
    @wraps(f)
    def wrapper(*args, **kwargs):
        header = request.headers.get("Authorization", "")
        if not header.startswith("Bearer "):
            return jsonify({"error": "Token requerido"}), 401
        token = header.split(" ", 1)[1]
        try:
            data = jwt.decode(token, app.config["SECRET_KEY"], algorithms=["HS256"])
            user = usuarios.find_one({"_id": ObjectId(data["user_id"])})
            if not user:
                return jsonify({"error": "Usuario no encontrado"}), 401
        except jwt.ExpiredSignatureError:
            return jsonify({"error": "Token expirado"}), 401
        except Exception:
            return jsonify({"error": "Token inválido"}), 401
        return f(user, *args, **kwargs)
    return wrapper


def admin_required(f):
    @wraps(f)
    def wrapper(current_user, *args, **kwargs):
        if gf(current_user, "rol", "Role", default="") not in ("admin", "Admin"):
            return jsonify({"error": "Permisos insuficientes"}), 403
        return f(current_user, *args, **kwargs)
    return wrapper


# ---------------------------------------------------------------------------
# BÚSQUEDA DE USUARIO (compatible esquema antiguo y nuevo)
# ---------------------------------------------------------------------------

def _find_user(user_input: str):
    q = user_input.lower()
    return usuarios.find_one({
        "$or": [
            {"email_lower":    q},
            {"username_lower": q},
            {"Correo":   {"$regex": f"^{q}$", "$options": "i"}},
            {"Username": {"$regex": f"^{q}$", "$options": "i"}},
        ]
    })


# ---------------------------------------------------------------------------
# AUTH
# ---------------------------------------------------------------------------

@app.post("/auth/register")
def register():
    data     = request.json or {}
    username = data.get("username", "").strip()
    email    = data.get("email",    "").strip()
    password = data.get("password", "")

    if not username or not email or not password:
        return jsonify({"error": "Faltan campos obligatorios"}), 400

    if _find_user(username) or _find_user(email):
        return jsonify({"error": "El usuario o email ya están registrados"}), 400

    doc = {
        "username":       username,
        "username_lower": username.lower(),
        "nombre":         data.get("nombre",   ""),
        "apellido":       data.get("apellido", ""),
        "edad":           int(data.get("edad", 0)),
        "email":          email,
        "email_lower":    email.lower(),
        "password":       hash_password(password),
        "rol":            "usuario",
        "fichas":         300,
        "pokes":          0,
        "medallas":       [],
    }
    usuarios.insert_one(doc)
    return jsonify({"mensaje": "Usuario registrado correctamente"}), 201


@app.post("/auth/login")
def login():
    try:
        data       = request.json or {}
        user_input = (data.get("email") or data.get("username") or "").strip()
        password   = data.get("password", "")

        if not user_input or not password:
            return jsonify({"error": "Faltan credenciales"}), 400

        usuario = _find_user(user_input)
        if not usuario:
            return jsonify({"error": "Usuario o contraseña incorrectos"}), 401

        pwd_hash = gf(usuario, "password", "Password", default="")
        if not pwd_hash or not verify_password(password, pwd_hash):
            return jsonify({"error": "Usuario o contraseña incorrectos"}), 401

        rol      = gf(usuario, "rol",    "Role",         default="usuario")
        fichas   = gf(usuario, "fichas", "FichasCasino", default=0)
        pokes    = gf(usuario, "pokes",  "Pokemon",      default=0)
        uname    = gf(usuario, "username", "Username",   default="")
        email    = gf(usuario, "email",    "Correo",      default="")
        medallas = usuario.get("medallas", [])

        return jsonify({
            "mensaje":  "Login correcto",
            "token":    _make_token(usuario["_id"], rol),
            "id":       str(usuario["_id"]),
            "username": uname,
            "email":    email,
            "rol":      rol,
            "fichas":   fichas,
            "pokes":    pokes,
            "medallas": medallas,
        }), 200

    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500


@app.get("/auth/me")
@token_required
def me(current_user):
    return jsonify(_serialize(current_user)), 200


# ---------------------------------------------------------------------------
# CONTRASEÑA
# ---------------------------------------------------------------------------

@app.post("/auth/cambiar_password")
@token_required
def cambiar_password(current_user):
    data   = request.json or {}
    actual = data.get("password_actual", "")
    nueva  = data.get("nueva_password",  "")

    if not actual or not nueva:
        return jsonify({"error": "Faltan campos"}), 400

    if not verify_password(actual, gf(current_user, "password", "Password", default="")):
        return jsonify({"error": "Contraseña actual incorrecta"}), 401

    if len(nueva) < 6:
        return jsonify({"error": "La nueva contraseña debe tener al menos 6 caracteres"}), 400

    campo = "Password" if "Password" in current_user else "password"
    usuarios.update_one({"_id": current_user["_id"]}, {"$set": {campo: hash_password(nueva)}})
    return jsonify({"mensaje": "Contraseña actualizada"}), 200


@app.post("/auth/recuperar_password")
def recuperar_password():
    data     = request.json or {}
    email    = data.get("email",    "").strip()
    username = data.get("username", "").strip()

    if not email or not username:
        return jsonify({"error": "Se requieren email y username"}), 400

    usuario = _find_user(email)
    if not usuario:
        return jsonify({"error": "No se encontró ningún usuario con ese email"}), 404

    if gf(usuario, "username", "Username", default="").lower() != username.lower():
        return jsonify({"error": "El username no coincide con el email"}), 400

    nueva = _random_password()
    campo = "Password" if "Password" in usuario else "password"
    usuarios.update_one({"_id": usuario["_id"]}, {"$set": {campo: hash_password(nueva)}})
    print(f"[DEV] Reset para {gf(usuario,'email','Correo')}: {nueva}")
    return jsonify({"msg": "Contraseña reseteada", "nueva_password": nueva}), 200


# ---------------------------------------------------------------------------
# USUARIOS (CRUD)
# ---------------------------------------------------------------------------

@app.get("/usuarios")
@token_required
@admin_required
def listar_usuarios(current_user):
    docs = list(usuarios.find())
    return jsonify([_serialize(d) for d in docs]), 200


@app.get("/usuarios/<id>")
@token_required
def obtener_usuario(current_user, id):
    try:
        doc = usuarios.find_one({"_id": ObjectId(id)})
    except Exception:
        return jsonify({"error": "ID inválido"}), 400
    if not doc:
        return jsonify({"error": "Usuario no encontrado"}), 404
    return jsonify(_serialize(doc)), 200


@app.post("/usuarios")
@token_required
@admin_required
def crear_usuario(current_user):
    data = request.json or {}
    if not data.get("username") or not data.get("email") or not data.get("password"):
        return jsonify({"error": "Faltan campos obligatorios"}), 400

    if _find_user(data["username"]) or _find_user(data["email"]):
        return jsonify({"error": "Usuario o email ya registrados"}), 400

    doc = {
        "username":       data["username"],
        "username_lower": data["username"].lower(),
        "nombre":         data.get("nombre",   ""),
        "apellido":       data.get("apellido", ""),
        "edad":           int(data.get("edad", 0)),
        "email":          data["email"],
        "email_lower":    data["email"].lower(),
        "password":       hash_password(data["password"]),
        "rol":            data.get("rol", "usuario"),
        "fichas":         int(data.get("fichas", 300)),
        "pokes":          0,
        "medallas":       [],
    }
    usuarios.insert_one(doc)
    return jsonify({"msg": "Usuario creado"}), 201


# Mapeo PascalCase (cliente C#/Kotlin) -> snake_case (MongoDB)
_USER_FIELD_MAP = {
    "Nombre":       "nombre",
    "Apellido":     "apellido",
    "Correo":       "email",
    "Username":     "username",
    "Role":         "rol",
    "FichasCasino": "fichas",
    "Pokes":        "pokes",
    "Edad":         "edad",
    # snake_case nativo (Android / admin)
    "nombre":    "nombre",
    "apellido":  "apellido",
    "email":     "email",
    "username":  "username",
    "rol":       "rol",
    "fichas":    "fichas",
    "pokes":     "pokes",
    "edad":      "edad",
}

@app.put("/usuarios/<id>")
@token_required
def modificar_usuario(current_user, id):
    try:
        oid = ObjectId(id)
    except Exception:
        return jsonify({"error": "ID inválido"}), 400

    data   = request.json or {}
    update = {_USER_FIELD_MAP[k]: data[k] for k in data if k in _USER_FIELD_MAP}

    if not update:
        return jsonify({"error": "No se enviaron datos para actualizar"}), 400

    if "email"    in update: update["email_lower"]    = update["email"].lower()
    if "username" in update: update["username_lower"] = update["username"].lower()

    r = usuarios.update_one({"_id": oid}, {"$set": update})
    if r.matched_count == 0:
        return jsonify({"error": "Usuario no encontrado"}), 404
    return jsonify({"msg": "Usuario actualizado"}), 200


@app.delete("/usuarios/<id>")
@token_required
@admin_required
def eliminar_usuario(current_user, id):
    try:
        r = usuarios.delete_one({"_id": ObjectId(id)})
    except Exception:
        return jsonify({"error": "ID inválido"}), 400
    if r.deleted_count == 0:
        return jsonify({"error": "Usuario no encontrado"}), 404
    return jsonify({"msg": "Usuario eliminado"}), 200


@app.put("/usuarios/<id>/reset_password")
@token_required
@admin_required
def reset_password(current_user, id):
    try:
        oid = ObjectId(id)
    except Exception:
        return jsonify({"error": "ID inválido"}), 400

    usuario = usuarios.find_one({"_id": oid})
    if not usuario:
        return jsonify({"error": "Usuario no encontrado"}), 404

    nueva = (request.json or {}).get("password") or _random_password()
    campo = "Password" if "Password" in usuario else "password"
    usuarios.update_one({"_id": oid}, {"$set": {campo: hash_password(nueva)}})
    print(f"[DEV] Reset para {gf(usuario,'email','Correo')}: {nueva}")
    return jsonify({"msg": "Contraseña reseteada"}), 200


# ---------------------------------------------------------------------------
# POKÉMON DEL USUARIO
# ---------------------------------------------------------------------------

def _recalcular_pokes(user_id: str):
    n = pokemon_user.count_documents({"UserId": user_id})
    usuarios.update_one({"_id": ObjectId(user_id)}, {"$set": {"pokes": n}})


@app.get("/usuarios/<id>/pokemon")
@token_required
def pokemon_del_usuario(current_user, id):
    lista = list(pokemon_user.find({"UserId": id}))
    return jsonify([{**p, "_id": str(p["_id"])} for p in lista]), 200


@app.get("/usuarios/mis_pokemon")
@token_required
def mis_pokemon(current_user):
    uid   = str(current_user["_id"])
    lista = list(pokemon_user.find({"UserId": uid}))
    return jsonify([{**p, "_id": str(p["_id"])} for p in lista]), 200


@app.post("/pokemon/obtener")
@token_required
def obtener_pokemon(current_user):
    try:
        data       = request.json or {}
        pokemon_id = int(data.get("pokemon_id"))
        nombre     = data.get("nombre",     "")
        tipo1      = data.get("tipo1",      "")
        tipo2      = data.get("tipo2",      "")
        current_hp = int(data.get("current_hp", 0))
        uid        = str(current_user["_id"])
        uname      = gf(current_user, "username", "Username", default="")

        existente = pokemon_user.find_one({"UserId": uid, "PokemonId": pokemon_id})

        resultado = {
            "movimiento_aprendido":              None,
            "movimiento_aprendido_directamente": False,
            "movimiento_evolucion":              None,
            "movimiento_evolucion_directamente": False,
            "evoluciono":      False,
            "nombre_evolucion": None,
            "pokemon":          None,
        }

        # ---------- NUEVO POKÉMON ----------
        if existente is None:
            pdex    = pokedex.find_one({"numero_pokedex": pokemon_id})
            moveset = []
            if pdex:
                for m in pdex.get("movimientos", []):
                    if m.get("metodo") == "nivel" and m.get("nivel") == 1 and len(moveset) < 4:
                        moveset.append(m["nombre"])
            nuevo = {
                "UserId": uid, "Username": uname,
                "PokemonId": pokemon_id, "numero_pokedex": pokemon_id,
                "Nombre": nombre, "TipoPrincipal": tipo1, "TipoSecundario": tipo2,
                "Nivel": 1, "Cantidad": 1,
                "FechaObtenido": datetime.datetime.utcnow().isoformat(),
                "HiddenPowerSeed":  random.randint(0, 15),
                "HiddenPowerPower": (random.randint(31, 70) + random.randint(31, 70)) // 2,
                "CurrentHp": current_hp, "MoveSet": moveset,
                "AbilityId": None, "ItemId": None, "Status": None,
            }
            pokemon_user.insert_one(nuevo)
            _recalcular_pokes(uid)
            nuevo["_id"] = str(nuevo["_id"])
            resultado["pokemon"] = nuevo
            return jsonify(resultado), 201

        # ---------- SUBIDA DE NIVEL ----------
        id_orig = existente["PokemonId"]
        existente["Cantidad"]  += 1
        existente["Nivel"]     += 1
        existente["Username"]   = uname
        if not existente.get("HiddenPowerSeed"):
            existente["HiddenPowerSeed"]  = random.randint(0, 15)
            existente["HiddenPowerPower"] = (random.randint(31, 70) + random.randint(31, 70)) // 2
        if not existente.get("CurrentHp"):
            existente["CurrentHp"] = current_hp
        if existente.get("MoveSet") is None:
            existente["MoveSet"] = []

        pdex = pokedex.find_one({"numero_pokedex": pokemon_id})

        if pdex:
            mov = next((m for m in pdex.get("movimientos", [])
                        if m.get("metodo") == "nivel"
                        and m.get("nivel") == existente["Nivel"]
                        and m["nombre"] not in existente["MoveSet"]), None)
            if mov:
                resultado["movimiento_aprendido"] = mov["nombre"]
                if len(existente["MoveSet"]) < 4:
                    existente["MoveSet"].append(mov["nombre"])
                    resultado["movimiento_aprendido_directamente"] = True

        evo = pdex.get("evolucion") if pdex else None
        if (evo and evo.get("metodo") == "subida_nivel"
                and evo.get("nivel") is not None
                and existente["Nivel"] >= evo["nivel"]):

            datos_evo = pokedex.find_one({"nombre": evo["nombre"]})
            existente["Nombre"]         = evo["nombre"]
            existente["PokemonId"]      = datos_evo["numero_pokedex"] if datos_evo else existente["PokemonId"]
            existente["numero_pokedex"] = existente["PokemonId"]
            if datos_evo:
                tipos_evo = datos_evo.get("tipos", [])
                existente["TipoPrincipal"]  = tipos_evo[0] if tipos_evo else existente["TipoPrincipal"]
                existente["TipoSecundario"] = tipos_evo[1] if len(tipos_evo) > 1 else None
                existente["CurrentHp"]      = datos_evo.get("estadisticas_base", {}).get("ps", existente["CurrentHp"])
            resultado["evoluciono"]       = True
            resultado["nombre_evolucion"] = evo["nombre"]

            if datos_evo:
                mov_evo = next((m for m in datos_evo.get("movimientos", [])
                                if m.get("metodo") == "nivel" and m.get("nivel") == 1
                                and m["nombre"] not in existente["MoveSet"]), None)
                if mov_evo:
                    resultado["movimiento_evolucion"] = mov_evo["nombre"]
                    if len(existente["MoveSet"]) < 4:
                        existente["MoveSet"].append(mov_evo["nombre"])
                        resultado["movimiento_evolucion_directamente"] = True

        pk_id = existente.pop("_id")
        pokemon_user.replace_one({"UserId": uid, "PokemonId": id_orig}, existente)
        existente["_id"] = str(pk_id)
        _recalcular_pokes(uid)
        resultado["pokemon"] = existente
        return jsonify(resultado), 200

    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500


# ---------------------------------------------------------------------------
# MOVIMIENTOS
# ---------------------------------------------------------------------------

@app.put("/pokemon/movimiento")
@token_required
def actualizar_movimiento(current_user):
    try:
        data       = request.json or {}
        pokemon_id = int(data.get("pokemon_id"))
        slot       = int(data.get("slot", 0))
        movimiento = data.get("movimiento", "")
        uid        = str(current_user["_id"])

        if slot < 0 or slot > 3:
            return jsonify({"error": "Slot inválido (0-3)"}), 400

        poke = pokemon_user.find_one({"UserId": uid, "PokemonId": pokemon_id})
        if not poke:
            return jsonify({"error": "Pokémon no encontrado"}), 404

        moveset = poke.get("MoveSet") or []
        while len(moveset) <= slot:
            moveset.append(None)
        moveset[slot] = movimiento

        pokemon_user.update_one({"UserId": uid, "PokemonId": pokemon_id}, {"$set": {"MoveSet": moveset}})
        return jsonify({"msg": "Movimiento actualizado", "moveset": moveset}), 200

    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno"}), 500


# ---------------------------------------------------------------------------
# MEDALLAS
# ---------------------------------------------------------------------------

@app.post("/medallas/otorgar")
@token_required
def otorgar_medalla(current_user):
    data     = request.json or {}
    user_id  = data.get("user_id")
    medalla  = data.get("medalla")

    if not user_id or not medalla:
        return jsonify({"error": "Faltan datos"}), 400

    try:
        usuarios.update_one(
            {"_id": ObjectId(user_id)},
            {"$addToSet": {"medallas": medalla}}
        )
    except Exception:
        return jsonify({"error": "ID inválido"}), 400

    return jsonify({"msg": f"Medalla '{medalla}' otorgada"}), 200


@app.get("/medallas")
@token_required
def mis_medallas(current_user):
    medallas = current_user.get("medallas", [])
    return jsonify({"medallas": medallas}), 200


# ---------------------------------------------------------------------------
# CASINO
# ---------------------------------------------------------------------------

@app.post("/casino/jugar")
@token_required
def jugar_casino(current_user):
    try:
        uid    = str(current_user["_id"])
        fichas = gf(current_user, "fichas", "FichasCasino", default=0)

        if fichas < 1:
            return jsonify({"error": "No tienes fichas suficientes"}), 400

        simbolos  = ["🍒", "🍋", "🔔", "⭐", "7️⃣"]
        resultado = [random.choice(simbolos) for _ in range(3)]
        ganancia  = 0

        if resultado[0] == resultado[1] == resultado[2]:
            ganancia = 10 if resultado[0] == "7️⃣" else 5

        nuevo_saldo = fichas - 1 + ganancia
        campo_fichas = "fichas" if "fichas" in current_user else "FichasCasino"
        usuarios.update_one({"_id": current_user["_id"]}, {"$set": {campo_fichas: nuevo_saldo}})

        return jsonify({
            "resultado": resultado,
            "ganancia":  ganancia,
            "fichas":    nuevo_saldo,
        }), 200

    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno"}), 500


# ---------------------------------------------------------------------------
# POKÉDEX
# ---------------------------------------------------------------------------

@app.get("/pokedex")
@token_required
def listar_pokedex(current_user):
    docs = list(pokedex.find({}, {"_id": 0}))
    return jsonify(docs), 200


@app.get("/pokedex/<int:pokemon_id>")
@token_required
def obtener_pokemon_pokedex(current_user, pokemon_id):
    doc = pokedex.find_one({"numero_pokedex": pokemon_id}, {"_id": 0})
    if not doc:
        return jsonify({"error": "Pokémon no encontrado"}), 404
    return jsonify(doc), 200


# ---------------------------------------------------------------------------
# BATALLAS
# ---------------------------------------------------------------------------

@app.post("/battle_requests")
@token_required
def crear_batalla(current_user):
    data = request.json or {}
    uid  = str(current_user["_id"])

    doc = {
        "challenger_id": uid,
        "opponent_id":   data.get("opponent_id"),
        "status":        "pending",
        "created_at":    datetime.datetime.utcnow().isoformat(),
    }
    battles.insert_one(doc)
    doc["_id"] = str(doc["_id"])
    return jsonify(doc), 201


@app.get("/battle_requests")
@token_required
def listar_batallas(current_user):
    uid  = str(current_user["_id"])
    docs = list(battles.find({"$or": [{"challenger_id": uid}, {"opponent_id": uid}]}))
    return jsonify([{**d, "_id": str(d["_id"])} for d in docs]), 200


@app.put("/battle_requests/<id>")
@token_required
def actualizar_batalla(current_user, id):
    data   = request.json or {}
    status = data.get("status")
    if not status:
        return jsonify({"error": "Falta el campo status"}), 400
    try:
        battles.update_one({"_id": ObjectId(id)}, {"$set": {"status": status}})
    except Exception:
        return jsonify({"error": "ID inválido"}), 400
    return jsonify({"msg": "Batalla actualizada"}), 200


# ---------------------------------------------------------------------------
# ZONAS
# ---------------------------------------------------------------------------

@app.get("/zonas")
@token_required
def listar_zonas(current_user):
    docs = list(zonas.find({}, {"_id": 0}))
    return jsonify(docs), 200


@app.get("/zonas/<nombre>")
@token_required
def obtener_zona(current_user, nombre):
    doc = zonas.find_one({"nombre": {"$regex": f"^{nombre}$", "$options": "i"}}, {"_id": 0})
    if not doc:
        return jsonify({"error": "Zona no encontrada"}), 404
    return jsonify(doc), 200


# ---------------------------------------------------------------------------
# HISTORICO TIRADAS
# ---------------------------------------------------------------------------

@app.post("/historico_tiradas")
@token_required
def registrar_tirada(current_user):
    data = request.json or {}
    uid  = str(current_user["_id"])

    doc = {
        "UserId":     uid,
        "PokemonId":  data.get("pokemon_id"),
        "Nombre":     data.get("nombre", ""),
        "Zona":       data.get("zona",   ""),
        "Fecha":      datetime.datetime.utcnow().isoformat(),
    }
    historico_tiradas.insert_one(doc)
    doc["_id"] = str(doc["_id"])
    return jsonify(doc), 201


@app.get("/historico_tiradas")
@token_required
def mis_tiradas(current_user):
    uid  = str(current_user["_id"])
    docs = list(historico_tiradas.find({"UserId": uid}))
    return jsonify([{**d, "_id": str(d["_id"])} for d in docs]), 200


# ---------------------------------------------------------------------------
# MAIN
# ---------------------------------------------------------------------------

if __name__ == "__main__":
    app.run(debug=True)
