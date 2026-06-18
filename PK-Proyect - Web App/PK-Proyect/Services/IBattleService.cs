using PK_Proyect.Models;
using System.Threading.Tasks;

namespace PK_Proyect.Services
{
    /// <summary>
    /// Contrato de comunicación con el servidor Python para el combate.
    ///
    /// Endpoints reales de app.py:
    ///   POST /battles                       → crear batalla / desafío
    ///   POST /battles/{id}/join             → unirse a una batalla existente
    ///   GET  /battles/{id}                  → estado completo
    ///   POST /battles/{id}/teams            → enviar equipo
    ///   POST /battles/{id}/choose_pokemon   → elegir Pokémon inicial
    ///   POST /battles/{id}/action           → acción de turno
    /// </summary>
    public interface IBattleService
    {
        // ─ Flujo de búsqueda / emparejamiento ──────────────────────────────────

        /// <summary>Crea una batalla desafiando a otro jugador. Devuelve battle_id o null.</summary>
        Task<string?> SendChallengeAsync(string challengerId, string challengedId);

        /// <summary>Solicita unirse a una batalla existente (jugador 2).</summary>
        Task<bool> RequestJoinAsync(string playerId, string battleId);

        /// <summary>Espera (polling) hasta que la batalla pase a status != "waiting".</summary>
        Task<bool> WaitForAcceptanceAsync(string battleId);

        // ─ Estado de la batalla ───────────────────────────────────────────

        /// <summary>Obtiene el snapshot completo del estado de la batalla.</summary>
        Task<BattleState?> GetBattleStateAsync(string battleId);

        // ─ Equipo ────────────────────────────────────────────────────────

        /// <summary>
        /// Envía el equipo elegido al servidor.
        /// Endpoint: POST /battles/{battleId}/teams  Body: {team_id}
        /// Devuelve true si el servidor aceptó.
        /// </summary>
        Task<bool> SubmitTeamAsync(string battleId, string teamId);

        // ─ Acciones de turno ──────────────────────────────────────────

        /// <summary>Envía la elección de Pokémon inicial por índice.</summary>
        Task<bool> ChoosePokemonAsync(string battleId, int pokemonIndex);

        /// <summary>Envía un movimiento del turno actual.</summary>
        Task<TurnResult?> UseMoveAsync(string battleId, string moveName);

        /// <summary>Envía un cambio de Pokémon.</summary>
        Task<bool> SwitchPokemonAsync(string battleId, int pokemonIndex);
    }
}
