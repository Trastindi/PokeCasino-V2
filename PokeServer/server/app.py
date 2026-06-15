from flask import Flask, request, jsonify
from flask_cors import CORS
from pymongo.mongo_client import MongoClient
from pymongo.server_api import ServerApi
from pymongo.errors import OperationFailure
from dotenv import load_dotenv
import bcrypt
import jwt
import datetime
import random
import math
import string
import os
from functools import wraps
from bson import ObjectId

app = Flask(__name__)
CORS(app)

_secret_key = "secret_key_aleatoria_y_segura_para_jwt"  # Valor por defecto para desarrollo

if not _secret_key:
    raise RuntimeError("SECRET_KEY no está definida. Configúrala en el entorno o en .env")
app.config["SECRET_KEY"] = _secret_key

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
    """Elimina campos sensibles y convierte ObjectId a str recursivamente."""
    if doc is None:
        return None
    doc = dict(doc)
    doc["id"] = str(doc.pop("_id"))
    doc.pop("password", None)
    doc.pop("Password", None)

    # Serializar recursivamente listas y subdocumentos
    for key, value in doc.items():
        if isinstance(value, list):
            doc[key] = [_serialize_value(item) for item in value]
        else:
            doc[key] = _serialize_value(value)

    return doc


def _serialize_value(value):
    """Convierte ObjectId a str y serializa dicts anidados."""
    from bson import ObjectId
    if isinstance(value, ObjectId):
        return str(value)
    if isinstance(value, dict):
        return {k: _serialize_value(v) for k, v in value.items()}
    if isinstance(value, list):
        return [_serialize_value(item) for item in value]
    return value


# ---------------------------------------------------------------------------
# MONGODB
# ---------------------------------------------------------------------------
load_dotenv(dotenv_path="server/.env")
_uri = os.environ.get("MONGO_URI", "")

try:
    _client          = MongoClient(_uri, server_api=ServerApi("1"))
    _db              = _client["PokemonDB"]
    naturalezas      = _db["Naturalezas"]
    mensajes        = _db["Messages"]
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
        if gf(current_user, "Role", "rol", default="") not in ("admin", "Admin"):
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
    nombre   = data.get("nombre",   "").strip()
    apellido = data.get("apellido", "").strip()
    birthdate_str = data.get("birthdate", None)

    if not username or not email or not password:
        return jsonify({"error": "Faltan campos obligatorios"}), 400

    if _find_user(username) or _find_user(email):
        return jsonify({"error": "El usuario o email ya están registrados"}), 400

    try:
        birthdate = datetime.datetime.fromisoformat(birthdate_str) if birthdate_str else datetime.datetime.utcnow()
    except Exception:
        birthdate = datetime.datetime.utcnow()

    doc = {
        "Nombre":         nombre,
        "Apellido":       apellido,
        "Username":       username,
        "username_lower": username.lower(),
        "Correo":         email,
        "email_lower":    email.lower(),
        "Pokemon":        0,
        "Password":       hash_password(password),
        "Birthdate":      birthdate,
        "Role":           "user",
        "Pokes":          300,
        "FichasCasino":   0,
        "Medallas":       [],
        "Messages":       [
            {
                "messageid": 0,
                "foreignid": 0,
                "text": "Welcome",
                "date": datetime.datetime.utcnow(),
                "read": False
            }
        ],
    }
    usuarios.insert_one(doc)
    
    # Devolver el usuario creado con token (como login)
    rol      = "user"
    token    = _make_token(doc["_id"], rol)
    
    return jsonify({
        "mensaje":  "Usuario registrado correctamente",
        "token":    token,
        "id":       str(doc["_id"]),
        "username": username,
        "email":    email,
        "rol":      rol,
        "fichas":   0,
        "pokes":    300,
        "pokemon":  0
    }), 201


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

        pwd_hash = gf(usuario, "Password", "password", default="")
        if not pwd_hash or not verify_password(password, pwd_hash):
            return jsonify({"error": "Usuario o contraseña incorrectos"}), 401

        rol      = gf(usuario, "Role",         "rol",      default="user")
        fichas   = gf(usuario, "FichasCasino", "fichas",   default=0)
        pokes    = gf(usuario, "Pokes",        "pokes",    default=0)
        pokemon  = gf(usuario, "Pokemon",                  default=0)
        uname    = gf(usuario, "Username",     "username", default="")
        email    = gf(usuario, "Correo",       "email",    default="")
        medallas = gf(usuario, "Medallas",     "medallas", default=[])

        return jsonify({
            "mensaje":  "Login correcto",
            "token":    _make_token(usuario["_id"], rol),
            "id":       str(usuario["_id"]),
            "username": uname,
            "email":    email,
            "rol":      rol,
            "fichas":   fichas,
            "pokes":    pokes,
            "pokemon":  pokemon,
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

    if not verify_password(actual, gf(current_user, "Password", "password", default="")):
        return jsonify({"error": "Contraseña actual incorrecta"}), 401

    if len(nueva) < 6:
        return jsonify({"error": "La nueva contraseña debe tener al menos 6 caracteres"}), 400

    usuarios.update_one({"_id": current_user["_id"]}, {"$set": {"Password": hash_password(nueva)}})
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

    if gf(usuario, "Username", "username", default="").lower() != username.lower():
        return jsonify({"error": "El username no coincide con el email"}), 400

    nueva = _random_password()
    usuarios.update_one({"_id": usuario["_id"]}, {"$set": {"Password": hash_password(nueva)}})
    print(f"[DEV] Nueva contraseña para {email}: {nueva}")
    return jsonify({"mensaje": "Se ha enviado una nueva contraseña al correo indicado"}), 200


# ---------------------------------------------------------------------------
# CRUD USUARIOS (admin)
# ---------------------------------------------------------------------------

@app.get("/usuarios")
@token_required
@admin_required
def listar_usuarios(current_user):
    return jsonify([_serialize(u) for u in usuarios.find()]), 200


@app.get("/usuarios/<id>")
@token_required
def obtener_usuario(current_user, id):
    try:
        u = usuarios.find_one({"_id": ObjectId(id)})
    except Exception:
        return jsonify({"error": "ID inválido"}), 400
    if not u:
        return jsonify({"error": "Usuario no encontrado"}), 404
    return jsonify(_serialize(u)), 200


@app.post("/usuarios")
@token_required
@admin_required
def crear_usuario(current_user):
    data = request.json or {}
    for k in ["Nombre", "Apellido", "Username", "Correo", "Password", "Birthdate", "Role"]:
        if k not in data:
            return jsonify({"error": f"Falta el campo '{k}'"}), 400

    if _find_user(data["Correo"]) or _find_user(data["Username"]):
        return jsonify({"error": "Correo o Username ya registrado"}), 400

    try:
        birthdate = datetime.datetime.fromisoformat(data["Birthdate"])
    except Exception:
        return jsonify({"error": "Formato de Birthdate inválido (ISO 8601)"}), 400

    doc = {
        "Nombre":         data["Nombre"],
        "Apellido":       data["Apellido"],
        "Username":       data["Username"],
        "username_lower": data["Username"].lower(),
        "Correo":         data["Correo"],
        "email_lower":    data["Correo"].lower(),
        "Pokemon":        0,
        "Password":       hash_password(data["Password"]),
        "Birthdate":      birthdate,
        "Role":           data["Role"],
        "Pokes":          int(data.get("Pokes", 300)),
        "FichasCasino":   int(data.get("FichasCasino", 0)),
        "Medallas":       [],
        "Messages":       [],
    }
    usuarios.insert_one(doc)
    return jsonify({"msg": "Usuario creado"}), 201


@app.put("/usuarios/<id>")
@token_required
def modificar_usuario(current_user, id):
    try:
        oid = ObjectId(id)
    except Exception:
        return jsonify({"error": "ID inválido"}), 400

    data   = request.json or {}
    update = {k: data[k] for k in ["Nombre", "Apellido", "Correo", "Role", "FichasCasino", "Pokes", "Birthdate", "Username"] if k in data}
    if not update:
        return jsonify({"error": "No se enviaron datos para actualizar"}), 400

    if "Correo"   in update: update["email_lower"]    = update["Correo"].lower()
    if "Username" in update: update["username_lower"] = update["Username"].lower()

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
    usuarios.update_one({"_id": oid}, {"$set": {"Password": hash_password(nueva)}})
    print(f"[DEV] Reset para {gf(usuario,'Correo','email')}: {nueva}")
    return jsonify({"msg": "Contraseña reseteada"}), 200


# ---------------------------------------------------------------------------
# POKÉMON DEL USUARIO
# ---------------------------------------------------------------------------

def _recalcular_pokes(user_id: str):
    """Actualiza Users.Pokemon con el recuento real de documentos en PokemonUser."""
    n = pokemon_user.count_documents({"UserId": user_id})
    usuarios.update_one({"_id": ObjectId(user_id)}, {"$set": {"Pokemon": n}})


@app.get("/usuarios/<id>/pokemon")
@token_required
def pokemon_de_usuario(current_user, id):
    lista = list(pokemon_user.find({"UserId": id}))
    for p in lista: p["_id"] = str(p["_id"])
    return jsonify(lista), 200


@app.get("/usuarios/mis_pokemon")
@token_required
def mis_pokemon(current_user):
    uid   = str(current_user["_id"])
    lista = list(pokemon_user.find({"UserId": uid}))
    for p in lista: p["_id"] = str(p["_id"])
    return jsonify(lista), 200


@app.post("/pokemon/obtener")
@token_required
def obtener_pokemon(current_user):
    """
    Siempre crea un nuevo documento PokemonUser a nivel 1.
    Nunca sube de nivel ni evoluciona: cada tirada es una carta nueva.
    El campo 'Shards' queda reservado para uso futuro.
    """
    try:
        data       = request.json or {}
        pokemon_id = int(data.get("pokemon_id"))
        uid        = str(current_user["_id"])
        uname      = gf(current_user, "Username", "username", default="")

        pdex = pokedex.find_one({"numero_pokedex": pokemon_id})
        if not pdex:
            return jsonify({"error": "Pokémon no encontrado en la Pokédex"}), 404

        nombre = data.get("nombre") or pdex.get("Nombre", "")
        tipo1  = data.get("tipo1") or pdex.get("Tipo1") or pdex.get("tipo_1", "")
        tipo2  = data.get("tipo2") or pdex.get("Tipo2") or pdex.get("tipo_2", "")

        estadisticas_base = pdex.get("estadisticas_base", {}) or {}
        current_hp = int(estadisticas_base.get("ps", 0))

        moveset = []
        for m in pdex.get("movimientos", []):
            if m.get("metodo") == "nivel" and m.get("nivel") == 1 and len(moveset) < 4:
                moveset.append(m["nombre"])

        abilities = pdex.get("habilidades", [])
        ability = None
        if abilities:
            print(f"{len(abilities)} habilidades posibles para {nombre} (ID {pokemon_id}): {[h.get('nombre') for h in abilities]}")
            if len(abilities) == 2:
                ability = abilities[random.randint(0, 1)].get("nombre")
            else:
                ability = abilities[0].get("nombre")

        nuevo = {
            "UserId": uid,
            "Username": uname,
            "PokemonId": pokemon_id,
            "numero_pokedex": pokemon_id,
            "Nombre": nombre,
            "TipoPrincipal": tipo1,
            "TipoSecundario": tipo2,
            "Nivel": 1,
            "Cantidad": 1,
            "FechaObtenido": datetime.datetime.utcnow().isoformat(),
            "HiddenPowerSeed": random.randint(0, 15),
            "HiddenPowerPower": (random.randint(31, 70) + random.randint(31, 70)) // 2,
            "estadisticas_base": estadisticas_base,
            "CurrentHp": current_hp,
            "MoveSet": moveset,
            "AbilityId": ability,
            "ItemId": None,
            "Status": None,
            "Shards": 0,
        }

        result = pokemon_user.insert_one(nuevo)
        _recalcular_pokes(uid)
        nuevo["_id"] = str(result.inserted_id)
        return jsonify(nuevo), 201

    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500

@app.put("/pokemon/movimiento")
@token_required
def aplicar_movimiento(current_user):
    try:
        data       = request.json or {}
        pokemon_id = int(data.get("pokemon_id"))
        indice     = int(data.get("indice_a_borrar", -1))
        mov_nuevo  = data.get("movimiento_nuevo", "")
        uid        = str(current_user["_id"])

        if not mov_nuevo:
            return jsonify({"error": "Falta movimiento_nuevo"}), 400

        poke = pokemon_user.find_one({"UserId": uid, "PokemonId": pokemon_id})
        if not poke:
            return jsonify({"error": "Pokémon no encontrado"}), 404

        moveset = poke.get("MoveSet", [])
        if 0 <= indice < len(moveset):
            moveset[indice] = mov_nuevo
        elif len(moveset) < 4:
            moveset.append(mov_nuevo)
        else:
            return jsonify({"error": "Moveset lleno; indica un índice a reemplazar"}), 400

        pokemon_user.update_one({"UserId": uid, "PokemonId": pokemon_id}, {"$set": {"MoveSet": moveset}})
        return jsonify({"msg": "Movimiento aplicado", "moveset": moveset}), 200

    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500

@app.get("/movimiento/<movimiento_name>")
def obtener_datos_movimiento(movimiento_name):
    """Devuelve los datos de un movimiento dado su nombre."""
    mov = movimientos.find_one({"name": movimiento_name})
    if not mov:
        return jsonify({"error": "Movimiento no encontrado"}), 404
    return jsonify(_serialize(mov)), 200

#Ver detalles de un Pokémon específico del usuario (para mostrar en la sección "Mis Pokémon" o en la selección de equipo para batalla)
@app.get("/pokemon/<pokemon_id>")
@token_required
def ver_pokemon(current_user, pokemon_id):
    uid = str(current_user["_id"])
    poke = pokemon_user.find_one({"UserId": uid, "PokemonId": int(pokemon_id)})
    if not poke:
        return jsonify({"error": "Pokémon no encontrado"}), 404
    poke["_id"] = str(poke["_id"])
    return jsonify(poke), 200

# ---------------------------------------------------------------------------
# MEDALLAS  (almacenadas en Users.Medallas como lista de strings)
# ---------------------------------------------------------------------------

@app.post("/medallas/otorgar")
@token_required
def otorgar_medalla(current_user):
    tipo = (request.json or {}).get("tipo", "").strip()
    if not tipo:
        return jsonify({"error": "Falta el tipo de medalla"}), 400
    medallas_actuales = gf(current_user, "Medallas", "medallas", default=[])
    if tipo in medallas_actuales:
        return jsonify({"error": "El usuario ya tiene esta medalla"}), 409
    usuarios.update_one({"_id": current_user["_id"]}, {"$push": {"Medallas": tipo}})
    return jsonify({"msg": f"Medalla '{tipo}' otorgada"}), 201


@app.get("/medallas")
@token_required
def mis_medallas(current_user):
    return jsonify(gf(current_user, "Medallas", "medallas", default=[])), 200


# ---------------------------------------------------------------------------
# CASINO
# ---------------------------------------------------------------------------

SYMBOLS = ["Bar", "Meowth", "Koffing", "Arbok", "Cherry", "Seven"]
PAYOUTS = {"Seven": 300, "Bar": 100, "Meowth": 15, "Koffing": 15, "Arbok": 15, "Cherry": 8}


def _comprobar_ganar(tablero, apuesta):
    lineas = []
    f0 = tablero[0][0] == tablero[1][0] == tablero[2][0]
    f1 = tablero[0][1] == tablero[1][1] == tablero[2][1]
    f2 = tablero[0][2] == tablero[1][2] == tablero[2][2]
    d1 = tablero[0][0] == tablero[1][1] == tablero[2][2]
    d2 = tablero[0][2] == tablero[1][1] == tablero[2][0]
    if apuesta >= 1 and f1: lineas.append(SYMBOLS[tablero[1][1]])
    if apuesta == 3 and f0: lineas.append(SYMBOLS[tablero[1][0]])
    if apuesta == 3 and f2: lineas.append(SYMBOLS[tablero[1][2]])
    if apuesta >= 2 and d1: lineas.append(SYMBOLS[tablero[1][1]])
    if apuesta >= 2 and d2: lineas.append(SYMBOLS[tablero[1][1]])
    return sum(PAYOUTS.get(s, 0) for s in lineas), lineas


@app.post("/casino/jugar")
@token_required
def jugar(current_user):
    try:
        data    = request.json or {}
        apuesta = int(data.get("apuesta", 1))
        tablero = data.get("tablero")

        if apuesta not in (1, 2, 3):
            return jsonify({"error": "Apuesta inválida (1, 2 o 3)"}), 400
        if not tablero:
            return jsonify({"error": "Falta el tablero"}), 400

        fichas = gf(current_user, "FichasCasino", "fichas", default=0)
        if fichas < apuesta:
            return jsonify({"error": "No tienes fichas suficientes"}), 400

        payout, lineas = _comprobar_ganar(tablero, apuesta)
        fichas_final   = fichas - apuesta + payout
        usuarios.update_one({"_id": current_user["_id"]}, {"$set": {"FichasCasino": fichas_final}})

        return jsonify({"tablero": tablero, "simbolos": SYMBOLS, "apuesta": apuesta,
                        "payout": payout, "lineas_ganadoras": lineas,
                        "fichas_final": fichas_final}), 200
    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500


# ---------------------------------------------------------------------------
# POKÉDEX
# ---------------------------------------------------------------------------

@app.get("/pokedex")
@token_required
def get_pokedex(current_user):
    return jsonify(list(pokedex.find({}, {"_id": 0}).sort("numero_pokedex", 1))), 200


@app.get("/pokedex/<int:pokemon_id>")
@token_required
def get_pokemon(current_user, pokemon_id):
    doc = pokedex.find_one({"numero_pokedex": pokemon_id}, {"_id": 0})
    if not doc:
        return jsonify({"error": "Pokémon no encontrado"}), 404
    return jsonify(doc), 200


# ---------------------------------------------------------------------------
# HISTORIAL DE TIRADAS
# ---------------------------------------------------------------------------

@app.post("/historico")
@token_required
def registrar_tirada(current_user):
    try:
        data = request.json or {}
        doc = {
            "UserId":        data.get("UserId",        str(current_user["_id"])),
            "PokemonId":     data.get("PokemonId",     0),
            "NombrePokemon": data.get("NombrePokemon", ""),
            "Zona":          data.get("Zona",          ""),
            "TipoTirada":    data.get("TipoTirada",    "single"),
            "Fecha":         datetime.datetime.utcnow().isoformat(),
        }
        historico_tiradas.insert_one(doc)
        doc["_id"] = str(doc["_id"])
        return jsonify(doc), 201
    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500


@app.get("/historico/<user_id>")
@token_required
def obtener_historico(current_user, user_id):
    try:
        lista = list(
            historico_tiradas
            .find({"UserId": user_id})
            .sort("Fecha", -1)
        )
        for t in lista:
            t["_id"] = str(t["_id"])
        return jsonify(lista), 200
    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500


# ---------------------------------------------------------------------------
# SOLICITUDES DE BATALLA
# ---------------------------------------------------------------------------

@app.post("/battle_requests/<rival_id>")
@token_required
def make_battle_request(current_user, rival_id):
    try:
        rival = usuarios.find_one({"_id": ObjectId(rival_id)})
        if not rival:
            return jsonify({"error": "Rival no encontrado"}), 404

        doc = {
            "_id":     ObjectId(),
            "from":    str(gf(current_user, "Username", "username", default="")),
            "from_id": str(current_user["_id"]),
            "to":      str(rival_id),
            "title":   "Battle Request",
            "text":    "You have received a battle request from "
                       + gf(current_user, "Username", "username", default="")
                       + ". Do you accept?",
            "Fecha":   datetime.datetime.utcnow().isoformat(),
            "type":    "battle_request",
            "responded": False,
        }
        mensajes.insert_one(doc)
        doc["_id"] = str(doc["_id"])
        return jsonify(doc), 201
    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500


@app.post("/battle_requests/<msg_id>/respond")
@token_required
def respond_battle_request(current_user, msg_id):
    """
    Acepta o rechaza una solicitud de batalla.
    - Si acepta: genera un battle_id placeholder y manda mensaje
      type=battle_response al retador con ese ID.
    - Si rechaza: manda mensaje type=battle_rejected al retador.
    - Marca el mensaje original como responded=True.
    El battle_id será reemplazado por el ID real cuando exista el endpoint de batalla.
    """
    try:
        data     = request.json or {}
        accepted = bool(data.get("accepted", False))

        # Verificar que el mensaje existe, es para este usuario y no fue ya respondido
        msg = mensajes.find_one({
            "_id":       ObjectId(msg_id),
            "to":        str(current_user["_id"]),
            "type":      "battle_request",
            "responded": False,
        })
        if not msg:
            return jsonify({"error": "Solicitud no encontrada o ya respondida"}), 404

        # Marcar como respondida
        mensajes.update_one(
            {"_id": ObjectId(msg_id)},
            {"$set": {"responded": True, "accepted": accepted}}
        )

        retador_id = msg.get("from_id", "")

        if not accepted:
            mensajes.insert_one({
                "_id":   ObjectId(),
                "from":  str(gf(current_user, "Username", "username", default="?")),
                "from_id": str(current_user["_id"]),
                "to":    retador_id,
                "title": "Battle Rejected",
                "text":  gf(current_user, "Username", "username", default="?")
                         + " ha rechazado tu solicitud de batalla.",
                "Fecha": datetime.datetime.utcnow().isoformat(),
                "type":  "battle_rejected",
                "responded": False,
            })
            return jsonify({"msg": "Solicitud rechazada"}), 200

        # ── ACEPTADO ────────────────────────────────────────────────────────
        # TODO: sustituir por battle_id real cuando exista el endpoint de batalla
        battle_id = str(ObjectId())

        mensajes.insert_one({
            "_id":       ObjectId(),
            "from":      str(gf(current_user, "Username", "username", default="?")),
            "to":        retador_id,
            "title":     "Battle Accepted",
            "text":      gf(current_user, "Username", "username", default="?")
                         + " ha aceptado tu solicitud de batalla.",
            "Fecha":     datetime.datetime.utcnow().isoformat(),
            "type":      "battle_response",
            "battle_id": battle_id,
            "responded": False,
        })
        battles.insert_one({
            "_id": ObjectId(battle_id),
            "player1_id": str(retador_id),
            "player2_id": str(current_user["_id"]),
            "status": "pending",
            "created_at": datetime.datetime.utcnow().isoformat(),
            "player1_team": {},
            "player2_team": {},
            "turn": 0,
            "field_status": "normal",
        })
        return jsonify({"msg": "Batalla aceptada", "battle_id": battle_id}), 200

    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500

@app.get("/battles/<battle_id>")
@token_required
def get_battle_status(current_user, battle_id):
    try:
        battle = battles.find_one({"_id": ObjectId(battle_id)})
        if not battle:
            return jsonify({"error": "Batalla no encontrada"}), 404
        battle["_id"] = str(battle["_id"])
        return jsonify(battle), 200
    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500

@app.post("/battles/<battle_id>/teams")
@token_required
def submit_battle_team(current_user, battle_id):
    try:
        data    = request.json or {}
        team_id = data.get("team_id", "").strip()

        if not team_id:
            return jsonify({"error": "Falta team_id"}), 400

        # ── 1. Obtener la batalla ────────────────────────────────────────────
        battle = battles.find_one({"_id": ObjectId(battle_id)})
        if not battle:
            return jsonify({"error": "Batalla no encontrada"}), 404

        uid = str(current_user["_id"])
        if battle["player1_id"] != uid and battle["player2_id"] != uid:
            return jsonify({"error": "No eres parte de esta batalla"}), 403

        # ── 2. Obtener el equipo del usuario ─────────────────────────────────
        result = usuarios.find_one(
            {"_id": current_user["_id"], "PokemonTeams._id": ObjectId(team_id)},
            {"PokemonTeams.$": 1, "_id": 0}
        )
        if not result or not result.get("PokemonTeams"):
            return jsonify({"error": "Equipo no encontrado"}), 404

        team_doc    = result["PokemonTeams"][0]
        pokemon_ids = team_doc.get("pokemon_ids", [])

        # ── 3. Enriquecer cada Pokémon ────────────────────────────────────────
        enriched_pokemon = []
        for poke_id in pokemon_ids:
            poke = pokemon_user.find_one({"_id": ObjectId(poke_id), "UserId": uid})
            if not poke:
                continue  # si ya no existe en la colección, lo saltamos

            # Expandir movimientos
            moveset_expanded = []
            for move_name in poke.get("MoveSet", []):
                mov_doc = movimientos.find_one({"name": move_name})
                if mov_doc:
                    moveset_expanded.append(_serialize_value(mov_doc))
                else:
                    moveset_expanded.append(move_name)  # fallback al nombre

            # Expandir habilidad
            ability_doc = None
            ability_id  = poke.get("AbilityId")
            if ability_id:
                ability_doc = habilidades.find_one({"name": ability_id})

            poke_enriched = _serialize_value(dict(poke))
            poke_enriched["MoveSet"] = moveset_expanded
            poke_enriched["Ability"] = _serialize_value(ability_doc) if ability_doc else []

            enriched_pokemon.append(poke_enriched)

        battle_team = {
            "_id":      str(team_doc.get("_id", "")),
            "team_name": team_doc.get("team_name", ""),
            "pokemon":  enriched_pokemon,
        }

        # ── 4. Guardar en la batalla ──────────────────────────────────────────
        if battle["player1_id"] == uid:
            battles.update_one({"_id": ObjectId(battle_id)}, {"$set": {"player1_team": battle_team}})
        else:
            battles.update_one({"_id": ObjectId(battle_id)}, {"$set": {"player2_team": battle_team}})

        # Marcar como ready si ambos enviaron equipo
        battle_updated = battles.find_one({"_id": ObjectId(battle_id)})
        if battle_updated["player1_team"] and battle_updated["player2_team"]:
            battles.update_one({"_id": ObjectId(battle_id)}, {"$set": {"status": "ready"}})

        return jsonify({"msg": "Equipo enviado", "team": battle_team}), 200

    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500


# ---------------------------------------------------------------------------
# BATALLA — ELEGIR POKÉMON ACTIVO
# ---------------------------------------------------------------------------

@app.post("/battles/<battle_id>/choose_pokemon")
@token_required
def choose_pokemon(current_user, battle_id):
    try:
        data  = request.json or {}
        idx   = data.get("pokemon_index")
        if idx is None:
            return jsonify({"error": "Falta pokemon_index"}), 400

        battle = battles.find_one({"_id": ObjectId(battle_id)})
        if not battle:
            return jsonify({"error": "Batalla no encontrada"}), 404

        uid = str(current_user["_id"])
        if battle["player1_id"] != uid and battle["player2_id"] != uid:
            return jsonify({"error": "No eres parte de esta batalla"}), 403

        if battle.get("status") != "ready":
            return jsonify({"error": f"Estado incorrecto: {battle.get('status')}"}), 409

        slot      = "player1_active" if battle["player1_id"] == uid else "player2_active"
        team_slot = "player1_team"   if battle["player1_id"] == uid else "player2_team"
        team      = battle.get(team_slot, {}).get("pokemon", [])

        if not (0 <= int(idx) < len(team)):
            return jsonify({"error": "Índice de Pokémon inválido"}), 400
        if team[int(idx)].get("CurrentHp", 0) <= 0:
            return jsonify({"error": "Ese Pokémon no tiene HP"}), 400

        battles.update_one(
            {"_id": ObjectId(battle_id)},
            {"$set": {slot: int(idx)}}
        )

        # Si ambos jugadores ya eligieron Pokémon activo → pasar a choosing_action
        battle_updated = battles.find_one({"_id": ObjectId(battle_id)})
        p1_ready = battle_updated.get("player1_active") is not None
        p2_ready = battle_updated.get("player2_active") is not None
        if p1_ready and p2_ready:
            battles.update_one(
                {"_id": ObjectId(battle_id)},
                {"$set": {"status": "choosing_action"}}
            )

        return jsonify({"msg": "Pokémon activo seleccionado", "index": int(idx)}), 200

    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500


# ---------------------------------------------------------------------------
# BATALLA — ELEGIR ACCIÓN (movimiento o cambio)
# ---------------------------------------------------------------------------

@app.post("/battles/<battle_id>/action")
@token_required
def battle_action(current_user, battle_id):
    try:
        data   = request.json or {}
        action = data.get("action", {})

        battle = battles.find_one({"_id": ObjectId(battle_id)})
        if not battle:
            return jsonify({"error": "Batalla no encontrada"}), 404

        uid = str(current_user["_id"])
        if battle["player1_id"] != uid and battle["player2_id"] != uid:
            return jsonify({"error": "No eres parte de esta batalla"}), 403

        if battle.get("status") != "choosing_action":
            return jsonify({"error": f"Estado incorrecto: {battle.get('status')}"}), 409

        action_slot = "player1_action" if battle["player1_id"] == uid else "player2_action"
        battles.update_one(
            {"_id": ObjectId(battle_id)},
            {"$set": {action_slot: action}}
        )

        # Si ambos enviaron acción → resolver turno
        battle_updated = battles.find_one({"_id": ObjectId(battle_id)})
        if battle_updated.get("player1_action") and battle_updated.get("player2_action"):
            _resolver_turno(battle_id, battle_updated)

        return jsonify({"msg": "Acción registrada"}), 200

    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500


# ---------------------------------------------------------------------------
# TABLA DE EFECTIVIDAD DE TIPOS (cacheada en memoria)
# ---------------------------------------------------------------------------

_tipo_cache: dict = {}  # {"atacante/defensor": 0.5 | 1.0 | 2.0}

def _efectividad_tipo(tipo_ataque: str, tipo_defensor: str) -> float:
    """Devuelve la efectividad (0, 0.5, 1 o 2) consultando TablaTipos en MongoDB."""
    if not tipo_ataque or not tipo_defensor:
        return 1.0
    clave = f"{tipo_ataque}/{tipo_defensor}"
    if clave in _tipo_cache:
        return _tipo_cache[clave]
    doc = tabla_tipos.find_one({
        "$or": [
            {"attacking_type": tipo_ataque, "defending_type": tipo_defensor},
            {"ataque": tipo_ataque, "defensa": tipo_defensor},
        ]
    })
    if doc:
        ef = float(doc.get("effectiveness", doc.get("efectividad", 1.0)))
    else:
        ef = 1.0
    _tipo_cache[clave] = ef
    return ef


# ---------------------------------------------------------------------------
# FÓRMULA DE DAÑO GEN III
# ---------------------------------------------------------------------------

def _aplicar_dano(atacante, defensor, movimiento, field_status="normal"):
    """
    Fórmula Gen III completa (Bulbapedia):

    damage = floor(
        floor( floor( floor((2*L/5)+2) * Power * A/D ) / 50 + 2 )
        * Burn * Screen * Targets * Weather * FF
        * Stockpile * Critical * DoubleDmg * Charge * HH
        * STAB * Type1 * Type2
        * random[85..100] / 100
    )

    Esta implementación cubre los modificadores aplicables a un combate 1v1:
    Burn, Weather, STAB, Type1, Type2, Critical y random.
    Los modificadores de dobles (Screen dobles, Targets, HH) usan sus valores
    por defecto (1) al ser una batalla individual.
    """
    if isinstance(movimiento, str):
        return 0

    categoria = movimiento.get("damage_class", "physical")
    potencia  = int(movimiento.get("power") or 0)
    if potencia == 0:
        return 0

    stats_atk = atacante.get("estadisticas_base") or {}
    stats_def = defensor.get("estadisticas_base") or {}

    if categoria == "special":
        A = int(stats_atk.get("sp_ataque", stats_atk.get("ataque_especial", 50)))
        D = int(stats_def.get("sp_defensa", stats_def.get("defensa_especial", 50)))
    else:
        A = int(stats_atk.get("ataque", 50))
        D = int(stats_def.get("defensa", 50))

    if D == 0:
        D = 1

    nivel = int(atacante.get("Nivel", 1))

    # ── Base damage ─────────────────────────────────────────────────────────
    base = math.floor(
        math.floor(
            math.floor((2 * nivel / 5) + 2) * potencia * A / D
        ) / 50
    ) + 2

    # ── Burn ────────────────────────────────────────────────────────────────
    # 0.5 si el atacante está quemado Y el movimiento es físico
    status_atk = (atacante.get("Status") or "").lower()
    burn = 0.5 if (status_atk == "quemado" or status_atk == "burn") and categoria == "physical" else 1

    # ── Weather ─────────────────────────────────────────────────────────────
    tipo_mov = (movimiento.get("type") or movimiento.get("tipo") or "").lower()
    campo    = (field_status or "normal").lower()
    if campo == "lluvia" or campo == "rain":
        weather = 1.5 if tipo_mov == "agua" or tipo_mov == "water" else (
                  0.5 if tipo_mov == "fuego" or tipo_mov == "fire"  else 1)
    elif campo == "sol" or campo == "sun" or campo == "harsh sunlight":
        weather = 1.5 if tipo_mov == "fuego" or tipo_mov == "fire" else (
                  0.5 if tipo_mov == "agua"  or tipo_mov == "water" else 1)
    elif campo not in ("none", "normal", "despejado", "clear"):
        # Granizo, tormenta de arena, etc.
        weather = 0.5 if (tipo_mov == "solar" or tipo_mov == "rayo solar") else 1
    else:
        weather = 1

    # ── STAB ────────────────────────────────────────────────────────────────
    tipo1_atk = (atacante.get("TipoPrincipal")  or "").lower()
    tipo2_atk = (atacante.get("TipoSecundario") or "").lower()
    stab = 1.5 if tipo_mov and tipo_mov in (tipo1_atk, tipo2_atk) else 1

    # ── Efectividad de tipos ─────────────────────────────────────────────────
    tipo1_def = (defensor.get("TipoPrincipal")  or "").title()
    tipo2_def = (defensor.get("TipoSecundario") or "")
    tipo_ataque_titulo = tipo_mov.title() if tipo_mov else ""

    type1_eff = _efectividad_tipo(tipo_ataque_titulo, tipo1_def)
    type2_eff = _efectividad_tipo(tipo_ataque_titulo, tipo2_def) if tipo2_def else 1.0

    # ── Crítico ─────────────────────────────────────────────────────────────
    # Probabilidad Gen III: 1/16 por defecto
    critical = 2 if random.randint(1, 16) == 1 else 1

    # ── Random [85..100] / 100 ──────────────────────────────────────────────
    rand = random.randint(85, 100) / 100

    # ── Daño final ──────────────────────────────────────────────────────────
    dano = math.floor(
        base * burn * weather * stab * type1_eff * type2_eff * critical * rand
    )
    dano = max(1, dano)

    # ── Aplicar al defensor ──────────────────────────────────────────────────
    hp_actual = int(defensor.get("CurrentHp", 0))
    defensor["CurrentHp"] = max(0, hp_actual - dano)

    return dano, critical == 2


def _resolver_turno(battle_id: str, battle: dict):
    """Resuelve las acciones de ambos jugadores, actualiza HP y estado en la BD."""
    log = []

    p1_team  = battle["player1_team"]["pokemon"]
    p2_team  = battle["player2_team"]["pokemon"]
    p1_idx   = battle.get("player1_active", 0)
    p2_idx   = battle.get("player2_active", 0)

    p1_action = battle.get("player1_action", {})
    p2_action = battle.get("player2_action", {})

    field_status = battle.get("field_status", "normal")

    def get_speed(poke):
        stats = poke.get("estadisticas_base") or {}
        return int(stats.get("velocidad", 0))

    # Determinar orden de ataque (mayor velocidad primero; empate → aleatorio)
    p1_speed = get_speed(p1_team[p1_idx])
    p2_speed = get_speed(p2_team[p2_idx])
    if p1_speed > p2_speed:
        orden = [("p1", p1_action, p1_team, p1_idx, p2_team, p2_idx),
                 ("p2", p2_action, p2_team, p2_idx, p1_team, p1_idx)]
    elif p2_speed > p1_speed:
        orden = [("p2", p2_action, p2_team, p2_idx, p1_team, p1_idx),
                 ("p1", p1_action, p1_team, p1_idx, p2_team, p2_idx)]
    else:
        orden_base = [("p1", p1_action, p1_team, p1_idx, p2_team, p2_idx),
                      ("p2", p2_action, p2_team, p2_idx, p1_team, p1_idx)]
        random.shuffle(orden_base)
        orden = orden_base

    winner = None

    for jugador, accion, equipo_atk, idx_atk, equipo_def, idx_def in orden:
        atacante = equipo_atk[idx_atk]
        defensor = equipo_def[idx_def]

        tipo_accion = accion.get("type", "move")

        if tipo_accion == "switch":
            nuevo_idx = int(accion.get("pokemon_index", idx_atk))
            if jugador == "p1":
                p1_idx = nuevo_idx
            else:
                p2_idx = nuevo_idx
            log.append(f"{jugador} cambia al Pokémon #{nuevo_idx}")

        elif tipo_accion == "move":
            mov_index = int(accion.get("move_index", 0))
            moveset   = atacante.get("MoveSet", [])
            if mov_index < len(moveset):
                movimiento = moveset[mov_index]
                resultado  = _aplicar_dano(atacante, defensor, movimiento, field_status)
                if resultado:
                    dano, es_critico = resultado
                else:
                    dano, es_critico = 0, False

                mov_nombre = movimiento.get("name", movimiento) if isinstance(movimiento, dict) else movimiento
                entrada_log = f"{jugador} usa {mov_nombre} → {dano} de daño"
                if es_critico:
                    entrada_log += " (¡Golpe crítico!)"
                log.append(entrada_log)

                # Comprobar KO del defensor
                if defensor.get("CurrentHp", 0) <= 0:
                    log.append(f"{'p2' if jugador == 'p1' else 'p1'} Pokémon #{idx_def} ha sido derrotado")

                    # Comprobar si todos los Pokémon del equipo defensor están a 0 HP
                    equipo_vivo = any(p.get("CurrentHp", 0) > 0 for p in equipo_def)
                    if not equipo_vivo:
                        winner = battle["player1_id"] if jugador == "p1" else battle["player2_id"]
                        break

    # Construir update de la batalla
    update = {
        "player1_team.pokemon": p1_team,
        "player2_team.pokemon": p2_team,
        "player1_active": p1_idx,
        "player2_active": p2_idx,
        "player1_action": None,
        "player2_action": None,
        "turn": battle.get("turn", 0) + 1,
        "last_turn_log": log,
    }

    if winner:
        update["status"] = "finished"
        update["winner"] = winner
    else:
        update["status"] = "choosing_action"

    battles.update_one({"_id": ObjectId(battle_id)}, {"$set": update})


# ---------------------------------------------------------------------------
# MENSAJES DEL USUARIO
# ---------------------------------------------------------------------------

@app.get("/messages/mis_mensajes")
@token_required
def get_messages(current_user):
    try:
        uid = str(current_user["_id"])
        lista = list(mensajes.find({"to": uid}).sort("Fecha", -1))
        for m in lista:
            m["_id"] = str(m["_id"])
        return jsonify(lista), 200
    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500


# ---------------------------------------------------------------------------
# EQUIPOS DE POKÉMON
# ---------------------------------------------------------------------------

@app.get("/users/pokemonteams")
@token_required
def get_pokemon_teams(current_user):
    try:
        teams = list(current_user.get("PokemonTeams", []))
        serialized = [_serialize_value(t) for t in teams]
        return jsonify(serialized), 200
    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500


@app.post("/users/pokemonteams")
@token_required
def create_pokemon_team(current_user):
    try:
        data        = request.json or {}
        team_name   = data.get("team_name", "").strip()
        pokemon_ids = data.get("pokemon_ids", [])

        if not team_name:
            return jsonify({"error": "Falta el nombre del equipo"}), 400
        if not isinstance(pokemon_ids, list) or len(pokemon_ids) == 0:
            return jsonify({"error": "pokemon_ids debe ser una lista no vacía"}), 400
        if len(pokemon_ids) > 6:
            return jsonify({"error": "Un equipo no puede tener más de 6 Pokémon"}), 400

        new_team = {
            "_id": ObjectId(),
            "user_id": current_user["_id"],
            "team_name": team_name,
            "pokemon_ids": pokemon_ids,
            "created_at": datetime.datetime.utcnow().isoformat(),
        }
        usuarios.update_one(
            {"_id": current_user["_id"]},
            {"$push": {"PokemonTeams": new_team}}
        )
        new_team["_id"] = str(new_team["_id"])
        new_team["user_id"] = str(new_team["user_id"])
        return jsonify(new_team), 201
    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500


@app.put("/users/pokemonteams/<team_id>")
@token_required
def update_pokemon_team(current_user, team_id):
    try:
        data        = request.json or {}
        pokemon_ids = data.get("pokemon_ids")

        if pokemon_ids is None:
            return jsonify({"error": "Falta pokemon_ids"}), 400
        if not isinstance(pokemon_ids, list):
            return jsonify({"error": "pokemon_ids debe ser una lista"}), 400
        if len(pokemon_ids) > 6:
            return jsonify({"error": "Un equipo no puede tener más de 6 Pokémon"}), 400

        result = usuarios.update_one(
            {"_id": current_user["_id"], "PokemonTeams._id": ObjectId(team_id)},
            {"$set": {"PokemonTeams.$.pokemon_ids": pokemon_ids}}
        )
        if result.matched_count == 0:
            return jsonify({"error": "Equipo no encontrado"}), 404
        return jsonify({"msg": "Equipo actualizado"}), 200
    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Error interno del servidor"}), 500

@app.get("/users/pokemonteams/<team_id>")
@token_required
def get_team(current_user, team_id):
    try:
        result = usuarios.find_one(
            {
                "_id": current_user["_id"],
                "PokemonTeams._id": ObjectId(team_id)
            },
            {
                "PokemonTeams.$": 1,
                "_id": 0
            }
        )

        if not result or not result.get("PokemonTeams"):
            return jsonify({"error": "Equipo no encontrado"}), 404

        team = result["PokemonTeams"][0]
        team["_id"] = str(team["_id"])
        return jsonify(team["pokemon_ids"]), 200

    except Exception:
        import traceback; traceback.print_exc()
        return jsonify({"error": "Equipo no encontrado"}), 404

@app.get("/users/pokemonteams/<pokemon_id>/moveset")
@token_required
def team_clone(current_user, pokemon_id):
    try:
        pokemon = pokemon_user.find_one({
            "_id": ObjectId(pokemon_id),
            "UserId": str(current_user["_id"])
        })

        if not pokemon:
            return jsonify({"error": "Pokémon no encontrado"}), 404

        moveset = pokemon.get("MoveSet", [])

        return jsonify({"moveset": moveset}), 200

    except Exception:
        return jsonify({"error": "error al extraer los movimientos"}), 500


# ---------------------------------------------------------------------------
# MAIN
# ---------------------------------------------------------------------------

if __name__ == "__main__":
    app.run(debug=True)
