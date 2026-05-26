from flask import Flask, request, jsonify
from flask_cors import CORS
from pymongo.mongo_client import MongoClient
from pymongo.server_api import ServerApi
import uuid
import jwt
import datetime
from functools import wraps
from bson import ObjectId
import random
from werkzeug.security import generate_password_hash, check_password_hash



app = Flask(__name__)
CORS(app)

app.config["SECRET_KEY"] = "super_secret_key"  # cámbiala en producción

# -------------------------
# CONEXIÓN MONGODB
# -------------------------
uri = "mongodb+srv://marcosemiliorodriguezmartin_db_user:dpoPg74YCtl147mX@pokecasino.asaeily.mongodb.net/?retryWrites=true&w=majority&appName=PokeCasino"
client = MongoClient(uri, server_api=ServerApi("1"))
db = client["PokemonPyDB"]
usuarios = db["usuarios"]
premios = db["premios"]
pokedex = db["pokedex"]

print("Collections in app.py:", db.list_collection_names())

# -------------------------
# HELPERS JWT / ROLES
# -------------------------

def generate_token(user_id, role):
    payload = {
        "user_id": user_id,
        "role": role,
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

            # 1. Intentar buscar como ObjectId
            try:
                current_user = usuarios.find_one({"_id": ObjectId(user_id)})
            except:
                current_user = None

            # 2. Si no existe, intentar como string
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

# -------------------------
# AUTH
# -------------------------



@app.post("/auth/register")
def register():
    data = request.json or {}

    username = data.get("username")
    email = data.get("email")
    password = data.get("password")

    if not username or not email or not password:
        return jsonify({"error": "Faltan campos obligatorios"}), 400

    # Comprobación de duplicados usando las versiones lower
    if usuarios.find_one({"username_lower": username.lower()}):
        return jsonify({"error": "El nombre de usuario ya existe"}), 400

    if usuarios.find_one({"email_lower": email.lower()}):
        return jsonify({"error": "El email ya está registrado"}), 400

    # Hash de la contraseña (guardado como STRING)
    hashed = generate_password_hash(password)
    
    nuevo_usuario = {
        "username": username,                 # versión original
        "username_lower": username.lower(),   # versión para login
        "nombre": data.get("nombre", ""),
        "apellido": data.get("apellido", ""),
        "edad": int(data.get("edad", 0)),
        "email": email,                       # versión original
        "email_lower": email.lower(),         # versión para login
        "password": hashed,                   # string, como tú quieres
        "rol": "usuario",
        "fichas": 300,
        "pokes": 0
    }

    usuarios.insert_one(nuevo_usuario)

    return jsonify({"mensaje": "Usuario registrado correctamente"}), 201






@app.post("/auth/login")
def login():
    try:
        data = request.json or {}

        user_input = data.get("email") or data.get("username")
        password = data.get("password")

        if not user_input or not password:
            return jsonify({"error": "Faltan credenciales"}), 400

        user_input_lower = user_input.lower()

        usuario = usuarios.find_one({
            "$or": [
                {"email_lower": user_input_lower},
                {"username_lower": user_input_lower}
            ]
        })

        if not usuario:
            return jsonify({"error": "Usuario o contraseña incorrectos"}), 401

        hashed = usuario["password"]  # string

        if not check_password_hash(hashed, password):
            return jsonify({"error": "Usuario o contraseña incorrectos"}), 401


        token = jwt.encode(
            {"user_id": str(usuario["_id"]), "rol": usuario["rol"]},
            app.config["SECRET_KEY"],
            algorithm="HS256"
        )

        return jsonify({
            "mensaje": "Login correcto",
            "token": token,
            "username": usuario["username"],
            "email": usuario["email"],
            "rol": usuario["rol"]
        }), 200

    except Exception as e:
        import traceback
        traceback.print_exc()
        return jsonify({"error": "Error en el servidor"}), 500





@app.get("/auth/me")
@token_required
def me(current_user):
    try:
        current_user["_id"] = str(current_user["_id"])
        current_user.pop("password", None)
        return jsonify(current_user), 200
    except Exception:
        return jsonify({"error": "Error en el servidor"}), 500

# -------------------------
# CRUD USUARIOS
# -------------------------

@app.get("/usuarios")
@token_required
def listar_usuarios(current_user):
    try:
        lista = list(usuarios.find())
        for u in lista:
            u["_id"] = str(u["_id"])
            u.pop("password", None)
        return jsonify(lista), 200
    except:
        return jsonify({"error": "Error al obtener usuarios"}), 500

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

    u["_id"] = str(u["_id"])
    u.pop("password", None)
    return jsonify(u), 200

@app.post("/usuarios")
@token_required
@admin_required
def crear_usuario(current_user):
    try:
        data = request.json
        required = ["nombre", "apellido", "username", "email", "password", "edad", "rol"]
        if not all(k in data for k in required):
            return jsonify({"error": "Faltan campos"}), 400

        if usuarios.find_one({"email": data["email"]}) or usuarios.find_one({"username": data["username"]}):
            return jsonify({"error": "Email o username ya registrado"}), 400

        hashed = generate_password_hash(data["password"])

        usuario = {
            "email": data["email"],
            "username": data["username"],
            "password": hashed,
            "nombre": data["nombre"],
            "apellido": data["apellido"],
            "edad": int(data["edad"]),
            "pokes": data.get("pokes", 0),
            "fichas": data.get("fichas", 0),
            "pokemon": data.get("pokemon", []),
            "rol": data["rol"]
        }

        usuarios.insert_one(usuario)
        return jsonify({"msg": "Usuario creado"}), 201
    except Exception:
        return jsonify({"error": "Error en el servidor"}), 500

@app.put("/usuarios/<id>")
@token_required
@admin_required
def modificar_usuario(current_user, id):
    
    try:
        obj_id = ObjectId(id)
    except Exception:
        return jsonify({"error": "ID inválido"}), 400
    
    try:
        
        data = request.json
        update = {}

        for campo in ["nombre", "apellido", "email", "rol", "fichas", "pokes","edad", "username"]:
            if campo in data:
                update[campo] = data[campo]

        if not update:
            return jsonify({"error": "No se enviaron datos para actualizar"}), 400

        r = usuarios.update_one({"_id": obj_id}, {"$set": update})

        if r.matched_count == 0:
            return jsonify({"error": "Usuario no encontrado"}), 404

        return jsonify({"msg": "Usuario actualizado"}), 200

    except:
        return jsonify({"error": "Error al actualizar usuario"}), 500


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

# -------------------------
# CASINO
# -------------------------




SYMBOLS = ["Bar", "Meowth", "Koffing", "Arbok", "Cherry", "Seven"]

PAYOUTS = {
    "Seven": 300,
    "Bar": 100,
    "Meowth": 15,
    "Koffing": 15,
    "Arbok": 15,
    "Cherry": 8
}

def generar_rodillo():
    return [random.randint(0, 5) for _ in range(5)]

def desplazar(roll):
    for i in range(4):
        roll[i] = roll[i+1]
    roll[4] = random.randint(0, 5)

def obtener_tablero(r1, r2, r3):
    return [
        [r1[1], r1[2], r1[3]],
        [r2[1], r2[2], r2[3]],
        [r3[1], r3[2], r3[3]]
    ]

def es_jackpot(tablero):
    primero = tablero[0][0]
    for x in range(3):
        for y in range(3):
            if tablero[x][y] != primero:
                return False
    return True

def payout_linea(simbolo):
    return PAYOUTS.get(simbolo, 0)

def comprobar_ganar(tablero, apuesta):
    lineas = []
    payout_total = 0

    # FILAS (fila 0, fila 1, fila 2)
    fila0 = tablero[0][0] == tablero[1][0] == tablero[2][0]
    fila1 = tablero[0][1] == tablero[1][1] == tablero[2][1]
    fila2 = tablero[0][2] == tablero[1][2] == tablero[2][2]

    # DIAGONALES
    diag1 = tablero[0][0] == tablero[1][1] == tablero[2][2]
    diag2 = tablero[0][2] == tablero[1][1] == tablero[2][0]

    # FILA CENTRAL siempre activa con apuesta 1
    if apuesta >= 1 and fila1:
        lineas.append(SYMBOLS[tablero[1][1]])

    # FILA SUPERIOR e INFERIOR solo con apuesta 3
    if apuesta == 3 and fila0:
        lineas.append(SYMBOLS[tablero[1][0]])

    if apuesta == 3 and fila2:
        lineas.append(SYMBOLS[tablero[1][2]])

    # DIAGONALES con apuesta 2 o 3
    if apuesta >= 2 and diag1:
        lineas.append(SYMBOLS[tablero[1][1]])

    if apuesta >= 2 and diag2:
        lineas.append(SYMBOLS[tablero[1][1]])

    # Calcular payout
    for simbolo in lineas:
        payout_total += PAYOUTS.get(simbolo, 0)

    return payout_total, lineas


@app.post("/casino/jugar")
@token_required
def jugar(current_user):
    try:
        data = request.json or {}
        apuesta = int(data.get("apuesta", 1))
        tablero = data.get("tablero")

        if apuesta not in [1, 2, 3]:
            return jsonify({"error": "Apuesta inválida"}), 400

        if not tablero:
            return jsonify({"error": "Falta el tablero"}), 400

        if current_user["fichas"] < apuesta:
            return jsonify({"error": "No tienes fichas suficientes"}), 400

        payout, lineas = comprobar_ganar(tablero, apuesta)

        fichas_final = current_user["fichas"] - apuesta + payout
        pokes_ganados = 0

        usuarios.update_one(
            {"_id": current_user["_id"]},
            {
                "$set": {"fichas": fichas_final}
            }
        )

        return jsonify({
            "tablero": tablero,
            "simbolos": SYMBOLS,
            "apuesta": apuesta,
            "payout": payout,
            "lineas_ganadoras": lineas,
            "fichas_final": fichas_final,
            "pokes_ganados": pokes_ganados
        }), 200

    except Exception as e:
        print("ERROR:", e)
        return jsonify({"error": "Error en el servidor"}), 500

@app.get("/premios")
@token_required
def listar_premios(current_user):
    premios = list(db["premios"].find({}, {"_id": 0}))
    return jsonify(premios), 200


@app.post("/premios/comprar/<int:pokemon_id>")
@token_required
def comprar_pokemon(current_user, pokemon_id):
    premios = db["premios"]
    pokedex = db["pokedex"]
    usuarios = db["usuarios"]

    premio = premios.find_one({"pokemon_id": pokemon_id})
    if not premio:
        return jsonify({"error": "Pokémon no disponible como premio"}), 404

    if current_user["fichas"] < premio["precio"]:
        return jsonify({"error": "No tienes suficientes fichas"}), 400

    pokemon = pokedex.find_one({"Id": pokemon_id})
    if not pokemon:
        return jsonify({"error": "Pokémon no encontrado en la Pokédex"}), 404

    nuevo_pokemon = {
        "pokemon_id": pokemon_id,
        "nombre": pokemon["Nombre"],
        "fecha_obtenido": datetime.datetime.utcnow().strftime("%Y-%m-%d")
    }

    usuarios.update_one(
        {"_id": current_user["_id"]},
        {
            "$inc": {"fichas": -premio["precio"]},
            "$push": {"pokemon": nuevo_pokemon}
        }
    )

    return jsonify({
        "msg": "Pokémon obtenido",
        "pokemon": nuevo_pokemon
    }), 200



@app.get("/pokedex")
@token_required
def pokedex(current_user):
    data = list(db["pokedex"].find({}, {"_id": 0}))
    return jsonify(data), 200



@app.get("/usuarios/mis_pokemon")
@token_required
def mis_pokemon(current_user):
    return jsonify(current_user.get("pokemon", [])), 200





@app.put("/usuarios/<id>/reset_password")
@token_required
@admin_required
def reset_password(current_user, id):
    try:
        obj_id = ObjectId(id)
    except Exception:
        return jsonify({"error": "ID inválido"}), 400

    nueva = request.json.get("password")
    if not nueva:
        return jsonify({"error": "Contraseña requerida"}), 400

    hashed = generate_password_hash(nueva)

    r = usuarios.update_one({"_id": obj_id}, {"$set": {"password": hashed}})
    if r.matched_count == 0:
        return jsonify({"error": "Usuario no encontrado"}), 404

    return jsonify({"msg": "Contraseña reseteada"}), 200

@app.post("/usuarios/admin_create") 
@token_required 
@admin_required
def admin_create_user(current_user):
    try:
        data = request.json

        # Si no trae contraseña, se genera una por defecto
        if "password" not in data:
            data["password"] = "12345678"

        if "password" not in data:
            data["password"] = "12345678"

        data["password"] = generate_password_hash(data["password"])


        usuarios.insert_one(data)

        return jsonify({"msg": "Usuario creado"}), 201

    except Exception as e:
        print("ERROR:", e)
        return jsonify({"error": "Error al crear usuario"}), 500


@app.get("/usuarios/pokemon/<id>")
@token_required
def pokemon_usuario(current_user, id):
    try:
        obj_id = ObjectId(id)
    except Exception:
        return jsonify({"error": "ID inválido"}), 400

    u = usuarios.find_one({"_id": obj_id})
    if not u:
        return jsonify({"error": "Usuario no encontrado"}), 404

    return jsonify(u.get("pokemon", [])), 200

# -------------------------
# MAIN
# -------------------------

if __name__ == "__main__":
    app.run(debug=True)