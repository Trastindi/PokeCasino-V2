using PK_Proyect.Models;
using System.Threading.Tasks;

namespace PK_Proyect.Services
{
    /// <summary>
    /// Contrato de comunicación con el servidor Python para el combate.
    /// Todos los métodos son fire-and-forget seguros: nunca lanzan excepción al caller,
    /// devuelven null/false en caso de error de red.
    ///
    /// Endpoints reales de app.py:
    ///   GET  /battles/{id}                  → estado completo de la batalla
    ///   POST /battles/{id}/teams            → enviar equipo (status: lobby)
    ///   POST /battles/{id}/choose_pokemon   → elegir Pokémon inicial (status: ready)
    ///   POST /battles/{id}/action           → enviar acción de turno (status: choosing_action)
    ///                                         body: { "action": { "type": "move"|"switch", ... } }
    /// </summary>
    public interface IBattleService
    {
        /// <summary>
        /// Obtiene el snapshot completo del estado de la batalla.
        /// Endpoint: GET /battles/{battleId}
        /// Devuelve null si hay error de red o la batalla no existe.
        /// </summary>
        Task<BattleState?> GetBattleStateAsync(string battleId);

        /// <summary>
        /// Envía la elección de Pokémon inicial por índice dentro del equipo.
        /// Endpoint: POST /battles/{battleId}/choose_pokemon
        /// Body: { "pokemon_index": int }
        /// El servidor identifica al jugador por el JWT — no se envía player_id.
        /// Devuelve true si el servidor confirmó la recepción.
        /// </summary>
        Task<bool> ChoosePokemonAsync(string battleId, int pokemonIndex);

        /// <summary>
        /// Envía un movimiento del turno actual.
        /// Endpoint: POST /battles/{battleId}/action
        /// Body: { "action": { "type": "move", "move_name": string } }
        /// Devuelve el resultado del turno (log + nuevos HP) o null si hay error.
        /// </summary>
        Task<TurnResult?> UseMoveAsync(string battleId, string moveName);

        /// <summary>
        /// Envía un cambio de Pokémon (voluntario o forzado).
        /// Endpoint: POST /battles/{battleId}/action
        /// Body: { "action": { "type": "switch", "pokemon_index": int } }
        /// Devuelve true si el servidor confirmó el cambio.
        /// </summary>
        Task<bool> SwitchPokemonAsync(string battleId, int pokemonIndex);
    }
}
