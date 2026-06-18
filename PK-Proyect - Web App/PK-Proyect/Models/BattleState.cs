using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PK_Proyect.Models
{
    public class BattleState
    {
        [JsonPropertyName("_id")]         public string  Id         { get; set; } = string.Empty;
        [JsonPropertyName("battle_id")]   public string  BattleId   { get; set; } = string.Empty;
        [JsonPropertyName("status")]      public string  Status     { get; set; } = string.Empty;
        [JsonPropertyName("player1_id")]  public string  Player1Id  { get; set; } = string.Empty;
        [JsonPropertyName("player2_id")]  public string  Player2Id  { get; set; } = string.Empty;

        // El servidor guarda el equipo como objeto { team_name, pokemon: [...] }
        [JsonPropertyName("player1_team")] public BattleTeam? Player1TeamData { get; set; }
        [JsonPropertyName("player2_team")] public BattleTeam? Player2TeamData { get; set; }

        // Pokemon activo durante la batalla
        [JsonPropertyName("player1_pokemon")] public BattlePokemon? Player1Pokemon { get; set; }
        [JsonPropertyName("player2_pokemon")] public BattlePokemon? Player2Pokemon { get; set; }

        [JsonPropertyName("turn_log")]       public List<string> TurnLog      { get; set; } = new();
        [JsonPropertyName("winner_id")]      public string?      WinnerId     { get; set; }
        [JsonPropertyName("switch_turn_id")] public string?      SwitchTurnId { get; set; }

        // ─ Helpers ────────────────────────────────────────────────────────────

        public string GetOpponentId(string myId)
            => myId == Player1Id ? Player2Id : Player1Id;

        public BattlePokemon? GetActivePokemonOf(string playerId)
            => playerId == Player1Id ? Player1Pokemon : Player2Pokemon;

        public List<BattlePokemon> GetTeamOf(string playerId)
        {
            var teamData = playerId == Player1Id ? Player1TeamData : Player2TeamData;
            if (teamData?.Pokemon?.Count > 0) return teamData.Pokemon;
            // Fallback: si hay pokemon activo, devolver lista de uno
            var active = GetActivePokemonOf(playerId);
            return active != null ? new List<BattlePokemon> { active } : new List<BattlePokemon>();
        }

        public bool HasTeam(string playerId)
        {
            var teamData = playerId == Player1Id ? Player1TeamData : Player2TeamData;
            return teamData?.Pokemon?.Count > 0;
        }
    }

    /// <summary>Representa el equipo tal como lo guarda el servidor: { team_name, pokemon: [...] }</summary>
    public class BattleTeam
    {
        [JsonPropertyName("team_name")] public string              TeamName { get; set; } = string.Empty;
        [JsonPropertyName("pokemon")]   public List<BattlePokemon> Pokemon  { get; set; } = new();
    }

    public class BattlePokemon
    {
        [JsonPropertyName("pokemon_id")] public string         PokemonId { get; set; } = string.Empty;
        [JsonPropertyName("name")]        public string         Name      { get; set; } = string.Empty;
        [JsonPropertyName("level")]       public int            Level     { get; set; } = 5;
        [JsonPropertyName("hp_current")]  public int            HpCurrent { get; set; }
        [JsonPropertyName("hp_max")]      public int            HpMax     { get; set; } = 1;
        [JsonPropertyName("sprite_url")]  public string         SpriteUrl { get; set; } = string.Empty;
        [JsonPropertyName("moves")]       public List<MoveModel> Moves    { get; set; } = new();
    }

    public class TurnResult
    {
        [JsonPropertyName("log")]          public List<string> Log        { get; set; } = new();
        [JsonPropertyName("player_hp")]    public int          PlayerHp   { get; set; }
        [JsonPropertyName("opponent_hp")]  public int          OpponentHp { get; set; }
        [JsonPropertyName("status")]       public string?      Status     { get; set; }
        [JsonPropertyName("battle_state")] public BattleState? NewState   { get; set; }
    }
}
