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
import string
import os
from functools import wraps
from bson import ObjectId

app = Flask(__name__)
CORS(app)

_secret_key = "secret_key_aleatoria_y_segura_para_jwt"  # Valor por defecto para desarrollo
_type_chart_cache: dict = {}

def _load_type_chart():
    """Carga la tabla de tipos desde MongoDB al arrancar (o si el caché está vacío)."""
    global _type_chart_cache
    if _type_chart_cache:
        return
    for entry in tipo_tabla.find({}, {"_id": 0, "attack_type": 1, "effectiveness": 1}):
        # Normalizar a minúsculas para comparar sin distinción de mayúsculas
        atk = entry["attack_type"].lower()
        _type_chart_cache[atk] = {
            def_type.lower(): mult
            for def_type, mult in entry["effectiveness"].items()
        }

_load_type_chart()

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
        nombre     = data.get("nombre",     "")
        tipo1      = data.get("tipo1",      "")
        tipo2      = data.get("tipo2",      "")
        current_hp = int(data.get("current_hp", 0))
        uid        = str(current_user["_id"])
        uname      = gf(current_user, "Username", "username", default="")

        pdex    = pokedex.find_one({"numero_pokedex": pokemon_id})
        moveset = []
        
        if pdex:
            for m in pdex.get("movimientos", []):
                if m.get("metodo") == "nivel" and m.get("nivel") == 1 and len(moveset) < 4:
                    moveset.append(m["nombre"])
            
            abilities = pdex.get("habilidades", [])                
            print(f"{len(abilities)} habilidades posibles para {nombre} (ID {pokemon_id}): {[h.get('nombre') for h in abilities]}")
            
            if len(abilities) == 2:
                    ability = abilities[random.randint(0, 1)].get("nombre")
            else:
                ability = abilities[0].get("nombre")

        nuevo = {
            "UserId":          uid,
            "Username":        uname,
            "PokemonId":       pokemon_id,
            "numero_pokedex":  pokemon_id,
            "Nombre":          nombre,
            "TipoPrincipal":   tipo1,
            "TipoSecundario":  tipo2,
            "Nivel":           1,
            "Cantidad":        1,
            "FechaObtenido":   datetime.datetime.utcnow().isoformat(),
            "HiddenPowerSeed":  random.randint(0, 15),
            "HiddenPowerPower": (random.randint(31, 70) + random.randint(31, 70)) // 2,
            "CurrentHp":       current_hp,
            "MoveSet":         moveset,
            "AbilityId":       ability,
            "ItemId":          None,
            "Status":          None,
            "Shards":          0,
        }
        pokemon_user.insert_one(nuevo)
        _recalcular_pokes(uid)
        nuevo["_id"] = str(nuevo["_id"])
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
    return jsonify(list(pokedex.find({}, {"_id": 0}))), 200


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
            "player1_id": retador_id,
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
        for t in teams:
            t["_id"] = str(t["_id"])
        return jsonify(teams), 200
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


def batalla_loop(battle_id):
    global batalla
    print(f"\n{GREEN}=== BATALLA INICIADA ==={RESET}")

    while True:
        batalla = obtener_batalla(battle_id)
        status  = batalla.get("status", "")

        # ── Elegir Pokémon inicial / tras KO ─────────────────────────────
        if status == "ready":
            _elegir_pokemon_activo(battle_id)
            continue

        # ── Elegir acción del turno ───────────────────────────────────────
        if status == "choosing_action":
            _mostrar_estado_batalla(batalla)
            _elegir_accion(battle_id)
            print(f"{YELLOW}Esperando al rival...{RESET}")
            # Polling hasta que el turno se resuelva
            while True:
                time.sleep(1.5)
                batalla = obtener_batalla(battle_id)
                if batalla.get("status") in ("choosing_action", "finished"):
                    break
            _mostrar_log_turno(batalla.get("turn_log", []))
            if batalla.get("status") == "finished":
                break
            continue

        if status == "finished":
            break

        time.sleep(1.5)  # esperar si el rival aún no eligió Pokémon

    # ── Resultado final ───────────────────────────────────────────────────
    winner = batalla.get("winner", "")
    uid    = current_user.get("id") or current_user.get("_id", "")
    if winner == uid:
        print(f"\n{GREEN}🏆 ¡Has ganado la batalla!{RESET}")
    else:
        print(f"\n{RED}💀 Has perdido la batalla.{RESET}")


def _elegir_pokemon_activo(battle_id):
    global batalla
    uid     = current_user.get("id") or current_user.get("_id", "")
    my_slot = "player1_team" if batalla.get("player1_id") == uid else "player2_team"
    team    = batalla.get(my_slot, {}).get("pokemon", [])

    print(f"\n{CYAN}Elige tu Pokémon:{RESET}")
    for i, p in enumerate(team):
        hp = p.get("CurrentHp", 0)
        print(f"  {i+1}. {p['Nombre']}  HP: {hp}")

    while True:
        sel = input("Número: ").strip()
        try:
            idx = int(sel) - 1
            if 0 <= idx < len(team) and team[idx].get("CurrentHp", 0) > 0:
                break
            print("Pokémon inválido o sin HP.")
        except ValueError:
            print("Número inválido.")

    r = requests.post(
        f"{API_URL}/battles/{battle_id}/choose_pokemon",
        json={"pokemon_index": idx},
        headers=headers()
    )
    if r.status_code == 200:
        print(f"{GREEN}¡{team[idx]['Nombre']} al campo!{RESET}")
    else:
        print("Error:", r.json().get("error"))


def _elegir_accion(battle_id):
    global batalla
    uid     = current_user.get("id") or current_user.get("_id", "")
    my_slot = "player1_team" if batalla.get("player1_id") == uid else "player2_team"
    my_idx  = batalla.get("player1_active" if my_slot == "player1_team" else "player2_active", 0)
    poke    = batalla.get(my_slot, {}).get("pokemon", [])[my_idx]

    print(f"\n{CYAN}--- {poke['Nombre']} ---  HP: {poke.get('CurrentHp', 0)}{RESET}")
    print("1. Atacar")
    print("2. Cambiar Pokémon")

    op = input("Opción: ").strip()

    if op == "1":
        moves = poke.get("MoveSet", [])
        print(f"\n{CYAN}Movimientos:{RESET}")
        for i, m in enumerate(moves):
            if isinstance(m, dict):
                pp = m.get("pp", {})
                print(f"  {i+1}. {m.get('name','?')}  "
                      f"[{m.get('type','?')}]  "
                      f"Poder: {m.get('powerModel',{}).get('basePower','?')}  "
                      f"PP: {pp.get('base','?')}")
            else:
                print(f"  {i+1}. {m}")

        while True:
            sel = input("Elige movimiento: ").strip()
            try:
                midx = int(sel) - 1
                if 0 <= midx < len(moves):
                    break
                print("Índice inválido.")
            except ValueError:
                print("Número inválido.")

        action = {"type": "move", "move_index": midx}

    elif op == "2":
        team = batalla.get(my_slot, {}).get("pokemon", [])
        print(f"\n{CYAN}Pokémon disponibles:{RESET}")
        for i, p in enumerate(team):
            if i == my_idx:
                continue
            hp = p.get("CurrentHp", 0)
            print(f"  {i+1}. {p['Nombre']}  HP: {hp}")

        while True:
            sel = input("Elige Pokémon: ").strip()
            try:
                pidx = int(sel) - 1
                if 0 <= pidx < len(team) and pidx != my_idx and team[pidx].get("CurrentHp", 0) > 0:
                    break
                print("Selección inválida.")
            except ValueError:
                print("Número inválido.")

        action = {"type": "switch", "pokemon_index": pidx}

    else:
        print("Opción inválida, atacando con movimiento 0.")
        action = {"type": "move", "move_index": 0}

    r = requests.post(
        f"{API_URL}/battles/{battle_id}/action",
        json={"action": action},
        headers=headers()
    )
    if r.status_code != 200:
        print("Error:", r.json().get("error"))


def _mostrar_estado_batalla(batalla):
    uid     = current_user.get("id") or current_user.get("_id", "")
    my_slot = "player1_team" if batalla.get("player1_id") == uid else "player2_team"
    ri_slot = "player2_team" if my_slot == "player1_team" else "player1_team"
    my_idx  = batalla.get("player1_active" if my_slot == "player1_team" else "player2_active", 0)
    ri_idx  = batalla.get("player2_active" if my_slot == "player1_team" else "player1_active", 0)

    my_poke = batalla.get(my_slot, {}).get("pokemon", [])[my_idx]
    ri_poke = batalla.get(ri_slot, {}).get("pokemon", [])[ri_idx]

    print(f"\n{'─'*40}")
    print(f"  Rival: {RED}{ri_poke['Nombre']}{RESET}  HP: {ri_poke.get('CurrentHp', 0)}")
    print(f"  Turno: {batalla.get('turn', 0)}  Campo: {batalla.get('field_status','normal')}")
    print(f"  Tú:    {GREEN}{my_poke['Nombre']}{RESET}  HP: {my_poke.get('CurrentHp', 0)}")
    print(f"{'─'*40}")


def _mostrar_log_turno(log):
    for entry in log:
        ev = entry.get("event")
        if ev == "speed_tie":
            print(f"{YELLOW}⚡ ¡Empate de velocidad! Orden aleatorio.{RESET}")
        elif ev == "attack":
            ef = entry.get("effectiveness", 1.0)
            ef_str = ""
            if ef > 1.5:   ef_str = f" {RED}¡Es muy eficaz!{RESET}"
            elif ef < 0.5: ef_str = f" {BLUE}No es muy eficaz...{RESET}"
            elif ef == 0:  ef_str = f" {WHITE}No afecta...{RESET}"
            crit_str = f" {YELLOW}¡Golpe crítico!{RESET}" if entry.get("crit") else ""
            stab_str = f" {GREEN}(STAB){RESET}" if entry.get("stab") else ""
            boost_str = f" {MAGENTA}(habilidad x{entry['ability_boost']:.1f}){RESET}" if entry.get("ability_boost") else ""
            print(f"  {entry['attacker']} usa {CYAN}{entry['move']}{RESET}{boost_str}  "
                  f"→  {entry['damage']} daño{ef_str}{crit_str}{stab_str}  "
                  f"(HP rival: {entry['remaining_hp']})")
        elif ev == "fainted":
            print(f"  {RED}💀 {entry['pokemon']} se ha debilitado.{RESET}")
        elif ev == "switch":
            print(f"  {GREEN}↔ {entry['player']} saca a {entry['to']}.{RESET}")
        elif ev == "stat_change":
            sign = "bajó" if entry.get("stages", 0) < 0 else "subió"
            print(f"  {MAGENTA}[{entry['ability']}] {entry['pokemon']}'s {entry['stat']} {sign}.{RESET}")
        elif ev == "field_damage":
            print(f"  {YELLOW}[{entry['field']}] {entry['pokemon']} recibe {entry['damage']} de daño.{RESET}")
        elif ev == "status_applied":
            print(f"  {MAGENTA}[{entry['ability']}] {entry['target']} quedó paralizado.{RESET}")

# Caché en memoria para evitar consultas repetidas en cada cálculo



def _type_effectiveness(move_type: str, defender_type: str) -> float:
    """Consulta la tabla de tipos desde caché (cargada de MongoDB)."""
    if not defender_type:
        return 1.0
    if not _type_chart_cache:
        _load_type_chart()
    atk_data = _type_chart_cache.get(move_type.lower(), {})
    return atk_data.get(defender_type.lower(), 1.0)

# ---------------------------------------------------------------------------
# MAIN
# ---------------------------------------------------------------------------

if __name__ == "__main__":
    app.run(debug=True)
