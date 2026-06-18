using System.Collections.Generic;
using System.Threading.Tasks;

namespace PK_Proyect.Services
{
    /// <summary>
    /// Contrato del servicio de batalla: matchmaking, polling de estado y acciones de turno.
    /// </summary>
    public interface IBattleService
    {
        // ── Matchmaking ──────────────────────────────────────────────────────────

        /// <summary>Envía un desafío a otro usuario. Devuelve el battle_id o null.</summary>
        Task<string> SendChallengeAsync(string currentUserId, string targetUserId);

        /// <summary>Solicita unirse a una batalla existente.</summary>
        Task<bool> RequestJoinAsync(string currentUserId, string battleId);

        /// <summary>Polling hasta que status == "active" o timeout.</summary>
        Task<bool> WaitForAcceptanceAsync(string battleIdOrUserId);

        /// <summary>Cancela una batalla pendiente.</summary>
        Task<bool> CancelBattleAsync(string battleId);

        // ── Estado ───────────────────────────────────────────────────────────────

        /// <summary>Obtiene el snapshot completo de la batalla (incluye status).</summary>
        Task<BattleState> GetBattleStateAsync(string battleId);

        // ── Acciones de turno ────────────────────────────────────────────────────

        /// <summary>El jugador elige su Pokémon inicial (status ready → choosing_action).</summary>
        Task<bool> ChoosePokemonAsync(string battleId, string playerId, string pokemonId);

        /// <summary>El jugador usa un movimiento. Devuelve el log del turno.</summary>
        Task<TurnResult> UseMoveAsync(string battleId, string playerId, string moveId);

        /// <summary>El jugador cambia de Pokémon (voluntario o forzado por waiting_switch).</summary>
        Task<bool> SwitchPokemonAsync(string battleId, string playerId, string pokemonId);

        // ── Compat. retrocompat ──────────────────────────────────────────────────

        /// <summary>Alias mantenido para no romper código existente.</summary>
        Task<BattleData> GetBattleDataAsync(string battleId);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Modelos
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Snapshot completo de la batalla tal como lo devuelve GET /battles/{id}.
    /// </summary>
    public class BattleState
    {
        public string   BattleId      { get; set; }

        /// <summary>"ready" | "choosing_action" | "waiting_switch" | "finished"</summary>
        public string   Status        { get; set; }

        public string   Player1Id     { get; set; }
        public string   Player2Id     { get; set; }

        /// <summary>Id del jugador cuyo turno es. Null si no aplica.</summary>
        public string   CurrentTurnId { get; set; }

        /// <summary>Id del jugador que debe cambiar forzosamente. Null si no aplica.</summary>
        public string   SwitchTurnId  { get; set; }

        /// <summary>Id del ganador. Null mientras la batalla sigue.</summary>
        public string   WinnerId      { get; set; }

        public PokemonSnapshot Player1Pokemon { get; set; }
        public PokemonSnapshot Player2Pokemon { get; set; }

        public List<string> TurnLog { get; set; } = new();
    }

    /// <summary>Datos mínimos del Pokémon activo de cada jugador.</summary>
    public class PokemonSnapshot
    {
        public string   PokemonId   { get; set; }
        public string   Name        { get; set; }
        public int      Level       { get; set; }
        public int      HpCurrent   { get; set; }
        public int      HpMax       { get; set; }
        public string   SpriteUrl   { get; set; }
        public List<MoveSnapshot> Moves { get; set; } = new();
    }

    public class MoveSnapshot
    {
        public string MoveId  { get; set; }
        public string Name    { get; set; }
        public string Type    { get; set; }
        public int    Power   { get; set; }
        public int    Pp      { get; set; }
        public int    MaxPp   { get; set; }
    }

    /// <summary>Resultado devuelto tras procesar un turno.</summary>
    public class TurnResult
    {
        public bool         Success      { get; set; }
        public List<string> Log          { get; set; } = new();
        public string       NewStatus    { get; set; }
        public string       WinnerId     { get; set; }
    }

    // ── Compat. legacy ────────────────────────────────────────────────────────────
    /// <summary>Mantenido para no romper código existente que usa BattleData.</summary>
    public class BattleData
    {
        public string BattleId              { get; set; }
        public string PlayerId              { get; set; }
        public string PlayerName            { get; set; }
        public string OpponentId            { get; set; }
        public string OpponentName          { get; set; }
        public int    PlayerPokemonId       { get; set; }
        public string PlayerPokemonName     { get; set; }
        public int    PlayerLevel           { get; set; }
        public int    PlayerHp              { get; set; }
        public int    PlayerMaxHp           { get; set; }
        public string PlayerSpriteUrl       { get; set; }
        public int    OpponentPokemonId     { get; set; }
        public string OpponentPokemonName   { get; set; }
        public int    OpponentLevel         { get; set; }
        public int    OpponentHp            { get; set; }
        public int    OpponentMaxHp         { get; set; }
        public string OpponentSpriteUrl     { get; set; }
        public bool   IsPlayerTurn          { get; set; }
        public string BattleStatus          { get; set; }
        public string WinnerId              { get; set; }
    }
}
