# -*- coding: utf-8 -*-
import requests
import time
import os
import json
import getpass

API_URL  = "http://127.0.0.1:5000"
token    = None
current_user     = {}
batalla          = {}
is_on_battle     = False
equipo_seleccionado = {}

# Colores ANSI
RED    = "\033[91m"
GREEN  = "\033[92m"
YELLOW = "\033[93m"
CYAN   = "\033[96m"
BOLD   = "\033[1m"
RESET  = "\033[0m"

# Tabla de tipos (se carga al inicio)
type_chart = {}

def _load_type_chart():
    path = os.path.join(os.path.dirname(__file__), "type_chart.json")
    global type_chart
    try:
        with open(path, "r", encoding="utf-8") as f:
            type_chart = json.load(f)
    except FileNotFoundError:
        pass

def headers():
    return {"Authorization": f"Bearer {token}"} if token else {}


# ============================
#   AUTH
# ============================

def login():
    global token, current_user
    print("\n--- Login ---")
    username = input("Usuario: ").strip()
    password = getpass.getpass("Contraseña: ")
    r = requests.post(f"{API_URL}/auth/login", json={"username": username, "password": password})
    if r.status_code == 200:
        data         = r.json()
        token        = data.get("token")
        current_user = data.get("user", {})
        print(f"{GREEN}Bienvenido, {current_user.get('Username', username)}!{RESET}")
        return True
    else:
        print(f"{RED}Error: {r.json().get('error', 'Login fallido')}{RESET}")
        return False


def register():
    global token, current_user
    print("\n--- Registro ---")
    username = input("Usuario: ").strip()
    password = getpass.getpass("Contraseña: ")
    email    = input("Email: ").strip()
    r = requests.post(f"{API_URL}/auth/register",
                      json={"username": username, "password": password, "email": email})
    if r.status_code == 201:
        data         = r.json()
        token        = data.get("token")
        current_user = data.get("user", {})
        print(f"{GREEN}Registro exitoso. Bienvenido, {username}!{RESET}")
    else:
        print(f"{RED}Error: {r.json().get('error', 'Registro fallido')}{RESET}")


# ============================
#   POKÉDEX
# ============================

def pokedex_menu():
    page   = 1
    limit  = 20
    search = ""

    while True:
        params = {"page": page, "limit": limit}
        if search:
            params["search"] = search

        r = requests.get(f"{API_URL}/pokedex", params=params, headers=headers())
        if r.status_code != 200:
            print(f"{RED}Error al obtener la Pokédex{RESET}")
            return

        data  = r.json()
        lista = data.get("pokemons", data) if isinstance(data, dict) else data
        total = data.get("total", len(lista)) if isinstance(data, dict) else len(lista)

        print(f"\n{'─'*50}")
        print(f"  {'#':>4}  {'Nombre':<16} {'Tipo'}")
        print(f"{'─'*50}")
        for p in lista:
            tipo2 = f"/{p['TipoSecundario']}" if p.get("TipoSecundario") else ""
            print(f"  {p['PokemonId']:>4}  {p['Nombre']:<16} {p.get('TipoPrincipal','?')}{tipo2}")
        print(f"{'─'*50}")
        print(f"  Página {page}  |  Total: {total}")
        print("\n  [n] Siguiente  [p] Anterior  [b] Buscar  [v] Ver detalle  [q] Volver")

        op = input("Opción: ").strip().lower()
        if op == "n":
            if page * limit < total:
                page += 1
            else:
                print("Ya estás en la última página.")
        elif op == "p":
            if page > 1:
                page -= 1
            else:
                print("Ya estás en la primera página.")
        elif op == "b":
            search = input("Buscar por nombre: ").strip()
            page   = 1
        elif op == "v":
            pid = input("ID del Pokémon: ").strip()
            try:
                pid = int(pid)
            except ValueError:
                print("ID inválido.")
                continue
            _ver_detalle_pokedex(pid)
        elif op == "q":
            return
        else:
            print("Opción no válida.")


def _ver_detalle_pokedex(pokemon_id):
    r = requests.get(f"{API_URL}/pokedex/{pokemon_id}", headers=headers())
    if r.status_code != 200:
        print(f"{RED}Pokémon no encontrado.{RESET}")
        return
    p     = r.json()
    tipo2 = f" / {p['TipoSecundario']}" if p.get("TipoSecundario") else ""
    print(f"\n{'─'*40}")
    print(f"  #{p['PokemonId']}  {BOLD}{p['Nombre']}{RESET}")
    print(f"  Tipo      : {p.get('TipoPrincipal','?')}{tipo2}")
    print(f"  HP base   : {p.get('BaseHp','?')}")
    print(f"  Ataque    : {p.get('BaseAtaque','?')}")
    print(f"  Defensa   : {p.get('BaseDefensa','?')}")
    movs = p.get("MovimientosDisponibles") or []
    if movs:
        print(f"  Movimientos ({len(movs)}):")
        for m in movs[:8]:
            if isinstance(m, dict):
                print(f"    - {m.get('Nombre','?')}  [{m.get('Tipo','?')}]  Poder: {m.get('Poder','?')}")
            else:
                print(f"    - {m}")
    print(f"{'─'*40}")


# ============================
#   MIS POKÉMON
# ============================

def mis_pokemon_menu():
    r = requests.get(f"{API_URL}/usuarios/mis_pokemon", headers=headers())
    if r.status_code != 200:
        print(f"{RED}Error: {r.json().get('error')}{RESET}")
        return

    lista = r.json()
    if not lista:
        print("\nNo tienes ningún Pokémon todavía.")
        return

    print("\n--- Mis Pokémon ---")
    for p in lista:
        tipo2 = f" / {p['TipoSecundario']}" if p.get("TipoSecundario") else ""
        print(f"  {p['PokemonId']}. {p['Nombre']}  "
              f"[{p.get('TipoPrincipal','?')}{tipo2}]  "
              f"Nv.{p.get('Nivel',1)}")

    sel = input("\nSelecciona un Pokémon por ID para ver detalles (o ENTER para volver): ").strip()
    if not sel:
        return
    try:
        sel = int(sel)
    except ValueError:
        print("ID inválido.")
        return

    elegido = next((p for p in lista if p["PokemonId"] == sel), None)
    if not elegido:
        print("Pokémon no encontrado en tu colección.")
        return

    tipo2 = f" / {elegido['TipoSecundario']}" if elegido.get("TipoSecundario") else ""
    print(f"\n--- {elegido['Nombre']} ---")
    print(f"  ID Pokédex : {elegido['PokemonId']}")
    print(f"  Tipo       : {elegido.get('TipoPrincipal','?')}{tipo2}")
    print(f"  Nivel      : {elegido.get('Nivel', 1)}")
    print(f"  HP actual  : {elegido.get('CurrentHp', '?')}")
    moveset = elegido.get("MoveSet") or []
    print(f"  Movimientos: {', '.join(moveset) if moveset else 'Ninguno'}")
    print(f"  Obtenido   : {elegido.get('FechaObtenido','?')}")

    print("\n¿Qué quieres hacer?")
    print("  1. Añadir a un equipo existente")
    print("  2. Crear un nuevo equipo con este Pokémon")
    print("  3. Volver")
    accion = input("Opción: ").strip()

    if accion == "1":
        _añadir_a_equipo_existente(elegido["PokemonId"])
    elif accion == "2":
        _crear_equipo_con_pokemon(elegido["PokemonId"])


def _añadir_a_equipo_existente(pokemon_id):
    r = requests.get(f"{API_URL}/users/pokemonteams", headers=headers())
    if r.status_code != 200:
        print(f"{RED}Error al obtener equipos: {r.json().get('error')}{RESET}")
        return

    equipos = r.json()
    if not equipos:
        print("No tienes equipos creados. Usa la opción 2 para crear uno.")
        return

    disponibles = []
    for t in equipos:
        ids = t.get("pokemon_ids", [])
        if len(ids) >= 6:
            continue
        if pokemon_id in ids:
            continue
        disponibles.append(t)

    if not disponibles:
        print("No hay equipos disponibles "
              "(todos tienen 6 Pokémon o ya incluyen este Pokémon).")
        return

    print("\n--- Equipos disponibles ---")
    for i, t in enumerate(disponibles, 1):
        print(f"  {i}. {t['team_name']}  "
              f"({len(t.get('pokemon_ids',[]))}/6 Pokémon)")

    sel = input("Selecciona el número del equipo: ").strip()
    try:
        sel    = int(sel) - 1
        equipo = disponibles[sel]
    except (ValueError, IndexError):
        print("Selección inválida.")
        return

    nuevos_ids = equipo.get("pokemon_ids", []) + [pokemon_id]
    team_id    = equipo.get("_id") or equipo.get("id")

    r2 = requests.put(
        f"{API_URL}/users/pokemonteams/{team_id}",
        json={"pokemon_ids": nuevos_ids},
        headers=headers()
    )
    if r2.status_code == 200:
        print(f"{GREEN}¡Pokémon añadido al equipo '{equipo['team_name']}'!{RESET}")
    else:
        print(f"{RED}Error: {r2.json().get('error')}{RESET}")


def _crear_equipo_con_pokemon(pokemon_id):
    nombre = input("Nombre para el nuevo equipo: ").strip()
    if not nombre:
        print("El nombre no puede estar vacío.")
        return

    r = requests.post(
        f"{API_URL}/users/pokemonteams",
        json={"team_name": nombre, "pokemon_ids": [pokemon_id]},
        headers=headers()
    )
    if r.status_code == 201:
        print(f"{GREEN}Equipo '{nombre}' creado correctamente "
              f"con el Pokémon {pokemon_id}.{RESET}")
    else:
        print(f"{RED}Error: {r.json().get('error')}{RESET}")


# ============================
#   BATALLA
# ============================

def obtener_batalla(battle_id):
    r = requests.get(f"{API_URL}/battles/{battle_id}", headers=headers())
    if r.status_code == 200:
        return r.json()
    return {}


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


# ============================
#   CASINO / GACHA / CANJE
# ============================

def jugar_casino():
    print("\n--- Casino (Gacha) ---")
    apuesta = input("Fichas a apostar: ").strip()
    try:
        apuesta = int(apuesta)
    except ValueError:
        print("Valor inválido.")
        return
    r = requests.post(f"{API_URL}/casino/gacha",
                      json={"fichas": apuesta}, headers=headers())
    if r.status_code == 200:
        data = r.json()
        print(f"Resultado: {data}")
    else:
        print(f"{RED}Error: {r.json().get('error')}{RESET}")


def canjear_pokemon():
    print("\n--- Canjear Pokémon ---")
    r = requests.get(f"{API_URL}/casino/canje", headers=headers())
    if r.status_code != 200:
        print(f"{RED}Error: {r.json().get('error')}{RESET}")
        return
    opciones = r.json()
    for i, p in enumerate(opciones, 1):
        print(f"  {i}. {p.get('Nombre','?')}  Coste: {p.get('Coste','?')} pokes")
    sel = input("Elige número (o ENTER para cancelar): ").strip()
    if not sel:
        return
    try:
        sel = int(sel) - 1
        elegido = opciones[sel]
    except (ValueError, IndexError):
        print("Selección inválida.")
        return
    r2 = requests.post(f"{API_URL}/casino/canje",
                       json={"pokemon_id": elegido.get("PokemonId")}, headers=headers())
    if r2.status_code == 200:
        print(f"{GREEN}¡Pokémon canjeado!{RESET}")
    else:
        print(f"{RED}Error: {r2.json().get('error')}{RESET}")


def desafiar_usuario(rival_id):
    r = requests.post(f"{API_URL}/battles/challenge",
                      json={"rival_id": rival_id}, headers=headers())
    if r.status_code in (200, 201):
        data = r.json()
        print(f"{GREEN}Desafío enviado. ID batalla: {data.get('battle_id') or data.get('_id')}{RESET}")
    else:
        print(f"{RED}Error: {r.json().get('error')}{RESET}")


# ============================
#   MENSAJES
# ============================

def mis_mensajes_menu():
    r = requests.get(f"{API_URL}/messages", headers=headers())
    if r.status_code != 200:
        print(f"{RED}Error al obtener mensajes{RESET}")
        return
    msgs = r.json()
    if not msgs:
        print("\nNo tienes mensajes.")
        return
    print("\n--- Mis mensajes ---")
    for i, m in enumerate(msgs, 1):
        tipo   = m.get("type", "msg")
        estado = f" {YELLOW}[respondido]{RESET}" if m.get("responded") else ""
        print(f"  {i}. {CYAN}[{tipo}]{RESET}{estado}  "
              f"De: {m.get('sender_username','?')}  "
              f"— {m.get('content','')[:60]}")
    print("\n  [e] Enviar mensaje  [cualquier tecla] Volver")
    if input("  > ").strip().lower() == "e":
        dest      = input("Destinatario (username): ").strip()
        contenido = input("Mensaje: ").strip()
        r2 = requests.post(f"{API_URL}/messages",
                           json={"recipient_username": dest, "content": contenido},
                           headers=headers())
        if r2.status_code == 201:
            print(f"{GREEN}Mensaje enviado.{RESET}")
        else:
            print(f"{RED}Error: {r2.json().get('error')}{RESET}")


# ============================
#   EXPORTAR / IMPORTAR (admin)
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
        r = requests.post(f"{API_URL}/usuarios", json=u, headers=headers())
        estado = "OK" if r.status_code in (200, 201) else r.json().get("error", "Error")
        print(f"  {u.get('username', '?')} → {estado}")


# ============================
#   MENÚ USUARIO
# ============================

def menu_usuario():
    global is_on_battle, batalla, equipo_seleccionado
    while True:
        print(f"{is_on_battle}")
        if is_on_battle == False:
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
        else:
            print("\n--- Estás en una batalla ---")
            print(f"{batalla}")
            print("Introduce el nombre de tu equipo para empezar a jugar")
            team = input("Nombre del equipo: ")
            r = requests.get(f"{API_URL}/users/pokemonteams", headers=headers()).json()
            print(r)
            for equipo in r:
                if equipo["team_name"] == team:
                    equipo_seleccionado = equipo
                    break
            if equipo:
                print(f"Equipo '{equipo}' seleccionado. Enviando datos al servidor para iniciar la batalla...")
                res = requests.post(
                    f"{API_URL}/battles/{batalla['_id']}/teams",
                    json={"team_id": equipo["_id"], "battle_id": batalla["_id"]},
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
            nuevo_nombre    = input("Nuevo nombre: ")
            nuevo_apellido  = input("Nuevo apellido: ")
            nuevo_email     = input("Nuevo email: ")
            nuevo_rol       = input("Nuevo rol (user/admin): ")
            nuevas_fichas   = input("Nuevas fichas: ")
            nuevos_pokes    = input("Nuevos pokes: ")

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

        # 5. Resetear contraseña
        elif op == "5":
            uid        = input("ID del usuario: ")
            nueva_pass = getpass.getpass("Nueva contraseña: ")

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


# ============================
#   MENÚ PRINCIPAL
# ============================

def ver_perfil():
    r = requests.get(f"{API_URL}/usuarios/perfil", headers=headers())
    if r.status_code == 200:
        u = r.json()
        print(f"\n  Usuario : {u.get('Username','?')}")
        print(f"  Email   : {u.get('Email','?')}")
        print(f"  Fichas  : {u.get('Fichas','?')}")
        print(f"  Pokes   : {u.get('Pokes','?')}")
        print(f"  Nivel   : {u.get('Nivel','?')}")
    else:
        # Fallback con datos locales si el endpoint no existe aún
        print(f"\n  Usuario : {current_user.get('Username','?')}")
        print(f"  Email   : {current_user.get('Email','?')}")


def _get_role():
    """Devuelve el rol del usuario actual ignorando mayúsculas/minúsculas."""
    return (current_user.get("Role") or current_user.get("role") or "user").lower()


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
