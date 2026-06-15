# -*- coding: utf-8 -*-
import requests, os, time, random, msvcrt, json
import getpass as getpass_module
from getpass import getpass

API_URL = "http://127.0.0.1:5000"

token        = None
current_user = None
batalla      = {}
is_on_battle = False
equipo_seleccionado = {}

# Colores ANSI
RED     = "\033[91m"
WHITE   = "\033[97m"
RESET   = "\033[0m"
GREEN   = "\033[92m"
YELLOW  = "\033[93m"
BLUE    = "\033[94m"
MAGENTA = "\033[95m"
CYAN    = "\033[96m"
BOLD    = "\033[1m"

# Tabla de tipos
type_chart = {}

def _load_type_chart():
    path = os.path.join(os.path.dirname(__file__), "type_chart.json")
    global type_chart
    try:
        with open(path, "r", encoding="utf-8") as f:
            type_chart = json.load(f)
    except FileNotFoundError:
        pass


# ============================
#   HEADERS
# ============================
def headers():
    return {"Authorization": f"Bearer {token}"} if token else {}


# ============================
#   LOGIN
# ============================
def login():
    global token, current_user

    print("\n--- Iniciar sesión ---")
    email    = input("Email o username: ")
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

    r2 = requests.get(f"{API_URL}/auth/me", headers=headers())
    current_user = r2.json()

    nombre_mostrar = current_user.get("nombre") or current_user.get("username")
    print(f"\nBienvenido, {nombre_mostrar}!")
    print("Token: ", token)
    return True


# ============================
#   REGISTRO
# ============================
def register():
    print("\n--- Registro ---")
    nombre   = input("Nombre: ")
    apellido = input("Apellido: ")
    username = input("Username: ")
    email    = input("Email: ")
    password = getpass("Contraseña: ")
    edad     = int(input("Edad: "))

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


# ============================
#   VER PERFIL
# ============================
def ver_perfil():
    r = requests.get(f"{API_URL}/auth/me", headers=headers())
    if r.status_code != 200:
        print("Error al obtener perfil")
        return

    u = r.json()
    print("\n--- Mi Perfil ---")
    print(f"Nombre: {u['Nombre']} {u['Apellido']}")
    print(f"Email: {u['Correo']}")
    print(f"Fichas: {u['FichasCasino']}")
    print(f"Pokes: {u['Pokes']}")
    print(f"Rol: {u['Role']}")


# ============================
#   MIS POKÉMON
# ============================
def mis_pokemon():
    r = requests.get(f"{API_URL}/usuarios/mis_pokemon", headers=headers())
    if r.status_code != 200:
        print("Error:", r.json().get("error"))
        return

    print("\n--- Mis Pokémon ---")
    lista = r.json()

    if not lista:
        print("No tienes ningún Pokémon todavía.")
        return

    for p in lista:
        print(f"{p['pokemon_id']} - {p['nombre']} (Obtenido: {p['fecha_obtenido']})")


# ============================
#   POKÉDEX
# ============================
def pokedex_menu():
    r = requests.get(f"{API_URL}/pokedex", headers=headers())
    if r.status_code != 200:
        print("Error al obtener Pokédex")
        return

    pokedex = r.json()

    r2 = requests.get(f"{API_URL}/usuarios/mis_pokemon", headers=headers())
    mis_pokes   = r2.json()
    ids_usuario = {p["pokemon_id"] for p in mis_pokes}

    id_to_name = {p["Id"]: p["Nombre"] for p in pokedex}

    print("\n--- Pokédex ---")
    for p in pokedex:
        capturado      = p["Id"] in ids_usuario
        color          = RED if capturado else BLUE
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


# ============================
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

    r2   = requests.post(f"{API_URL}/premios/comprar/{sel}", headers=headers())
    data = r2.json()

    if r2.status_code != 200:
        print("Error:", data.get("error"))
        return

    print("\n¡Pokémon obtenido!")
    print(f"{data['pokemon']['nombre']} - Fecha: {data['pokemon']['fecha_obtenido']}")


# ============================
#   CASINO
# ============================
def mostrar_tablero(tablero, simbolos):
    print(f"\n {RED}┌──────────────────────────────────────────────────────────────┐{RESET}")
    for fila in range(3):
        a, b, c = tablero[0][fila], tablero[1][fila], tablero[2][fila]
        print(f"   │ {simbolos[a]:^10} {simbolos[b]:^10} {simbolos[c]:^10} │")
    print(f"\n {RED}┌──────────────────────────────────────────────────────────────┐{RESET}")


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

        tablero = [
            [r1[0], r1[1], r1[2]],
            [r2[0], r2[1], r2[2]],
            [r3[0], r3[1], r3[2]]
        ]

        print("\nResultado final:")
        mostrar_tablero(tablero, simbolos)

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
            print(f"{GREEN}¡Has ganado {data['payout']} fichas!{RESET}")
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
            print("\n" * 5)
            break


# ============================
#   MENSAJES
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
        tipo   = m.get("type", "message")
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
        print("  ¿Quieres ir al menú de batalla ahora? (s/n)")
        if input().strip().lower() == "s":
            global is_on_battle, batalla
            is_on_battle = True
            batalla = obtener_batalla(bid)

    elif tipo == "battle_rejected":
        print(f"\n  {RED}❌ El rival rechazó tu solicitud de batalla.{RESET}")


# ============================
#   BATALLA
# ============================
def _responder_batalla(msg_id, accepted: bool):
    global is_on_battle, batalla
    r = requests.post(
        f"{API_URL}/battle_requests/{msg_id}/respond",
        json={"accepted": accepted},
        headers=headers()
    )
    if r.status_code == 200:
        if accepted:
            bid = r.json().get("battle_id", "?")
            print(f"{GREEN}¡Batalla aceptada! Battle ID: {bid}{RESET}")
            is_on_battle = True
            batalla = obtener_batalla(bid)
            print(f"{is_on_battle}")
        else:
            print("Solicitud rechazada correctamente.")
    else:
        print("Error:", r.json().get("error"))


def obtener_batalla(battle_id):
    r = requests.get(f"{API_URL}/battles/{battle_id}", headers=headers())
    if r.status_code == 200:
        return r.json()
    else:
        print("Error al obtener batalla:", r.json().get("error"))
        return None


def batalla_loop(battle_id):
    global batalla
    print(f"\n{GREEN}=== BATALLA INICIADA ==={RESET}")

    while True:
        batalla = obtener_batalla(battle_id)
        status  = batalla.get("status", "")

        if status == "ready":
            _elegir_pokemon_activo(battle_id)
            print(f"{YELLOW}Esperando confirmación...{RESET}")
            while True:
                time.sleep(1.5)
                batalla    = obtener_batalla(battle_id)
                new_status = batalla.get("status", "")
                if new_status != "ready":
                    break
            continue

        if status == "choosing_action":
            _mostrar_estado_batalla(batalla)
            turno_actual = batalla.get("turn", -1)
            _elegir_accion(battle_id)
            print(f"{YELLOW}Esperando al rival...{RESET}")
            while True:
                time.sleep(1.5)
                batalla    = obtener_batalla(battle_id)
                new_status = batalla.get("status", "")
                new_turno  = batalla.get("turn", -1)
                if new_status == "finished":
                    break
                if new_status == "choosing_action" and new_turno > turno_actual:
                    break
                if new_status == "ready":
                    break
            _mostrar_log_turno(batalla.get("turn_log", []))
            if batalla.get("status") == "finished":
                break
            continue

        if status == "finished":
            break

        time.sleep(1.5)

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

    vivos = [p for p in team if p.get("current_hp", 0) > 0]
    if not vivos:
        print(f"{RED}No tienes Pokémon disponibles.{RESET}")
        return

    print("\nElige tu Pokémon:")
    for i, p in enumerate(vivos, 1):
        print(f"  {i}. {p['name']}  HP: {p['current_hp']}")

    sel = None
    while sel is None:
        try:
            sel = int(input("Número: ").strip()) - 1
            if sel < 0 or sel >= len(vivos):
                print("Selección fuera de rango.")
                sel = None
        except ValueError:
            print("Número inválido.")

    pokemon_name = vivos[sel]["name"]
    r = requests.post(
        f"{API_URL}/battles/{battle_id}/choose_pokemon",
        json={"pokemon_name": pokemon_name},
        headers=headers()
    )
    if r.status_code == 200:
        print(f"{GREEN}¡{pokemon_name} al campo!{RESET}")
    else:
        print(f"{RED}Error al elegir Pokémon: {r.json().get('error')}{RESET}")


def _elegir_accion(battle_id):
    global batalla
    uid     = current_user.get("id") or current_user.get("_id", "")
    my_slot = "player1_team" if batalla.get("player1_id") == uid else "player2_team"
    active  = batalla.get(my_slot, {}).get("active_pokemon", {})

    print(f"\n--- {active.get('name','?')} ---  HP: {active.get('current_hp','?')}")
    print("1. Atacar")
    print("2. Cambiar Pokémon")
    op = input("Opción: ").strip()

    if op == "1":
        _elegir_ataque(battle_id, active)
    elif op == "2":
        _cambiar_pokemon(battle_id, my_slot)
    else:
        print("Opción no válida. Se pasa turno.")


def _elegir_ataque(battle_id, active):
    moves = active.get("moves", [])
    if not moves:
        print(f"{RED}No tienes movimientos disponibles.{RESET}")
        return

    print("\nMovimientos:")
    for i, m in enumerate(moves, 1):
        if isinstance(m, dict):
            print(f"  {i}. {m.get('name','?')}  "
                  f"[{m.get('type','?')}]  "
                  f"Poder: {m.get('power','?')}  "
                  f"PP: {m.get('pp','?')}")
        else:
            print(f"  {i}. {m}")

    sel = None
    while sel is None:
        try:
            sel = int(input("Elige movimiento: ").strip()) - 1
            if sel < 0 or sel >= len(moves):
                print("Selección fuera de rango.")
                sel = None
        except ValueError:
            print("Número inválido.")

    move_name = moves[sel].get("name") if isinstance(moves[sel], dict) else moves[sel]
    r = requests.post(
        f"{API_URL}/battles/{battle_id}/action",
        json={"action": "attack", "move": move_name},
        headers=headers()
    )
    if r.status_code != 200:
        print(f"{RED}Error al enviar ataque: {r.json().get('error')}{RESET}")


def _cambiar_pokemon(battle_id, my_slot):
    global batalla
    team  = batalla.get(my_slot, {}).get("pokemon", [])
    vivos = [p for p in team if p.get("current_hp", 0) > 0]

    if len(vivos) <= 1:
        print("No tienes más Pokémon disponibles para cambiar.")
        return

    print("\nPokémon disponibles:")
    for i, p in enumerate(vivos, 1):
        print(f"  {i}. {p['name']}  HP: {p['current_hp']}")

    sel = None
    while sel is None:
        try:
            sel = int(input("Número: ").strip()) - 1
            if sel < 0 or sel >= len(vivos):
                print("Selección fuera de rango.")
                sel = None
        except ValueError:
            print("Número inválido.")

    pokemon_name = vivos[sel]["name"]
    r = requests.post(
        f"{API_URL}/battles/{battle_id}/action",
        json={"action": "switch", "pokemon_name": pokemon_name},
        headers=headers()
    )
    if r.status_code != 200:
        print(f"{RED}Error al cambiar Pokémon: {r.json().get('error')}{RESET}")


def _mostrar_estado_batalla(b):
    uid     = current_user.get("id") or current_user.get("_id", "")
    my_slot = "player1_team" if b.get("player1_id") == uid else "player2_team"
    en_slot = "player2_team" if my_slot == "player1_team" else "player1_team"

    my_active = b.get(my_slot, {}).get("active_pokemon", {})
    en_active = b.get(en_slot, {}).get("active_pokemon", {})

    print(f"\n{'─'*40}")
    print(f"  Rival: {en_active.get('name','?')}  HP: {en_active.get('current_hp','?')}")
    print(f"  Turno: {b.get('turn',0)}  Campo: {b.get('weather','normal')}")
    print(f"  Tú:    {my_active.get('name','?')}  HP: {my_active.get('current_hp','?')}")
    print(f"{'─'*40}")


def _mostrar_log_turno(log):
    if not log:
        return
    print(f"\n{BOLD}--- Resultado del turno ---{RESET}")
    for entry in log:
        ev = entry.get("event")
        if ev == "attack":
            boost_str = ""
            if entry.get("effectiveness"):
                boost_str = f"  {YELLOW}({entry['effectiveness']}){RESET}"
            crit_str = f"  {RED}¡Crítico!{RESET}" if entry.get("critical") else ""
            print(f"  {entry['attacker']} usa {CYAN}{entry['move']}{RESET}{boost_str}"
                  f"  → {entry.get('damage', 0)} daño{crit_str}")
        elif ev == "faint":
            print(f"  {RED}¡{entry['pokemon']} se ha debilitado!{RESET}")
        elif ev == "switch":
            print(f"  {entry['trainer']} saca a {CYAN}{entry['pokemon']}{RESET}")
        elif ev == "weather":
            print(f"  Clima: {entry.get('weather','')}")
        else:
            print(f"  {entry}")


def desafiar_usuario(rival_id):
    r = requests.post(f"{API_URL}/battles/challenge",
                      json={"rival_id": rival_id}, headers=headers())
    if r.status_code in (200, 201):
        data = r.json()
        print(f"{GREEN}Desafío enviado. ID batalla: {data.get('battle_id') or data.get('_id')}{RESET}")
    else:
        print(f"{RED}Error: {r.json().get('error')}{RESET}")


# ============================
#   MENÚ USUARIO
# ============================
def menu_usuario():
    global is_on_battle, batalla, equipo_seleccionado
    while True:
        print(f"{is_on_battle}")
        if not is_on_battle:
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
                mis_pokemon()
            elif op == "6":
                rival_id = input("ID del usuario a desafiar: ")
                desafiar_usuario(rival_id)
            elif op == "7":
                mis_mensajes_menu()
            elif op == "8":
                break
            else:
                print("Opción inválida.")
        else:
            print("\n--- Estás en una batalla ---")
            print(f"{batalla}")
            print("Introduce el nombre de tu equipo para empezar a jugar")
            team = input("Nombre del equipo: ")
            r    = requests.get(f"{API_URL}/users/pokemonteams", headers=headers()).json()
            print(r)
            for equipo in r:
                if equipo["team_name"] == team:
                    equipo_seleccionado = equipo
                    break
            if equipo_seleccionado:
                print(f"Equipo '{equipo_seleccionado}' seleccionado. Enviando datos al servidor para iniciar la batalla...")
                res = requests.post(
                    f"{API_URL}/battles/{batalla['_id']}/teams",
                    json={"team_id": equipo_seleccionado["_id"], "battle_id": batalla["_id"]},
                    headers=headers()
                )
                print(res.json())
                batalla = obtener_batalla(batalla["_id"])
                print("¡La batalla comenzará pronto! Prepárate...")
                batalla = obtener_batalla(batalla["_id"])
                if batalla and batalla.get("status") in ("ready", "choosing_action"):
                    batalla_loop(batalla["_id"])
                    is_on_battle = False
                    batalla = {}


# ============================
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

        if op == "1":
            r = requests.get(f"{API_URL}/usuarios", headers=headers())
            if r.status_code != 200:
                print("Error:", r.json().get("error"))
                continue
            print("\n--- Lista de usuarios ---")
            for u in r.json():
                print(f"{u['_id']} - {u['nombre']} {u['apellido']} ({u['rol']})")

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

        elif op == "3":
            uid = input("ID del usuario a modificar: ")
            print("\nIntroduce los nuevos valores (deja vacío para no cambiar):")
            nuevo_nombre   = input("Nuevo nombre: ")
            nuevo_apellido = input("Nuevo apellido: ")
            nuevo_email    = input("Nuevo email: ")
            nuevo_rol      = input("Nuevo rol (user/admin): ")
            nuevas_fichas  = input("Nuevas fichas: ")
            nuevos_pokes   = input("Nuevos pokes: ")

            payload = {}
            if nuevo_nombre.strip():   payload["nombre"]   = nuevo_nombre
            if nuevo_apellido.strip(): payload["apellido"] = nuevo_apellido
            if nuevo_email.strip():    payload["email"]    = nuevo_email
            if nuevo_rol.strip():      payload["rol"]      = nuevo_rol
            if nuevas_fichas.strip():  payload["fichas"]   = int(nuevas_fichas)
            if nuevos_pokes.strip():   payload["pokes"]    = int(nuevos_pokes)

            if not payload:
                print("No se ha cambiado ningún dato.")
                continue

            r = requests.put(f"{API_URL}/usuarios/{uid}", json=payload, headers=headers())
            if r.status_code != 200:
                print("Error:", r.json().get("error"))
                continue
            print("Usuario modificado correctamente.")

        elif op == "4":
            uid     = input("ID del usuario a eliminar: ")
            confirm = input("¿Seguro que quieres eliminarlo? (s/n): ")
            if confirm.lower() != "s":
                print("Operación cancelada.")
                continue
            r = requests.delete(f"{API_URL}/usuarios/{uid}", headers=headers())
            if r.status_code != 200:
                print("Error:", r.json().get("error"))
                continue
            print("Usuario eliminado correctamente.")

        elif op == "5":
            uid        = input("ID del usuario: ")
            nueva_pass = getpass_module.getpass("Nueva contraseña: ")
            r = requests.put(
                f"{API_URL}/usuarios/{uid}/reset_password",
                json={"password": nueva_pass},
                headers=headers()
            )
            if r.status_code != 200:
                print("Error:", r.json().get("error"))
                continue
            print("Contraseña reseteada correctamente.")

        elif op == "6":
            exportar_usuario()
        elif op == "7":
            exportar_todos()
        elif op == "8":
            importar_desde_json()
        elif op == "9":
            break
        else:
            print("Opción inválida.")


# ============================
#   UTILIDADES ADMIN
# ============================
def exportar_usuario():
    uid = input("ID del usuario a exportar: ").strip()
    r   = requests.get(f"{API_URL}/usuarios/{uid}", headers=headers())
    if r.status_code != 200:
        print("Error:", r.json().get("error"))
        return
    filename = f"usuario_{uid}.json"
    with open(filename, "w", encoding="utf-8") as f:
        json.dump(r.json(), f, ensure_ascii=False, indent=2)
    print(f"Usuario exportado a {filename}")


def exportar_todos():
    r = requests.get(f"{API_URL}/usuarios", headers=headers())
    if r.status_code != 200:
        print("Error:", r.json().get("error"))
        return
    with open("todos_usuarios.json", "w", encoding="utf-8") as f:
        json.dump(r.json(), f, ensure_ascii=False, indent=2)
    print("Todos los usuarios exportados a todos_usuarios.json")


def importar_desde_json():
    filename = input("Ruta del archivo JSON: ").strip()
    try:
        with open(filename, "r", encoding="utf-8") as f:
            data = json.load(f)
    except (FileNotFoundError, json.JSONDecodeError) as e:
        print(f"Error al leer el archivo: {e}")
        return
    usuarios = data if isinstance(data, list) else [data]
    for u in usuarios:
        r      = requests.post(f"{API_URL}/usuarios", json=u, headers=headers())
        estado = "OK" if r.status_code in (200, 201) else r.json().get("error", "Error")
        print(f"  {u.get('username', '?')} → {estado}")


# ============================
#   MENÚ PRINCIPAL
# ============================
def _get_role():
    return (current_user.get("rol") or current_user.get("role") or "user").lower()


def main():
    _load_type_chart()
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
            if _get_role() == "admin":
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


if __name__ == "__main__":
    main()
