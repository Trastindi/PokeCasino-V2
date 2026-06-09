import requests, os, time, random, msvcrt
from getpass import getpass

API_URL = "http://127.0.0.1:5000"

token = None
current_user = None

# Colores ANSI
RED = "\033[91m"
WHITE = "\033[97m"
RESET = "\033[0m"
GREEN = "\033[92m" 
YELLOW = "\033[93m" 
BLUE = "\033[94m" 
MAGENTA = "\033[95m" 
CYAN = "\033[96m"



#   HEADERS
# ============================
def headers():
    return {"Authorization": f"Bearer {token}"} if token else {}



#   LOGIN
# ============================
def login():
    global token, current_user
    
    print("\n--- Iniciar sesión ---")
    email = input("Email o username: ")
    password = getpass("Contraseña: ")

    r = requests.post(f"{API_URL}/auth/login", json={
        "email": email,
        "password": password
    })

    if r.status_code != 200:
        try:
            data = r.json()
            print("Error:", data.get("error"))
        except:
            print("STATUS:", r.status_code)
            print("RESPUESTA RAW:", r.text)
        return False

    token = r.json()["token"]

    # Obtener datos del usuario
    r2 = requests.get(f"{API_URL}/auth/me", headers=headers())
    current_user = r2.json()

    nombre_mostrar = current_user.get("nombre") or current_user.get("username")

    print(f"\nBienvenido, {nombre_mostrar}!")
    print("Token: ", token)
    return True




#   REGISTRO
# ============================

def register():
    print("\n--- Registro ---")
    nombre = input("Nombre: ")
    apellido = input("Apellido: ")
    username = input("Username: ")
    email = input("Email: ")
    
    password = getpass("Contraseña: ")
    edad = int(input("Edad: "))

    r = requests.post(f"{API_URL}/auth/register", json={
        "nombre": nombre,
        "apellido": apellido,
        "username": username,
        "email": email,
        "password": password,
        "edad": edad
    })

    if r.status_code != 201:
        print("Error:", r.json().get("error"))
        return

    print("Usuario registrado correctamente.")


#   VER PERFIL
# ============================

def ver_perfil():
    r = requests.get(f"{API_URL}/auth/me", headers=headers())
    if r.status_code != 200:
        print("Error al obtener perfil")
        return

    u = r.json()
    print("\n--- Mi Perfil ---")
    print(f"Nombre: {u['nombre']} {u['apellido']}")
    print(f"Email: {u['email']}")
    print(f"Fichas: {u['fichas']}")
    print(f"Pokes: {u['pokes']}")
    print(f"Rol: {u['rol']}")



#   MIS POKÉMON (menú interactivo con detalle, stats y gestión de equipos)
# ============================

def mis_pokemon_menu():
    r = requests.get(f"{API_URL}/usuarios/mis_pokemon", headers=headers())
    if r.status_code != 200:
        print("Error:", r.json().get("error"))
        return

    lista = r.json()
    if not lista:
        print("\nNo tienes ningún Pokémon todavía.")
        return

    # Mostrar por posición (1-based) para evitar ambigüedad con duplicados
    print("\n--- Mis Pokémon ---")
    for i, p in enumerate(lista, 1):
        tipo2 = f" / {p['TipoSecundario']}" if p.get("TipoSecundario") else ""
        print(f"  {i}. {p['Nombre']}  "
              f"[{p.get('TipoPrincipal', '?')}{tipo2}]  "
              f"Nv.{p.get('Nivel', 1)}  "
              f"(ID: {p['_id']})")

    sel = input("\nSelecciona la posición del Pokémon (o ENTER para volver): ").strip()
    if not sel:
        return
    try:
        pos = int(sel) - 1
        if pos < 0 or pos >= len(lista):
            raise IndexError
        elegido = lista[pos]
    except (ValueError, IndexError):
        print("Posición inválida.")
        return

    # Mostrar detalle
    tipo2 = f" / {elegido['TipoSecundario']}" if elegido.get("TipoSecundario") else ""
    print(f"\n--- {elegido['Nombre']} ---")
    print(f"  ID PokemonUser : {elegido['_id']}")
    print(f"  Nº Pokédex     : {elegido['PokemonId']}")
    print(f"  Tipo           : {elegido.get('TipoPrincipal', '?')}{tipo2}")
    print(f"  Nivel          : {elegido.get('Nivel', 1)}")
    print(f"  HP actual      : {elegido.get('CurrentHp', '?')}")
    moveset = elegido.get("MoveSet") or []
    print(f"  Movimientos    : {', '.join(moveset) if moveset else 'Ninguno'}")
    print(f"  Obtenido       : {elegido.get('FechaObtenido', '?')}")

    # Estadísticas base desde la Pokédex
    r_dex = requests.get(f"{API_URL}/pokedex/{elegido['PokemonId']}", headers=headers())
    if r_dex.status_code == 200:
        stats = r_dex.json().get("estadisticas_base", {})
        if stats:
            print(f"\n  --- Estadísticas base ---")
            print(f"  {'PS':<22}: {stats.get('ps', '?')}")
            print(f"  {'Ataque':<22}: {stats.get('ataque', '?')}")
            print(f"  {'Defensa':<22}: {stats.get('defensa', '?')}")
            print(f"  {'Ataque Especial':<22}: {stats.get('ataque_especial', '?')}")
            print(f"  {'Defensa Especial':<22}: {stats.get('defensa_especial', '?')}")
            print(f"  {'Velocidad':<22}: {stats.get('velocidad', '?')}")
    else:
        print(f"  {YELLOW}(No se pudieron cargar las estadísticas){RESET}")

    # Gestión de equipos
    print("\n¿Qué quieres hacer?")
    print("  1. Añadir a un equipo existente")
    print("  2. Crear un nuevo equipo con este Pokémon")
    print("  3. Volver")
    accion = input("Opción: ").strip()

    # Se pasa el _id del documento PokemonUser (string)
    poke_doc_id = elegido["_id"]
    if accion == "1":
        _añadir_a_equipo_existente(poke_doc_id, elegido["Nombre"])
    elif accion == "2":
        _crear_equipo_con_pokemon(poke_doc_id, elegido["Nombre"])


def _añadir_a_equipo_existente(poke_doc_id, nombre_pokemon):
    """Añade el _id de PokemonUser a un equipo existente si hay hueco y no está ya."""
    r = requests.get(f"{API_URL}/users/pokemonteams", headers=headers())
    if r.status_code != 200:
        print("Error al obtener equipos:", r.json().get("error"))
        return

    equipos = r.json()
    if not equipos:
        print("No tienes equipos creados. Usa la opción 2 para crear uno.")
        return

    # Filtrar equipos con hueco y que no contengan ya este _id
    disponibles = []
    for t in equipos:
        ids = t.get("pokemon_ids", [])
        if len(ids) >= 6:
            continue
        if poke_doc_id in ids:
            continue
        disponibles.append(t)

    if not disponibles:
        print("No hay equipos disponibles "
              "(todos tienen 6 Pokémon o ya incluyen este Pokémon).")
        return

    print("\n--- Equipos disponibles ---")
    for i, t in enumerate(disponibles, 1):
        print(f"  {i}. {t['team_name']}  "
              f"({len(t.get('pokemon_ids', []))}/6 Pokémon)")

    sel = input("Selecciona el número del equipo: ").strip()
    try:
        idx = int(sel) - 1
        if idx < 0 or idx >= len(disponibles):
            raise IndexError
        equipo = disponibles[idx]
    except (ValueError, IndexError):
        print("Selección inválida.")
        return

    nuevos_ids = equipo.get("pokemon_ids", []) + [poke_doc_id]
    team_id = equipo.get("_id") or equipo.get("id")

    r2 = requests.put(
        f"{API_URL}/users/pokemonteams/{team_id}",
        json={"pokemon_ids": nuevos_ids},
        headers=headers()
    )
    if r2.status_code == 200:
        print(f"{GREEN}¡{nombre_pokemon} añadido al equipo '{equipo['team_name']}'!{RESET}")
    else:
        print("Error:", r2.json().get("error"))


def _crear_equipo_con_pokemon(poke_doc_id, nombre_pokemon):
    """Crea un equipo nuevo con el _id de PokemonUser como primer integrante."""
    nombre = input("Nombre para el nuevo equipo: ").strip()
    if not nombre:
        print("El nombre no puede estar vacío.")
        return

    r = requests.post(
        f"{API_URL}/users/pokemonteams",
        json={"team_name": nombre, "pokemon_ids": [poke_doc_id]},
        headers=headers()
    )
    if r.status_code == 201:
        print(f"{GREEN}Equipo '{nombre}' creado con {nombre_pokemon} como primer integrante.{RESET}")
    else:
        print("Error:", r.json().get("error"))


#   POKÉDEX
# ============================


def pokedex_menu():
    r = requests.get(f"{API_URL}/pokedex", headers=headers())
    if r.status_code != 200:
        print("Error al obtener Pokédex")
        return

    pokedex = r.json()

    r2 = requests.get(f"{API_URL}/usuarios/mis_pokemon", headers=headers())
    mis_pokes = r2.json()
    ids_usuario = {p["pokemon_id"] for p in mis_pokes}

    id_to_name = {p["Id"]: p["Nombre"] for p in pokedex}

    print("\n--- Pokédex ---")
    for p in pokedex:

        capturado = p["Id"] in ids_usuario
        color = RED if capturado else BLUE

        # Si NO está capturado mostrar "???"
        nombre_mostrar = p["Nombre"] if capturado else "???"

        print(f"{color}{p['Id']}. {nombre_mostrar}{RESET}")

    sel = input("\nSelecciona un Pokémon por ID: ")
    try:
        sel = int(sel)
    except:
        print("ID inválido")
        return

    elegido = next((p for p in pokedex if p["Id"] == sel), None)
    if not elegido:
        print("Pokémon no encontrado")
        return

    evoluciones_nombres = [id_to_name[e] for e in elegido["Evoluciones"]]

    print("\n--- Información del Pokémon ---")
    print(f"Nombre: {elegido['Nombre']}")
    print(f"Tipo: {elegido['Tipo1']} / {elegido.get('Tipo2', '')}")
    print(f"Región: {elegido['Region']}")
    print(f"Descripción: {elegido['Descripcion']}")
    print(f"Evoluciones: {', '.join(evoluciones_nombres) if evoluciones_nombres else 'Ninguna'}")




#   CANJEAR POKÉMON
# ============================
def canjear_pokemon():
    r = requests.get(f"{API_URL}/premios", headers=headers())
    if r.status_code != 200:
        print("Error al obtener premios")
        return

    premios = r.json()

    print("\n--- Premios disponibles ---")
    for p in premios:
        print(f"{p['pokemon_id']} - {p['nombre']} (Precio: {p['precio']} fichas)")

    sel = input("\nSelecciona un Pokémon por ID para comprar: ")
    try:
        sel = int(sel)
    except:
        print("ID inválido")
        return

    r2 = requests.post(f"{API_URL}/premios/comprar/{sel}", headers=headers())
    data = r2.json()

    if r2.status_code != 200:
        print("Error:", data.get("error"))
        return

    print("\n¡Pokémon obtenido!")
    print(f"{data['pokemon']['nombre']} - Fecha: {data['pokemon']['fecha_obtenido']}")


#   DESAFIAR A BATALLA
# ============================
def desafiar_usuario(rival_id):
    r = requests.post(f"{API_URL}/battle_requests/{rival_id}", json={}, headers=headers())
    if r.status_code != 201:
        print("Error:", r.json().get("error"))
        return

    print("Desafío enviado correctamente.")


#   MIS MENSAJES (menú interactivo con respuesta a solicitudes de batalla)
# ============================
def mis_mensajes_menu():
    r = requests.get(f"{API_URL}/messages/mis_mensajes", headers=headers())
    if r.status_code != 200:
        print("Error al obtener mensajes")
        return

    lista = r.json()
    if not lista:
        print("\nNo tienes mensajes.")
        return

    print("\n--- Mis Mensajes ---")
    for i, m in enumerate(lista, 1):
        tipo  = m.get("type", "message")
        estado = f" {YELLOW}[respondido]{RESET}" if m.get("responded") else ""
        print(f"  {i}. {CYAN}[{tipo}]{RESET}{estado}  "
              f"De: {m.get('from', '?')}  —  {m.get('title', '')}")

    sel = input("\nSelecciona un mensaje por número (o ENTER para volver): ").strip()
    if not sel:
        return
    try:
        pos = int(sel) - 1
        if pos < 0 or pos >= len(lista):
            raise IndexError
        msg = lista[pos]
    except (ValueError, IndexError):
        print("Selección inválida.")
        return

    print(f"\n--- {msg.get('title', '')} ---")
    print(f"  De    : {msg.get('from', '?')}")
    print(f"  Fecha : {msg.get('Fecha', '?')}")
    print(f"  Texto : {msg.get('text', '')}")

    tipo = msg.get("type", "")

    if tipo == "battle_request" and not msg.get("responded"):
        print("\n  1. Aceptar solicitud de batalla")
        print("  2. Rechazar")
        print("  3. Volver")
        op = input("Opción: ").strip()
        if op == "1":
            _responder_batalla(msg["_id"], accepted=True)
        elif op == "2":
            _responder_batalla(msg["_id"], accepted=False)

    elif tipo == "battle_request" and msg.get("responded"):
        print(f"\n  {YELLOW}Ya respondiste a esta solicitud.{RESET}")

    elif tipo == "battle_response":
        bid = msg.get("battle_id", "?")
        print(f"\n  {GREEN}✅ Tu solicitud de batalla fue aceptada.{RESET}")
        print(f"  Battle ID: {bid}")
        print("  (Próximamente: unirse a la batalla automáticamente)")

    elif tipo == "battle_rejected":
        print(f"\n  {RED}❌ El rival rechazó tu solicitud de batalla.{RESET}")


def _responder_batalla(msg_id, accepted: bool):
    """Llama a POST /battle_requests/<msg_id>/respond con accepted True/False."""
    r = requests.post(
        f"{API_URL}/battle_requests/{msg_id}/respond",
        json={"accepted": accepted},
        headers=headers()
    )
    if r.status_code == 200:
        if accepted:
            bid = r.json().get("battle_id", "?")
            print(f"{GREEN}¡Batalla aceptada! Battle ID: {bid}{RESET}")
            print("(Próximamente: conexión automática al endpoint de batalla)")
        else:
            print("Solicitud rechazada correctamente.")
    else:
        print("Error:", r.json().get("error"))


#   MENÚ PRINCIPAL USUARIO
# ============================
def menu_usuario():
    while True:
        print("\n--- Menú Usuario ---")
        print("1. Ver mi perfil")
        print("2. Jugar al casino (¡El buen Gacha!)")
        print("3. Canjear Pokémon")
        print("4. Pokédex")
        print("5. Mis Pokémon")
        print("6. Desafiar a otro usuario a batalla")
        print("7. Ver mis mensajes")
        print("8. Cerrar sesión")

        op = input("Opción: ")

        if op == "1":
            ver_perfil()
        elif op == "2":
            jugar_casino()
        elif op == "3":
            canjear_pokemon()
        elif op == "4":
            pokedex_menu()
        elif op == "5":
            mis_pokemon_menu()
        elif op == "6":
            rival_id = input("ID del usuario a desafiar: ")
            desafiar_usuario(rival_id)
        elif op == "7":
            mis_mensajes_menu()
        elif op == "8":
            break
        else:
            print("Opción inválida.")




#   MENÚ ADMIN
# ============================
def menu_admin():
    while True:
        print("\n--- Menú Administrador ---")
        print("1. Listar usuarios (básico)")
        print("2. Listar usuarios con detalles")
        print("3. Modificar datos de un usuario")
        print("4. Eliminar usuario")
        print("5. Resetear contraseña de un usuario")
        print("6. Exportar un usuario a JSON")
        print("7. Exportar todos los usuarios a JSON")
        print("8. Importar usuarios desde JSON")
        print("9. Salir")

        op = input("Opción: ")

        # 1. Listar usuarios (básico)
        if op == "1":
            r = requests.get(f"{API_URL}/usuarios", headers=headers())
            if r.status_code != 200:
                print("Error:", r.json().get("error"))
                continue

            print("\n--- Lista de usuarios ---")
            for u in r.json():
                print(f"{u['_id']} - {u['nombre']} {u['apellido']} ({u['rol']})")

        # 2. Listar usuarios con detalles
        elif op == "2":
            r = requests.get(f"{API_URL}/usuarios", headers=headers())
            if r.status_code != 200:
                print("Error:", r.json().get("error"))
                continue

            print("\n--- Lista de usuarios (detallado) ---")
            for u in r.json():
                print("\n------------------------")
                print(f"ID: {u['_id']}")
                print(f"Nombre: {u['nombre']} {u['apellido']}")
                print(f"Username: {u['username']}")
                print(f"Edad: {u['edad']}")
                print(f"Email: {u['email']}")
                print(f"Rol: {u['rol']}")
                print(f"Pokes: {u['pokes']}")
                print(f"Fichas: {u['fichas']}")
                print(f"Pokémon: {len(u.get('pokemon', []))}")

        # 3. Modificar datos del usuario
        elif op == "3":
            uid = input("ID del usuario a modificar: ")

            print("\nIntroduce los nuevos valores (deja vacío para no cambiar):")
            nuevo_nombre = input("Nuevo nombre: ")
            nuevo_apellido = input("Nuevo apellido: ")
            nuevo_email = input("Nuevo email: ")
            nuevo_rol = input("Nuevo rol (user/admin): ")
            nuevas_fichas = input("Nuevas fichas: ")
            nuevos_pokes = input("Nuevos pokes: ")

            payload = {}

            if nuevo_nombre.strip():
                payload["nombre"] = nuevo_nombre
            if nuevo_apellido.strip():
                payload["apellido"] = nuevo_apellido
            if nuevo_email.strip():
                payload["email"] = nuevo_email
            if nuevo_rol.strip():
                payload["rol"] = nuevo_rol
            if nuevas_fichas.strip():
                payload["fichas"] = int(nuevas_fichas)
            if nuevos_pokes.strip():
                payload["pokes"] = int(nuevos_pokes)

            if not payload:
                print("No se ha cambiado ningún dato.")
                continue

            r = requests.put(f"{API_URL}/usuarios/{uid}", json=payload, headers=headers())
            if r.status_code != 200:
                print("Error:", r.json().get("error"))
                continue

            print("Usuario modificado correctamente.")

        # 4. Eliminar usuario
        elif op == "4":
            uid = input("ID del usuario a eliminar: ")
            confirm = input("¿Seguro que quieres eliminarlo? (s/n): ")
            if confirm.lower() != "s":
                print("Operación cancelada.")
                continue

            r = requests.delete(f"{API_URL}/usuarios/{uid}", headers=headers())
            if r.status_code != 200:
                print("Error:", r.json().get("error"))
                continue

            print("Usuario eliminado correctamente.")

        # 5. Resetear contraseña
        elif op == "5":
            uid = input("ID del usuario: ")
            nueva_pass = input("Nueva contraseña: ")

            r = requests.put(
                f"{API_URL}/usuarios/{uid}/reset_password",
                json={"password": nueva_pass},
                headers=headers()
            )
            if r.status_code != 200:
                print("Error:", r.json().get("error"))
                continue

            print("Contraseña reseteada correctamente.")

        # 6. Exportar usuario a JSON
        elif op == "6":
            exportar_usuario()

        # 7. Exportar todos los usuarios a JSON
        elif op == "7":
            exportar_todos()

        # 8. Importar usuarios desde JSON
        elif op == "8":
            importar_desde_json()

        # 9. Salir
        elif op == "9":
            break

        else:
            print("Opción inválida.")




#   MENÚ PRINCIPAL
# ============================
def main():
    while True:
        print("\n===========================")
        print("      CASINO POKÉMON")
        print("===========================")
        print("1. Iniciar sesión")
        print("2. Registrarse")
        print("3. Salir")

        op = input("Opción: ")

        if op == "1":
            if not login():
                continue

            if current_user["Role"] == "admin": 
                menu_admin() 
            else: 
                menu_usuario()
        elif op == "2":
            register()
        elif op == "3":
            print("Hasta luego.")
            break
        else:
            print("Opción inválida.")


def exportar_usuario():
    uid = input("ID del usuario a exportar: ")

    r = requests.get(f"{API_URL}/usuarios/{uid}", headers=headers())
    if r.status_code != 200:
        print("Error:", r.json().get("error"))
        return

    usuario = r.json()

    filename = f"usuario_{uid}.json"
    with open(filename, "w", encoding="utf-8") as f:
        import json
        json.dump(usuario, f, indent=4, ensure_ascii=False)

    print(f"Usuario exportado correctamente a {filename}")


def exportar_todos():
    r = requests.get(f"{API_URL}/usuarios", headers=headers())
    if r.status_code != 200:
        print("Error:", r.json().get("error"))
        return

    usuarios = r.json()

    filename = "usuarios_completos.json"
    with open(filename, "w", encoding="utf-8") as f:
        import json
        json.dump(usuarios, f, indent=4, ensure_ascii=False)

    print(f"Todos los usuarios exportados correctamente a {filename}")

def importar_desde_json():
    archivo = input("Nombre del archivo JSON a importar: ")

    if not os.path.exists(archivo):
        print("El archivo no existe.")
        return

    import json
    with open(archivo, "r", encoding="utf-8") as f:
        datos = json.load(f)

    if isinstance(datos, dict):
        datos = [datos]  # convertir a lista si es un solo usuario

    for usuario in datos:
        r = requests.post(
            f"{API_URL}/usuarios/admin_create",
            json=usuario,
            headers=headers()
        )
        if r.status_code != 201:
            print(f"Error importando usuario {usuario.get('_id')}: {r.json().get('error')}")
        else:
            print(f"Usuario {usuario.get('_id')} importado correctamente.")

    print("Importación finalizada.")



def mostrar_tablero(tablero, simbolos):
    print(f"\n {RED}┌──────────────────────────────────────────────────────────────┐{RESET}")
    for fila in range(3):
        a, b, c = tablero[0][fila], tablero[1][fila], tablero[2][fila]
        print(f"   │ {simbolos[a]:^10} {simbolos[b]:^10} {simbolos[c]:^10} │")
    print(f"\n {RED}┌──────────────────────────────────────────────────────────────┐{RESET}")

def animar_giro(simbolos):
    for _ in range(6):
        fake = [[random.randint(0, 5) for _ in range(3)] for _ in range(3)]
        mostrar_tablero(fake, simbolos)
        time.sleep(0.1)
        print("\033[7A", end="")  # sube 7 líneas para reemplazar el tablero


def jugar_casino():
    simbolos = ["Bar", "Meowth", "Koffing", "Arbok", "Cherry", "Seven"]

    while True:
        print("\n--- Casino Pokémon ---")
        apuesta = input("Apuesta (1, 2 o 3 fichas): ")

        try:
            apuesta = int(apuesta)
            if apuesta not in [1, 2, 3]:
                raise ValueError
        except:
            print("Apuesta inválida.")
            continue

        print("\nGIRANDO... Pulsa ENTER para parar cada rodillo.")

        # --- RODILLO 1 ---
        while True:
            r1 = [random.randint(0, 5) for _ in range(3)]
            mostrar_tablero([[r1[0], r1[1], r1[2]], [0, 0, 0], [0, 0, 0]], simbolos)
            time.sleep(0.08)
            print("\033[7A", end="")
            if msvcrt.kbhit() and msvcrt.getch() == b'\r':
                break

        # --- RODILLO 2 ---
        while True:
            r2 = [random.randint(0, 5) for _ in range(3)]
            mostrar_tablero([[r1[0], r1[1], r1[2]], [r2[0], r2[1], r2[2]], [0, 0, 0]], simbolos)
            time.sleep(0.08)
            print("\033[7A", end="")
            if msvcrt.kbhit() and msvcrt.getch() == b'\r':
                break

        # --- RODILLO 3 ---
        while True:
            r3 = [random.randint(0, 5) for _ in range(3)]
            mostrar_tablero([[r1[0], r1[1], r1[2]], [r2[0], r2[1], r2[2]], [r3[0], r3[1], r3[2]]], simbolos)
            time.sleep(0.08)
            print("\033[7A", end="")
            if msvcrt.kbhit() and msvcrt.getch() == b'\r':
                break

        # TABLERO FINAL
        tablero = [
            [r1[0], r1[1], r1[2]],
            [r2[0], r2[1], r2[2]],
            [r3[0], r3[1], r3[2]]
        ]

        print("\nResultado final:")
        mostrar_tablero(tablero, simbolos)

        # Enviar al backend para calcular premio
        r = requests.post(
            f"{API_URL}/casino/jugar",
            json={"apuesta": apuesta, "tablero": tablero},
            headers=headers()
        )

        data = r.json()

        if r.status_code != 200:
            print("Error:", data.get("error"))
            return

        if data["payout"] > 0:
            print(f"{GREEN}¡Has ganado {data['payout']} fichas !{RESET}")
            if data["lineas_ganadoras"]:
                print(f"{YELLOW}Líneas ganadoras: {', '.join(set(data['lineas_ganadoras']))}{RESET}")
        else:
            print(f"{RED}No has ganado esta vez...{RESET}")

        print(f"{CYAN}Fichas actuales: {data['fichas_final']}{RESET}")

    
        while True:
            seguir = input("\n¿Quieres seguir jugando? (s/n): ").lower().strip()
            if seguir in ["s", "n"]:
                break
            print("Opción inválida. Escribe 's' o 'n'.")

        if seguir == "n":
            print("\nSaliendo del casino...\n")
            print("\n" * 5)  # evita solapamientos con el menú
            break




if __name__ == "__main__":
    main()
