# -*- coding: utf-8 -*-
import requests
import time
import os

API_URL  = "http://127.0.0.1:5000"
token    = None
current_user = {}
batalla  = {}

# Colores ANSI
RED    = "\033[91m"
GREEN  = "\033[92m"
YELLOW = "\033[93m"
CYAN   = "\033[96m"
BOLD   = "\033[1m"
RESET  = "\033[0m"

def headers():
    return {"Authorization": f"Bearer {token}"} if token else {}

# ─────────────────────────────────────────────
#  AUTH
# ─────────────────────────────────────────────

def login():
    global token, current_user
    print("\n--- Login ---")
    username = input("Usuario: ").strip()
    password = input("Contraseña: ").strip()
    r = requests.post(f"{API_URL}/auth/login", json={"username": username, "password": password})
    if r.status_code == 200:
        data  = r.json()
        token = data.get("token")
        current_user = data.get("user", {})
        print(f"{GREEN}Bienvenido, {current_user.get('Username', username)}!{RESET}")
    else:
        print(f"{RED}Error: {r.json().get('error', 'Login fallido')}{RESET}")

def register():
    global token, current_user
    print("\n--- Registro ---")
    username = input("Usuario: ").strip()
    password = input("Contraseña: ").strip()
    email    = input("Email: ").strip()
    r = requests.post(f"{API_URL}/auth/register",
                      json={"username": username, "password": password, "email": email})
    if r.status_code == 201:
        data  = r.json()
        token = data.get("token")
        current_user = data.get("user", {})
        print(f"{GREEN}Registro exitoso. Bienvenido, {username}!{RESET}")
    else:
        print(f"{RED}Error: {r.json().get('error', 'Registro fallido')}{RESET}")

# ─────────────────────────────────────────────
#  POKÉDEX
# ─────────────────────────────────────────────

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

        data   = r.json()
        lista  = data.get("pokemons", data) if isinstance(data, dict) else data
        total  = data.get("total", len(lista)) if isinstance(data, dict) else len(lista)

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
    p = r.json()
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

# ─────────────────────────────────────────────
#  MIS POKÉMON
# ─────────────────────────────────────────────

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
        sel   = int(sel) - 1
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

# ─────────────────────────────────────────────
#  EQUIPOS
# ─────────────────────────────────────────────

def ver_equipos():
    r = requests.get(f"{API_URL}/users/pokemonteams", headers=headers())
    if r.status_code != 200:
        print(f"{RED}Error: {r.json().get('error')}{RESET}")
        return
    equipos = r.json()
    if not equipos:
        print("\nNo tienes equipos creados.")
        return
    print("\n--- Mis Equipos ---")
    for t in equipos:
        ids = t.get("pokemon_ids", [])
        print(f"  [{t.get('_id','')}] {t['team_name']}  ({len(ids)}/6 Pokémon)  IDs: {ids}")

# ─────────────────────────────────────────────
#  TIENDA / PREMIOS
# ─────────────────────────────────────────────

def ver_tienda():
    r = requests.get(f"{API_URL}/tienda", headers=headers())
    if r.status_code != 200:
        print(f"{RED}Error al obtener la tienda{RESET}")
        return
    items = r.json()
    print("\n--- Tienda ---")
    for item in items:
        print(f"  {item.get('_id','')}  {item.get('Nombre','?')}  "
              f"Precio: {item.get('Precio','?')} monedas")

def comprar_item(item_id):
    r = requests.post(f"{API_URL}/tienda/comprar",
                      json={"item_id": item_id}, headers=headers())
    if r.status_code == 200:
        print(f"{GREEN}Compra realizada: {r.json().get('msg','')}{RESET}")
    else:
        print(f"{RED}Error: {r.json().get('error')}{RESET}")

def ver_premios():
    r = requests.get(f"{API_URL}/premios", headers=headers())
    if r.status_code != 200:
        print(f"{RED}Error al obtener premios{RESET}")
        return
    premios = r.json()
    print("\n--- Premios disponibles ---")
    for pr in premios:
        print(f"  {pr.get('Nombre','?')}  —  {pr.get('Descripcion','')}")

# ─────────────────────────────────────────────
#  MENSAJES
# ─────────────────────────────────────────────

def ver_mensajes():
    r = requests.get(f"{API_URL}/messages", headers=headers())
    if r.status_code != 200:
        print(f"{RED}Error al obtener mensajes{RESET}")
        return
    msgs = r.json()
    if not msgs:
        print("\nNo tienes mensajes.")
        return
    print("\n--- Mensajes ---")
    for i, m in enumerate(msgs, 1):
        tipo   = m.get("type", "msg")
        estado = f" {YELLOW}[respondido]{RESET}" if m.get("responded") else ""
        print(f"  {i}. {CYAN}[{tipo}]{RESET}{estado}  "
              f"De: {m.get('sender_username','?')}  "
              f"— {m.get('content','')[:60]}")

def enviar_mensaje():
    dest    = input("Destinatario (username): ").strip()
    contenido = input("Mensaje: ").strip()
    r = requests.post(f"{API_URL}/messages",
                      json={"recipient_username": dest, "content": contenido},
                      headers=headers())
    if r.status_code == 201:
        print(f"{GREEN}Mensaje enviado.{RESET}")
    else:
        print(f"{RED}Error: {r.json().get('error')}{RESET}")

# ─────────────────────────────────────────────
#  BATALLA
# ─────────────────────────────────────────────

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
            # Esperar a que el servidor procese ambas elecciones antes de continuar
            print(f"{YELLOW}Esperando confirmación...{RESET}")
            while True:
                time.sleep(1.5)
                batalla   = obtener_batalla(battle_id)
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
                # Salir cuando el turno haya avanzado O la batalla haya terminado
                if new_status == "finished":
                    break
                if new_status == "choosing_action" and new_turno > turno_actual:
                    break
                if new_status == "ready":  # algún Pokémon fainted
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

    my_active  = b.get(my_slot,  {}).get("active_pokemon", {})
    en_active  = b.get(en_slot,  {}).get("active_pokemon", {})

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


# ─────────────────────────────────────────────
#  CASINO (ruleta, slots, blackjack…)
# ─────────────────────────────────────────────

def jugar_ruleta():
    apuesta = input("Apuesta (monedas): ").strip()
    numero  = input("Número (0-36): ").strip()
    try:
        apuesta = int(apuesta)
        numero  = int(numero)
    except ValueError:
        print("Valores inválidos.")
        return
    r = requests.post(f"{API_URL}/casino/ruleta",
                      json={"apuesta": apuesta, "numero": numero},
                      headers=headers())
    if r.status_code == 200:
        data = r.json()
        print(f"Resultado: {data.get('resultado','?')}  "
              f"Ganancia: {data.get('ganancia','?')} monedas")
    else:
        print(f"{RED}Error: {r.json().get('error')}{RESET}")

def jugar_slots():
    apuesta = input("Apuesta (monedas): ").strip()
    try:
        apuesta = int(apuesta)
    except ValueError:
        print("Valor inválido.")
        return
    r = requests.post(f"{API_URL}/casino/slots",
                      json={"apuesta": apuesta},
                      headers=headers())
    if r.status_code == 200:
        data = r.json()
        print(f"Resultado: {data.get('resultado','?')}  "
              f"Ganancia: {data.get('ganancia','?')} monedas")
    else:
        print(f"{RED}Error: {r.json().get('error')}{RESET}")

# ─────────────────────────────────────────────
#  MENÚS
# ─────────────────────────────────────────────

def menu_principal():
    while True:
        print(f"\n{'═'*40}")
        print(f"  {BOLD}PokéCasino CLI{RESET}")
        print(f"{'═'*40}")
        if token:
            print(f"  Sesión: {GREEN}{current_user.get('Username','?')}{RESET}")
        print("  1. Login")
        print("  2. Registro")
        print("  3. Pokédex")
        if token:
            print("  4. Menú usuario")
        print("  0. Salir")
        op = input("Opción: ").strip()

        if op == "1":
            login()
        elif op == "2":
            register()
        elif op == "3":
            pokedex_menu()
        elif op == "4" and token:
            menu_usuario()
        elif op == "0":
            print("¡Hasta luego!")
            break
        else:
            print("Opción no válida.")


def menu_usuario():
    global batalla
    equipo_enviado = False
    battle_id_activo = None

    while True:
        print(f"\n{'─'*40}")
        print(f"  {BOLD}Menú Usuario{RESET}  —  {current_user.get('Username','?')}")
        print(f"  Monedas: {current_user.get('Monedas', '?')}")
        print(f"{'─'*40}")
        print("  1. Ver perfil")
        print("  2. Pokédex")
        print("  3. Mis Pokémon")
        print("  4. Mis equipos")
        print("  5. Mensajes")
        print("  6. Tienda")
        print("  7. Premios")
        print("  8. Ruleta")
        print("  9. Slots")
        print(" 10. Batalla PvP")
        print("  0. Cerrar sesión")
        op = input("Opción: ").strip()

        if op == "1":
            print(f"\n  Usuario : {current_user.get('Username','?')}")
            print(f"  Email   : {current_user.get('Email','?')}")
            print(f"  Monedas : {current_user.get('Monedas','?')}")
            print(f"  Nivel   : {current_user.get('Nivel','?')}")

        elif op == "2":
            pokedex_menu()

        elif op == "3":
            mis_pokemon_menu()

        elif op == "4":
            ver_equipos()

        elif op == "5":
            ver_mensajes()
            print("\n  [e] Enviar mensaje  [cualquier tecla] Volver")
            if input("  > ").strip().lower() == "e":
                enviar_mensaje()

        elif op == "6":
            ver_tienda()
            print("\n  ¿Comprar algún ítem? (ID o ENTER para volver)")
            item_id = input("  > ").strip()
            if item_id:
                comprar_item(item_id)

        elif op == "7":
            ver_premios()

        elif op == "8":
            jugar_ruleta()

        elif op == "9":
            jugar_slots()

        elif op == "10":
            print("\n--- Estás en una batalla ---")
            # Fase 1: seleccionar y enviar equipo
            if not equipo_enviado:
                print("Introduce el nombre de tu equipo para empezar a jugar")
                while not equipo_enviado:
                    nombre_equipo = input("Nombre del equipo: ").strip()
                    if not nombre_equipo:
                        break
                    # Verificar que el equipo existe
                    r_equipos = requests.get(f"{API_URL}/users/pokemonteams", headers=headers())
                    if r_equipos.status_code != 200:
                        print(f"{RED}Error al obtener equipos.{RESET}")
                        break
                    equipos = r_equipos.json()
                    equipo_sel = next(
                        (e for e in equipos if e.get("team_name","").lower() == nombre_equipo.lower()),
                        None
                    )
                    if not equipo_sel:
                        print(f"{RED}Equipo '{nombre_equipo}' no encontrado. Inténtalo de nuevo.{RESET}")
                        continue
                    print(f"{GREEN}Equipo '{nombre_equipo}' seleccionado. Enviando datos al servidor...{RESET}")
                    r_join = requests.post(
                        f"{API_URL}/battles/join",
                        json={"team_name": nombre_equipo},
                        headers=headers()
                    )
                    if r_join.status_code in (200, 201):
                        battle_data = r_join.json()
                        battle_id_activo = battle_data.get("battle_id") or battle_data.get("_id")
                        print(f"{GREEN}¡Equipo enviado! Esperando al rival...{RESET}")
                        equipo_enviado = True
                    else:
                        print(f"{RED}Error al unirse a batalla: {r_join.json().get('error')}{RESET}")
                        break

            # Fase 2: polling hasta que el rival se una
            if equipo_enviado and battle_id_activo:
                batalla = obtener_batalla(battle_id_activo)
                while batalla.get("status") == "pending":
                    print(f"{YELLOW}Esperando al rival... (estado: {batalla.get('status','?')}){RESET}")
                    time.sleep(2)
                    batalla = obtener_batalla(battle_id_activo)

                if batalla and batalla.get("status") in ("ready", "choosing_action"):
                    batalla_loop(battle_id_activo)
                    equipo_enviado   = False
                    battle_id_activo = None
                elif batalla.get("status") == "finished":
                    print("La batalla ya ha terminado.")
                    equipo_enviado   = False
                    battle_id_activo = None

        elif op == "0":
            break
        else:
            print("Opción no válida.")


# ─────────────────────────────────────────────
#  ENTRY POINT
# ─────────────────────────────────────────────

if __name__ == "__main__":
    menu_principal()
