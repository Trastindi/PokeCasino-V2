using System.Threading.Tasks;

namespace PK_Proyect.Services
{
    /// <summary>
    /// Interfaz para el servicio de batalla que gestiona desafíos y unirse a batallas.
    /// </summary>
    public interface IBattleService
    {
        /// <summary>
        /// Envía un desafío a otro usuario.
        /// Devuelve el battle_id creado, o null si falla.
        /// </summary>
        Task<string> SendChallengeAsync(string currentUserId, string targetUserId);

        /// <summary>
        /// Solicita unirse a una batalla existente.
        /// </summary>
        Task<bool> RequestJoinAsync(string currentUserId, string battleId);

        /// <summary>
        /// Espera a que la otra parte acepte (ya sea el desafío o la unión).
        /// </summary>
        Task<bool> WaitForAcceptanceAsync(string battleIdOrUserId);

        /// <summary>
        /// Obtiene los datos de batalla actual (Pokémon, jugador, rival, etc.).
        /// </summary>
        Task<BattleData> GetBattleDataAsync(string battleId);

        /// <summary>
        /// Cancela una batalla pendiente.
        /// </summary>
        Task<bool> CancelBattleAsync(string battleId);
    }

    /// <summary>
    /// Modelo de datos para una batalla en progreso.
    /// </summary>
    public class BattleData
    {
        public string BattleId { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string OpponentId { get; set; }
        public string OpponentName { get; set; }
        
        // Pokémon del jugador
        public int PlayerPokemonId { get; set; }
        public string PlayerPokemonName { get; set; }
        public int PlayerLevel { get; set; }
        public int PlayerHp { get; set; }
        public int PlayerMaxHp { get; set; }
        public string PlayerSpriteUrl { get; set; }

        // Pokémon del rival
        public int OpponentPokemonId { get; set; }
        public string OpponentPokemonName { get; set; }
        public int OpponentLevel { get; set; }
        public int OpponentHp { get; set; }
        public int OpponentMaxHp { get; set; }
        public string OpponentSpriteUrl { get; set; }

        // Estados
        public bool IsPlayerTurn { get; set; }
        public string BattleStatus { get; set; } // "waiting", "active", "finished"
        public string WinnerId { get; set; } // null si sigue en progreso
    }
}
