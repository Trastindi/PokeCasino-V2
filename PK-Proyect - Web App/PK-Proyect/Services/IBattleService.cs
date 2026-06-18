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
        // ─ Flujo de búsqueda / emparejamiento ────────────────────────────────

        /// <summary>
        /// Crea una batalla desafiando a otro jugador.
        /// Endpoint: POST /battles  Body: {challenger_id, challenged_id}
        /// Devuelve el battle_id creado, o null si hay error.
        /// </summary>
        Task<string?> SendChallengeAsync(string challengerId, string challengedId);

        /// <summary>
        /// Solicita unirse a una batalla existente (jugador 2).
        /// Endpoint: POST /battles/{battleId}/join  Body: {player_id}
        /// Devuelve true si el servidor aceptó.
        /// </summary>
        Task<bool> RequestJoinAsync(string playerId, string battleId);

        /// <summary>
        /// Espera (polling) hasta que la batalla pase a status != "waiting".
        /// Devuelve true si fue aceptada, false si expiró o fue cancelada.
        /// </summary>
        Task<bool> WaitForAcceptanceAsync(string battleId);

        // ─ Estado de la batalla ───────────────────────────────────────────

        /// <summary>
        /// Obtiene el snapshot completo del estado de la batalla.
        /// Endpoint: GET /battles/{battleId}
        /// </summary>
        Task<BattleState?> GetBattleStateAsync(string battleId);

        // ─ Acciones de turno ────────────────────────────────────────────

        /// <summary>
        /// Envía la elección de Pokémon inicial por índice.
        /// Endpoint: POST /battles/{battleId}/choose_pokemon  Body: {pokemon_index}
        /// </summary>
        Task<bool> ChoosePokemonAsync(string battleId, int pokemonIndex);

        /// <summary>
        /// Envía un movimiento del turno actual.
        /// Endpoint: POST /battles/{battleId}/action  Body: {action:{type:move, move_name}}
        /// </summary>
        Task<TurnResult?> UseMoveAsync(string battleId, string moveName);

        /// <summary>
        /// Envía un cambio de Pokémon.
        /// Endpoint: POST /battles/{battleId}/action  Body: {action:{type:switch, pokemon_index}}
        /// </summary>
        Task<bool> SwitchPokemonAsync(string battleId, int pokemonIndex);
    }
}
