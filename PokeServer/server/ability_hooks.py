import random

# ── Evaluación de condiciones ────────────────────────────────────────────────

def _check_condition(cond, ctx):
    t = cond["type"]
    v = cond.get("value")
    if t == "weather_is":        return ctx.get("weather") == v
    if t == "move_type_is":      return ctx.get("move_type") == v
    if t == "move_type_in":      return ctx.get("move_type") in v
    if t == "move_group_is":     return ctx.get("move_group") == v
    if t == "move_category_is":  return ctx.get("move_category") == v
    if t == "move_is_damaging":  return ctx.get("move_is_damaging") == v
    if t == "move_dealt_damage": return ctx.get("move_dealt_damage") == v
    if t == "status_is":         return ctx.get("applied_status") == v
    if t == "status_in":         return ctx.get("applied_status") in v
    if t == "has_major_status":  return bool(ctx.get("target_status")) == v
    if t == "species_is":        return ctx.get("target_species") == v
    if t == "target_has_type":   return v in ctx.get("target_types", [])
    if t == "hp_lte_fraction":   return ctx.get("hp_fraction", 1.0) <= v
    if t == "volatile_flag_is":  return ctx.get("volatile_flags", {}).get(cond["flag"]) == v
    if t == "ally_has_ability":  return ctx.get("ally_ability") == v
    if t == "is_grounded":       return ctx.get("is_grounded") == v
    if t == "battle_format_is":  return ctx.get("battle_format") == v
    if t == "battle_type_is":    return ctx.get("battle_type") == v
    if t == "battle_type_in":    return ctx.get("battle_type") in v
    if t == "source_is_opponent":return ctx.get("source_is_opponent") == v
    if t == "is_opposite_gender":return ctx.get("is_opposite_gender") == v
    return True  # condición desconocida → no bloquea

def _conditions_met(conditions, ctx):
    return all(_check_condition(c, ctx) for c in conditions)


# ── Handlers de efectos ──────────────────────────────────────────────────────

def _handle_effect(effect_type, params, target, ctx, battle_state):
    """
    Aplica el efecto y devuelve un dict con los cambios producidos.
    battle_state se modifica in-place para efectos persistentes.
    """
    result = {"blocked": False, "modified": False, "log": []}

    if effect_type == "modify_stat":
        stat = params["stat"]
        mult = params["multiplier"]
        battle_state["stat_multipliers"][target][stat] = (
            battle_state["stat_multipliers"][target].get(stat, 1.0) * mult
        )
        result["modified"] = True
        result["log"].append(f"[hook] modify_stat {stat} x{mult} para {target}")

    elif effect_type == "modify_move_power":
        battle_state["move_power_multiplier"] *= params["multiplier"]
        result["modified"] = True

    elif effect_type == "modify_damage_taken":
        battle_state["damage_multiplier"] *= params["multiplier"]
        result["modified"] = True

    elif effect_type == "modify_accuracy":
        battle_state["accuracy_multiplier"] *= params["multiplier"]
        result["modified"] = True

    elif effect_type == "modify_evasion":
        battle_state["evasion_multiplier"] *= params["multiplier"]
        result["modified"] = True

    elif effect_type in ("grant_move_immunity", "grant_move_group_immunity",
                         "block_damage_if_not_supereffective"):
        battle_state["move_blocked"] = True
        result["blocked"] = True

    elif effect_type == "block_moves_by_name":
        if ctx.get("move_name") in params.get("moves", []):
            battle_state["move_blocked"] = True
            result["blocked"] = True

    elif effect_type == "block_status":
        battle_state["status_blocked"] = True
        result["blocked"] = True

    elif effect_type == "block_stat_drop":
        battle_state["stat_drop_blocked"] = True
        result["blocked"] = True

    elif effect_type == "block_specific_stat_drop":
        if ctx.get("stat_being_dropped") == params.get("stat"):
            battle_state["stat_drop_blocked"] = True
            result["blocked"] = True

    elif effect_type == "block_flinch":
        battle_state["flinch_blocked"] = True
        result["blocked"] = True

    elif effect_type in ("block_escape", "block_switch"):
        battle_state["escape_blocked"] = True
        result["blocked"] = True

    elif effect_type == "block_forced_switch":
        battle_state["forced_switch_blocked"] = True
        result["blocked"] = True

    elif effect_type == "block_item_removal":
        battle_state["item_removal_blocked"] = True
        result["blocked"] = True

    elif effect_type == "guarantee_escape":
        battle_state["escape_guaranteed"] = True

    elif effect_type == "set_weather":
        battle_state["weather"] = params["weather"]
        battle_state["weather_turns"] = params.get("duration")  # None = permanente

    elif effect_type == "suppress_weather_effects":
        battle_state["weather_suppressed"] = True

    elif effect_type == "set_volatile_flag":
        battle_state["volatile_flags"][target] = battle_state["volatile_flags"].get(target, {})
        battle_state["volatile_flags"][target][params["flag"]] = params["value"]

    elif effect_type == "set_type_from_weather":
        battle_state["type_override"] = battle_state.get("weather", "normal")

    elif effect_type == "change_type_to_move_type":
        battle_state["type_override"] = ctx.get("move_type")

    elif effect_type == "apply_stat_stage_change":
        stat = params["stat"]
        stages = params["stages"]
        battle_state["stat_stages"][target][stat] = (
            battle_state["stat_stages"][target].get(stat, 0) + stages
        )

    elif effect_type == "heal_on_type_hit":
        battle_state["heal_fraction"] = params.get("fraction", 0.25)
        if params.get("grant_immunity"):
            battle_state["move_blocked"] = True
            result["blocked"] = True

    elif effect_type == "heal_fraction_max_hp":
        battle_state["heal_fraction"] = params.get("fraction", 0.0625)

    elif effect_type == "invert_drain_to_damage":
        battle_state["drain_inverted"] = True

    elif effect_type == "increase_pp_consumption_against_self":
        battle_state["extra_pp_cost"] = params.get("extra_pp", 1)

    elif effect_type == "apply_status_chance":
        if random.random() < params.get("chance", 0):
            battle_state["pending_status"] = {"target": target, "status": params["status"]}

    elif effect_type == "apply_random_status_chance":
        if random.random() < params.get("chance", 0):
            chosen = random.choice(params.get("statuses", []))
            battle_state["pending_status"] = {"target": target, "status": chosen}

    elif effect_type == "reflect_status_to_source":
        battle_state["reflect_status"] = True

    elif effect_type == "cure_major_status_chance":
        if random.random() < params.get("chance", 0):
            battle_state["cure_status"] = True

    elif effect_type == "cure_major_status":
        battle_state["cure_status"] = True

    elif effect_type == "copy_opponent_ability":
        battle_state["copied_ability"] = ctx.get("opponent_ability")

    elif effect_type == "prevent_critical_hit":
        battle_state["crit_blocked"] = True

    elif effect_type == "prevent_recoil_damage":
        battle_state["recoil_blocked"] = True

    elif effect_type == "suppress_burn_attack_penalty":
        battle_state["burn_penalty_suppressed"] = True

    elif effect_type == "modify_secondary_effect_chance":
        battle_state["secondary_chance_multiplier"] *= params.get("multiplier", 1.0)

    elif effect_type == "block_secondary_effects_from_moves":
        battle_state["secondary_effects_blocked"] = True

    elif effect_type == "reduce_sleep_duration":
        battle_state["sleep_duration_multiplier"] = params.get("multiplier", 0.5)

    elif effect_type == "deal_fraction_max_hp_damage":
        battle_state["contact_damage_fraction"] = params.get("fraction", 0.0625)

    elif effect_type == "skip_turn_every_other_turn":
        battle_state["skip_turn"] = not battle_state.get("skip_turn", False)

    elif effect_type == "redirect_move_to_self":
        battle_state["redirect_target"] = target

    return result


# ── Función principal ────────────────────────────────────────────────────────

def apply_hooks(phase: str, abilities: list[dict], ctx: dict, battle_state: dict) -> dict:
    """
    Evalúa y aplica todos los hooks de `phase` para las habilidades dadas.

    Parámetros:
      phase        — p.ej. "move_power", "receive_move", "turn_end"
      abilities    — lista de dicts de habilidad de MongoDB (con campo "hooks")
      ctx          — contexto inmutable del momento (move_type, weather, hp_fraction, ...)
      battle_state — dict mutable que acumula modificadores del turno

    Devuelve battle_state modificado.
    """
    for ability in abilities:
        for hook in ability.get("hooks", []):
            if hook["phase"] != phase:
                continue
            if not _conditions_met(hook.get("conditions", []), ctx):
                continue
            # Ordenar por priority (menor = primero)
            _handle_effect(
                hook["effectType"],
                hook.get("params", {}),
                hook.get("target", "self"),
                ctx,
                battle_state
            )
    return battle_state