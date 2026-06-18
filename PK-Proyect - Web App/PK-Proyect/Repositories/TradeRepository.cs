using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using PK_Proyect.Models;

namespace PK_Proyect.Repositories
{
    public class TradeRepository : ITradeRepository
    {
        // Sin constructor con parámetro ApiClient: ApiClient es una clase estática
        // y sus métodos se invocan directamente (ApiClient.PostAsync<T>(...)).

        // ── 1. Enviar solicitud ────────────────────────────────────────
        public async Task<TradeMensajeModel?> SendTradeRequestAsync(string rivalId)
            => await ApiClient.PostAsync<TradeMensajeModel>($"/trade_requests/{rivalId}", (object?)null);

        // ── 2. Responder solicitud ───────────────────────────────────
        /// <returns>trade_id si se aceptó, null si se rechazó</returns>
        public async Task<string?> RespondTradeRequestAsync(string msgId, bool accepted)
        {
            var doc = await ApiClient.PostAsync<JsonElement>(
                $"/trade_requests/{msgId}/respond",
                new { accepted });
            return doc.TryGetProperty("trade_id", out var t) ? t.GetString() : null;
        }

        // ── 3. Consultar estado ───────────────────────────────────────
        public async Task<TradeModel?> GetTradeAsync(string tradeId)
        {
            try
            {
                return await ApiClient.GetAsync<TradeModel>($"/trades/{tradeId}");
            }
            catch
            {
                return null;
            }
        }

        // ── 4. Listar intercambios del usuario ───────────────────────────
        public async Task<List<TradeModel>> GetMyTradesAsync()
            => await ApiClient.GetAsync<List<TradeModel>>("/trades");

        // ── 5. Ofrecer pokémon ───────────────────────────────────────────
        public async Task<bool> OfferPokemonAsync(string tradeId, string pokemonObjectId)
        {
            try
            {
                await ApiClient.PostAsync<JsonElement>(
                    $"/trades/{tradeId}/offer",
                    new { pokemon_id = pokemonObjectId });
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ── 6. Confirmar ───────────────────────────────────────────────
        /// <returns>"done" si el intercambio se completó, mensaje de espera si no</returns>
        public async Task<string> ConfirmTradeAsync(string tradeId)
        {
            var doc = await ApiClient.PostAsync<JsonElement>(
                $"/trades/{tradeId}/confirm", (object?)null);
            return doc.TryGetProperty("status", out var s) ? s.GetString() ?? "" : "waiting";
        }

        // ── 7. Cancelar ────────────────────────────────────────────────
        public async Task<bool> CancelTradeAsync(string tradeId)
        {
            try
            {
                await ApiClient.PostAsync<JsonElement>($"/trades/{tradeId}/cancel", (object?)null);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
