using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PK_Proyect.Models
{
    /// <summary>
    /// Snapshot completo de una batalla, tal como lo devuelve el servidor.
    /// Campos en snake_case mapeados con JsonPropertyName para no depender
    /// de opciones globales de deserialización.
    /// </summary>
    public class BattleState
    {
        // ── Metadatos de la batalla ───────────────────────────────────────────────

        [JsonPropertyName("battle_id")]
        public string BattleId { get; set; } = string.Empty;

        /// <summary>ready | choosing_action | waiting_switch | finished</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "ready";

        [JsonPropertyName("player1_id")]
        public string Player1Id { get; set; } = string.Empty;

        [JsonPropertyName("player2_id")]
        public string Player2Id { get; set; } = string.Empty;

        // ── Pokémon activos ───────────────────────────────────────────────────────

        [JsonPropertyName("player1_pokemon")]
        public BattlePokemon? Player1Pokemon { get; set; }

        [JsonPropertyName("player2_pokemon")]
        public BattlePokemon? Player2Pokemon { get; set; }

        // ── Log del último turno ─────────────────────────────────────────────────

        /// <summary>Líneas de texto del último turno procesado.</summary>
        [JsonPropertyName("turn_log")]
        public List<string> TurnLog { get; set; } = new();

        // ── Estado de fin ─────────────────────────────────────────────────────────

        /// <summary>Id del jugador ganador. Solo se rellena cuando status == finished.</summary>
        [JsonPropertyName("winner_id")]
        public string? WinnerId { get; set; }

        // ── Estado de cambio forzado ──────────────────────────────────────────────

        /// <summary>
        /// Id del jugador que debe elegir Pokémon (status == waiting_switch).
        /// Null si ambos jugadores ya tienen Pokémon disponible.
        /// </summary>
        [JsonPropertyName("switch_turn_id")]
        public string? SwitchTurnId { get; set; }
    }

    /// <summary>
    /// Datos del Pokémon activo de un jugador dentro de una batalla.
    /// </summary>
    public class BattlePokemon
    {
        [JsonPropertyName("pokemon_id")]
        public string PokemonId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("level")]
        public int Level { get; set; } = 5;

        [JsonPropertyName("hp_current")]
        public int HpCurrent { get; set; }

        [JsonPropertyName("hp_max")]
        public int HpMax { get; set; } = 1;

        [JsonPropertyName("sprite_url")]
        public string SpriteUrl { get; set; } = string.Empty;

        [JsonPropertyName("moves")]
        public List<MoveModel> Moves { get; set; } = new();
    }

    /// <summary>
    /// Resultado que devuelve el servidor tras procesar un turno (use_move).
    /// </summary>
    public class TurnResult
    {
        [JsonPropertyName("log")]
        public List<string> Log { get; set; } = new();

        /// <summary>Nuevo HP del jugador tras el turno.</summary>
        [JsonPropertyName("player_hp")]
        public int PlayerHp { get; set; }

        /// <summary>Nuevo HP del rival tras el turno.</summary>
        [JsonPropertyName("opponent_hp")]
        public int OpponentHp { get; set; }

        /// <summary>Nuevo estado de la batalla tras el turno.</summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}
