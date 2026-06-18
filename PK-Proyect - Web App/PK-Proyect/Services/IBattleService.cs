using PK_Proyect.Models;
using System.Threading.Tasks;

namespace PK_Proyect.Services
{
    /// <summary>
    /// Contrato de comunicación con el servidor Python para el combate.
    /// Todos los métodos son fire-and-forget seguros: nunca lanzan excepción al caller,
    /// devuelven null/false en caso de error de red.
    /// </summary>
    public interface IBattleService
    {
        /// <summary>
        /// Obtiene el snapshot completo del estado de la batalla.
        /// Devuelve null si hay error de red o la batalla no existe.
        /// </summary>
        Task<BattleState?> GetBattleStateAsync(string battleId);

        /// <summary>
        /// Envía la elección de Pokémon inicial (status: ready → servidor espera ambos jugadores).
        /// Devuelve true si el servidor confirmó la recepción.
        /// </summary>
        Task<bool> ChoosePokemonAsync(string battleId, string playerId, string pokemonId);

        /// <summary>
        /// Envía un movimiento del turno actual (status: choosing_action).
        /// Devuelve el resultado del turno (log + nuevos HP) o null si hay error.
        /// </summary>
        Task<TurnResult?> UseMoveAsync(string battleId, string playerId, string moveName);

        /// <summary>
        /// Envía el cambio de Pokémon, tanto voluntario como forzado (status: waiting_switch).
        /// Devuelve true si el servidor confirmó el cambio.
        /// </summary>
        Task<bool> SwitchPokemonAsync(string battleId, string playerId, string pokemonId);
    }
}
