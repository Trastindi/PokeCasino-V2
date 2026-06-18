using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace PK_Proyect.Services
{
    public class BattleService : IBattleService
    {
        private readonly HttpClient _http;

        public BattleService(HttpClient http) { _http = http; }
        public BattleService() : this(ApiClient.Client) { }

        // ── POST /battle_requests/<rival_id> ─────────────────────────────
        public async Task<string?> SendChallengeAsync(string challengerId, string challengedId)
        {
            try
            {
                var r = await _http.PostAsJsonAsync($"/battle_requests/{challengedId}", new { });
                var rawJson = await r.Content.ReadAsStringAsync();

                // ► DEBUG temporal: muestra el JSON crudo del servidor
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show(
                        $"Status: {(int)r.StatusCode}\n\nJSON:\n{rawJson}",
                        "[DEBUG] Respuesta /battle_requests",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information));

                if (!r.IsSuccessStatusCode) return null;

                var data = System.Text.Json.JsonDocument.Parse(rawJson).RootElement;
                return data.TryGetProperty("battle_id", out var id) ? id.GetString() : null;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show($"Excepción en SendChallengeAsync:\n{ex}",
                        "[DEBUG] Error", MessageBoxButton.OK, MessageBoxImage.Error));
                return null;
            }
        }

        // ── GET /battles/{battleId} ──────────────────────────────────────
        public async Task<BattleState?> GetBattleStateAsync(string battleId)
        {
            try { return await _http.GetFromJsonAsync<BattleState>($"/battles/{battleId}"); }
            catch { return null; }
        }

        // ── WaitForAcceptance: polling hasta status != pending_acceptance ────
        public async Task<bool> WaitForAcceptanceAsync(string battleId)
        {
            var deadline = DateTime.UtcNow.AddSeconds(120);
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    var state = await GetBattleStateAsync(battleId);
                    if (state == null)                        { await Task.Delay(2000); continue; }
                    if (state.Status == "cancelled")          return false;
                    if (state.Status != "pending_acceptance") return true;
                }
                catch { }
                await Task.Delay(2000);
            }
            return false;
        }

        public async Task<bool> RequestJoinAsync(string playerId, string battleId)
            => await Task.FromResult(true);

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
