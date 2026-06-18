using PK_Proyect.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PK_Proyect.Services
{
    /// <summary>
    /// Implementación HTTP de IBattleService que habla con el servidor Python (FastAPI / Flask).
    /// La base URL se toma de ApiClient.BaseUrl o se puede inyectar en el constructor.
    /// </summary>
    public class BattleService : IBattleService
    {
        private readonly HttpClient _http;

        public BattleService(HttpClient http)
        {
            _http = http;
        }

        // Usa el HttpClient compartido de la aplicación (misma baseAddress que ApiClient)
        public BattleService() : this(ApiClient.HttpClient) { }

        // ── GET /battles/{battleId}/state ─────────────────────────────────────────
        public async Task<BattleState?> GetBattleStateAsync(string battleId)
        {
            try
            {
                return await _http.GetFromJsonAsync<BattleState>($"/battles/{battleId}/state");
            }
            catch { return null; }
        }

        // ── POST /battles/{battleId}/choose_pokemon ───────────────────────────────
        public async Task<bool> ChoosePokemonAsync(string battleId, string playerId, string pokemonId)
        {
            try
            {
                var body = new { player_id = playerId, pokemon_id = pokemonId };
                var r = await _http.PostAsJsonAsync($"/battles/{battleId}/choose_pokemon", body);
                return r.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ── POST /battles/{battleId}/use_move ────────────────────────────────────
        public async Task<TurnResult?> UseMoveAsync(string battleId, string playerId, string moveName)
        {
            try
            {
                var body = new { player_id = playerId, move_name = moveName };
                var r = await _http.PostAsJsonAsync($"/battles/{battleId}/use_move", body);
                if (!r.IsSuccessStatusCode) return null;
                return await r.Content.ReadFromJsonAsync<TurnResult>();
            }
            catch { return null; }
        }

        // ── POST /battles/{battleId}/switch_pokemon ───────────────────────────────
        public async Task<bool> SwitchPokemonAsync(string battleId, string playerId, string pokemonId)
        {
            try
            {
                var body = new { player_id = playerId, pokemon_id = pokemonId };
                var r = await _http.PostAsJsonAsync($"/battles/{battleId}/switch_pokemon", body);
                return r.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}
