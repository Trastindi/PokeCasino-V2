using PK_Proyect.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PK_Proyect.Services
{
    public class BattleService : IBattleService
    {
        // ── DTOs internos ────────────────────────────────────────────────────────
        private class BattleRequestResponse { public string id { get; set; } public string battle_id { get; set; } }
        private class BoolResult            { public bool   success { get; set; } }

        // ── Matchmaking ──────────────────────────────────────────────────────────

        public async Task<string> SendChallengeAsync(string currentUserId, string targetUserId)
        {
            try
            {
                var r = await ApiClient.PostAsync<BattleRequestResponse>(
                    $"/battle_requests/{targetUserId}", (object?)null);
                return r?.battle_id ?? r?.id;
            }
            catch (Exception ex) { Show("Error al enviar desafío", ex); return null; }
        }

        public async Task<bool> RequestJoinAsync(string currentUserId, string battleId)
        {
            try
            {
                var r = await ApiClient.GetAsync<object>($"/battles/{battleId}");
                return r != null;
            }
            catch (Exception ex) { Show("Error al unirse", ex); return false; }
        }

        public async Task<bool> WaitForAcceptanceAsync(string battleIdOrUserId)
        {
            try
            {
                for (int i = 0; i < 30; i++)
                {
                    var r = await ApiClient.GetAsync<StatusDto>($"/battles/{battleIdOrUserId}/status");
                    if (r?.status == "active") return true;
                    await Task.Delay(1000);
                }
                return false;
            }
            catch (Exception ex) { Show("Error al esperar aceptación", ex); return false; }
        }

        public async Task<bool> CancelBattleAsync(string battleId)
        {
            try   { await ApiClient.DeleteAsync($"/battles/{battleId}"); return true; }
            catch (Exception ex) { Show("Error al cancelar", ex); return false; }
        }

        // ── Estado ───────────────────────────────────────────────────────────────

        /// <summary>GET /battles/{battleId}  — devuelve el snapshot completo.</summary>
        public async Task<BattleState> GetBattleStateAsync(string battleId)
        {
            try   { return await ApiClient.GetAsync<BattleState>($"/battles/{battleId}"); }
            catch (Exception ex) { Show("Error al obtener estado", ex); return null; }
        }

        // ── Acciones de turno ────────────────────────────────────────────────────

        /// <summary>POST /battles/{battleId}/choose_pokemon  { "player_id": ..., "pokemon_id": ... }</summary>
        public async Task<bool> ChoosePokemonAsync(string battleId, string playerId, string pokemonId)
        {
            try
            {
                var r = await ApiClient.PostAsync<BoolResult>(
                    $"/battles/{battleId}/choose_pokemon",
                    new { player_id = playerId, pokemon_id = pokemonId });
                return r?.success ?? false;
            }
            catch (Exception ex) { Show("Error al elegir Pokémon", ex); return false; }
        }

        /// <summary>POST /battles/{battleId}/use_move  { "player_id": ..., "move_id": ... }</summary>
        public async Task<TurnResult> UseMoveAsync(string battleId, string playerId, string moveId)
        {
            try
            {
                return await ApiClient.PostAsync<TurnResult>(
                    $"/battles/{battleId}/use_move",
                    new { player_id = playerId, move_id = moveId });
            }
            catch (Exception ex) { Show("Error al usar movimiento", ex); return null; }
        }

        /// <summary>POST /battles/{battleId}/switch_pokemon  { "player_id": ..., "pokemon_id": ... }</summary>
        public async Task<bool> SwitchPokemonAsync(string battleId, string playerId, string pokemonId)
        {
            try
            {
                var r = await ApiClient.PostAsync<BoolResult>(
                    $"/battles/{battleId}/switch_pokemon",
                    new { player_id = playerId, pokemon_id = pokemonId });
                return r?.success ?? false;
            }
            catch (Exception ex) { Show("Error al cambiar Pokémon", ex); return false; }
        }

        // ── Compat. legacy ───────────────────────────────────────────────────────
        public async Task<BattleData> GetBattleDataAsync(string battleId)
        {
            try   { return await ApiClient.GetAsync<BattleData>($"/battles/{battleId}"); }
            catch (Exception ex) { Show("Error al obtener datos de batalla", ex); return null; }
        }

        // ── Helpers ──────────────────────────────────────────────────────────────
        private static void Show(string msg, Exception ex) =>
            System.Windows.MessageBox.Show($"{msg}: {ex.Message}");

        private class StatusDto { public string status { get; set; } }
    }
}
