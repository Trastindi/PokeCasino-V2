using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PK_Proyect.Services
{
    /// <summary>
    /// Implementación HTTP de IBattleService alineada con los endpoints de app.py.
    /// </summary>
    public class BattleService : IBattleService
    {
        private readonly HttpClient _http;

        public BattleService(HttpClient http) { _http = http; }

        /// <summary>Usa el HttpClient compartido con token JWT ya configurado.</summary>
        /// FIX CS0117: ApiClient expone el cliente como propiedad estática "Client".
        public BattleService() : this(ApiClient.Client) { }

        // ── POST /battles ────────────────────────────────────────────────
        public async Task<string?> SendChallengeAsync(string challengerId, string challengedId)
        {
            try
            {
                var body = new { challenger_id = challengerId, challenged_id = challengedId };
                var r = await _http.PostAsJsonAsync("/battles", body);
                if (!r.IsSuccessStatusCode) return null;
                var data = await r.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                return data.TryGetProperty("battle_id", out var id) ? id.GetString() : null;
            }
            catch { return null; }
        }

        // ── POST /battles/{id}/join ────────────────────────────────────────
        public async Task<bool> RequestJoinAsync(string playerId, string battleId)
        {
            try
            {
                var body = new { player_id = playerId };
                var r = await _http.PostAsJsonAsync($"/battles/{battleId}/join", body);
                return r.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ── WaitForAcceptance: polling hasta status != "waiting" ───────────────────
        public async Task<bool> WaitForAcceptanceAsync(string battleId)
        {
            var deadline = DateTime.UtcNow.AddSeconds(60);
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    var state = await GetBattleStateAsync(battleId);
                    if (state == null)                    { await Task.Delay(2000); continue; }
                    if (state.Status == "cancelled")      return false;
                    if (state.Status != "waiting")        return true;
                }
                catch { /* red transitoria */ }
                await Task.Delay(2000);
            }
            return false; // timeout
        }

        // ── GET /battles/{battleId} ───────────────────────────────────────────
        public async Task<BattleState?> GetBattleStateAsync(string battleId)
        {
            try { return await _http.GetFromJsonAsync<BattleState>($"/battles/{battleId}"); }
            catch { return null; }
        }

        // ── POST /battles/{battleId}/choose_pokemon ───────────────────────────
        public async Task<bool> ChoosePokemonAsync(string battleId, int pokemonIndex)
        {
            try
            {
                var r = await _http.PostAsJsonAsync($"/battles/{battleId}/choose_pokemon",
                                                    new { pokemon_index = pokemonIndex });
                return r.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ── POST /battles/{battleId}/action  (movimiento) ─────────────────────
        public async Task<TurnResult?> UseMoveAsync(string battleId, string moveName)
        {
            try
            {
                var r = await _http.PostAsJsonAsync($"/battles/{battleId}/action",
                    new { action = new { type = "move", move_name = moveName } });
                if (!r.IsSuccessStatusCode) return null;
                return await r.Content.ReadFromJsonAsync<TurnResult>();
            }
            catch { return null; }
        }

        // ── POST /battles/{battleId}/action  (cambio) ─────────────────────────
        public async Task<bool> SwitchPokemonAsync(string battleId, int pokemonIndex)
        {
            try
            {
                var r = await _http.PostAsJsonAsync($"/battles/{battleId}/action",
                    new { action = new { type = "switch", pokemon_index = pokemonIndex } });
                return r.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}
