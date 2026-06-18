using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PK_Proyect.Services
{
    /// <summary>
    /// Implementación HTTP de IBattleService alineada con los endpoints de app.py.
    ///
    /// Mapa de endpoints:
    ///   GET  /battles/{id}                → GetBattleStateAsync
    ///   POST /battles/{id}/choose_pokemon → ChoosePokemonAsync  (body: {pokemon_index})
    ///   POST /battles/{id}/action         → UseMoveAsync        (body: {action:{type:move, move_name}})
    ///   POST /battles/{id}/action         → SwitchPokemonAsync  (body: {action:{type:switch, pokemon_index}})
    ///
    /// Nota: el servidor extrae el jugador del JWT, NO se envía player_id en el body.
    /// </summary>
    public class BattleService : IBattleService
    {
        private readonly HttpClient _http;

        public BattleService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>Usa el HttpClient compartido con token JWT ya configurado.</summary>
        public BattleService() : this(ApiClient.HttpClient) { }

        // ── GET /battles/{battleId} ───────────────────────────────────────────
        public async Task<BattleState?> GetBattleStateAsync(string battleId)
        {
            try
            {
                return await _http.GetFromJsonAsync<BattleState>($"/battles/{battleId}");
            }
            catch { return null; }
        }

        // ── POST /battles/{battleId}/choose_pokemon ───────────────────────────
        // app.py espera: { "pokemon_index": int }
        // El servidor saca el usuario del JWT — no se manda player_id.
        public async Task<bool> ChoosePokemonAsync(string battleId, int pokemonIndex)
        {
            try
            {
                var body = new { pokemon_index = pokemonIndex };
                var r = await _http.PostAsJsonAsync($"/battles/{battleId}/choose_pokemon", body);
                return r.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ── POST /battles/{battleId}/action  (movimiento) ─────────────────────
        // app.py espera: { "action": { "type": "move", "move_name": string } }
        public async Task<TurnResult?> UseMoveAsync(string battleId, string moveName)
        {
            try
            {
                var body = new { action = new { type = "move", move_name = moveName } };
                var r = await _http.PostAsJsonAsync($"/battles/{battleId}/action", body);
                if (!r.IsSuccessStatusCode) return null;
                return await r.Content.ReadFromJsonAsync<TurnResult>();
            }
            catch { return null; }
        }

        // ── POST /battles/{battleId}/action  (cambio) ─────────────────────────
        // app.py espera: { "action": { "type": "switch", "pokemon_index": int } }
        public async Task<bool> SwitchPokemonAsync(string battleId, int pokemonIndex)
        {
            try
            {
                var body = new { action = new { type = "switch", pokemon_index = pokemonIndex } };
                var r = await _http.PostAsJsonAsync($"/battles/{battleId}/action", body);
                return r.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}
