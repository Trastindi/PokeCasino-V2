using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System;
using System.Net.Http;
using System.Net.Http.Json;
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
        public BattleService() : this(ApiClient.Client) { }

        // ── POST /battle_requests/<rival_id> ─────────────────────────────
        // El servidor crea la batalla y devuelve { battle_id: "..." }
        // El challengerId ya va implícito en el JWT; solo se pasa el rival en la URL.
        public async Task<string?> SendChallengeAsync(string challengerId, string challengedId)
        {
            try
            {
                // challengerId está en el token JWT — no hace falta en el body.
                var r = await _http.PostAsJsonAsync($"/battle_requests/{challengedId}", new { });
                if (!r.IsSuccessStatusCode) return null;
                var data = await r.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                return data.TryGetProperty("battle_id", out var id) ? id.GetString() : null;
            }
            catch { return null; }
        }

        // ── GET /battles/{battleId} ──────────────────────────────────────
        public async Task<BattleState?> GetBattleStateAsync(string battleId)
        {
            try { return await _http.GetFromJsonAsync<BattleState>($"/battles/{battleId}"); }
            catch { return null; }
        }

        // ── WaitForAcceptance: polling hasta status != pending_acceptance ─
        // El servidor usa: pending_acceptance → pending → ready → in_progress
        public async Task<bool> WaitForAcceptanceAsync(string battleId)
        {
            var deadline = DateTime.UtcNow.AddSeconds(120);
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    var state = await GetBattleStateAsync(battleId);
                    if (state == null)                             { await Task.Delay(2000); continue; }
                    if (state.Status == "cancelled")               return false;
                    if (state.Status != "pending_acceptance")      return true;  // aceptado
                }
                catch { /* red transitoria */ }
                await Task.Delay(2000);
            }
            return false; // timeout
        }

        // ── POST /battles/{battleId}/teams ───────────────────────────────
        public async Task<bool> RequestJoinAsync(string playerId, string battleId)
        {
            // Método legacy mantenido por compatibilidad con la interfaz.
            // El flujo real usa SubmitTeamAsync desde SearchBattleViewModel.
            return await Task.FromResult(true);
        }

        // ── POST /battles/{battleId}/choose_pokemon ──────────────────────
        public async Task<bool> ChoosePokemonAsync(string battleId, int pokemonIndex)
        {
            try
            {
                var r = await _http.PostAsJsonAsync(
                    $"/battles/{battleId}/choose_pokemon",
                    new { pokemon_index = pokemonIndex });
                return r.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ── POST /battles/{battleId}/action  (movimiento) ────────────────
        public async Task<TurnResult?> UseMoveAsync(string battleId, string moveName)
        {
            try
            {
                var r = await _http.PostAsJsonAsync(
                    $"/battles/{battleId}/action",
                    new { action = new { type = "move", move_name = moveName } });
                if (!r.IsSuccessStatusCode) return null;
                return await r.Content.ReadFromJsonAsync<TurnResult>();
            }
            catch { return null; }
        }

        // ── POST /battles/{battleId}/action  (cambio) ────────────────────
        public async Task<bool> SwitchPokemonAsync(string battleId, int pokemonIndex)
        {
            try
            {
                var r = await _http.PostAsJsonAsync(
                    $"/battles/{battleId}/action",
                    new { action = new { type = "switch", pokemon_index = pokemonIndex } });
                return r.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}
