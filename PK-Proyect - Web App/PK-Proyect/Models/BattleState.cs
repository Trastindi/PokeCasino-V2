using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PK_Proyect.Models
{
    public class BattleState
    {
        [JsonPropertyName("battle_id")]   public string  BattleId   { get; set; } = string.Empty;
        [JsonPropertyName("status")]      public string  Status     { get; set; } = "ready";
        [JsonPropertyName("player1_id")]  public string  Player1Id  { get; set; } = string.Empty;
        [JsonPropertyName("player2_id")]  public string  Player2Id  { get; set; } = string.Empty;

        [JsonPropertyName("player1_pokemon")] public BattlePokemon? Player1Pokemon { get; set; }
        [JsonPropertyName("player2_pokemon")] public BattlePokemon? Player2Pokemon { get; set; }

        /// <summary>Líneas de texto que describen lo que ocurrió en el último turno.</summary>
        [JsonPropertyName("turn_log")]    public List<string> TurnLog   { get; set; } = new();

        [JsonPropertyName("winner_id")]   public string? WinnerId      { get; set; }
        [JsonPropertyName("switch_turn_id")] public string? SwitchTurnId { get; set; }

        // ─ Helpers ────────────────────────────────────────────────────────────

        public string GetOpponentId(string myId)
            => myId == Player1Id ? Player2Id : Player1Id;

        public BattlePokemon? GetActivePokemonOf(string playerId)
            => playerId == Player1Id ? Player1Pokemon : Player2Pokemon;

        public List<BattlePokemon> GetTeamOf(string playerId)
        {
            // El servidor devuelve el equipo dentro de player1_team / player2_team (lista).
            // Si solo devuelve el Pokémon activo, construimos una lista de uno.
            if (playerId == Player1Id)
                return Player1Team?.Count > 0 ? Player1Team
                     : Player1Pokemon != null  ? new List<BattlePokemon> { Player1Pokemon }
                     : new List<BattlePokemon>();

            return Player2Team?.Count > 0 ? Player2Team
                 : Player2Pokemon != null  ? new List<BattlePokemon> { Player2Pokemon }
                 : new List<BattlePokemon>();
        }

        // Campos opcionales de equipo completo (si el servidor los incluye)
        [JsonPropertyName("player1_team")] public List<BattlePokemon>? Player1Team { get; set; }
        [JsonPropertyName("player2_team")] public List<BattlePokemon>? Player2Team { get; set; }
    }

    public class BattlePokemon
    {
        [JsonPropertyName("pokemon_id")] public string PokemonId  { get; set; } = string.Empty;
        [JsonPropertyName("name")]        public string Name       { get; set; } = string.Empty;
        [JsonPropertyName("level")]       public int    Level      { get; set; } = 5;
        [JsonPropertyName("hp_current")]  public int    HpCurrent  { get; set; }
        [JsonPropertyName("hp_max")]      public int    HpMax      { get; set; } = 1;
        [JsonPropertyName("sprite_url")]  public string SpriteUrl  { get; set; } = string.Empty;
        [JsonPropertyName("moves")]       public List<MoveModel> Moves { get; set; } = new();
    }

    public class TurnResult
    {
        [JsonPropertyName("log")]        public List<string> Log       { get; set; } = new();
        [JsonPropertyName("player_hp")]  public int          PlayerHp  { get; set; }
        [JsonPropertyName("opponent_hp")] public int         OpponentHp { get; set; }
        [JsonPropertyName("status")]     public string?      Status    { get; set; }

        /// <summary>
        /// Estado completo de la batalla tras el turno.
        /// Puede venir directamente del servidor o construirse a partir de los campos simples.
        /// </summary>
        [JsonPropertyName("battle_state")] public BattleState? NewState { get; set; }
    }
}
