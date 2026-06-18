using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PK_Proyect.Models;

namespace PK_Proyect.Repositories
{
    public class TradeRepository : ITradeRepository
    {
        private readonly ApiClient _api;
        private static readonly JsonSerializerOptions _opts = new(JsonSerializerDefaults.Web);

        public TradeRepository(ApiClient api) => _api = api;

        // ── 1. Enviar solicitud ──────────────────────────────────────
        public async Task<TradeMensajeModel?> SendTradeRequestAsync(string rivalId)
        {
            var resp = await _api.PostAsync($"/trade_requests/{rivalId}", null);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TradeMensajeModel>(json, _opts);
        }

        // ── 2. Responder solicitud ───────────────────────────────────
        /// <returns>trade_id si se aceptó, null si se rechazó</returns>
        public async Task<string?> RespondTradeRequestAsync(string msgId, bool accepted)
        {
            var body = JsonSerializer.Serialize(new { accepted });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var resp = await _api.PostAsync($"/trade_requests/{msgId}/respond", content);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            var doc  = JsonSerializer.Deserialize<JsonElement>(json, _opts);
            return doc.TryGetProperty("trade_id", out var t) ? t.GetString() : null;
        }

        // ── 3. Consultar estado ──────────────────────────────────────
        public async Task<TradeModel?> GetTradeAsync(string tradeId)
        {
            var resp = await _api.GetAsync($"/trades/{tradeId}");
            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TradeModel>(json, _opts);
        }

        // ── 4. Listar intercambios del usuario ───────────────────────
        public async Task<List<TradeModel>> GetMyTradesAsync()
        {
            var resp = await _api.GetAsync("/trades");
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TradeModel>>(json, _opts) ?? new();
        }

        // ── 5. Ofrecer pokémon ───────────────────────────────────────
        public async Task<bool> OfferPokemonAsync(string tradeId, string pokemonObjectId)
        {
            var body    = JsonSerializer.Serialize(new { pokemon_id = pokemonObjectId });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var resp    = await _api.PostAsync($"/trades/{tradeId}/offer", content);
            return resp.IsSuccessStatusCode;
        }

        // ── 6. Confirmar ─────────────────────────────────────────────
        /// <returns>"done" si el intercambio se completó, mensaje de espera si no</returns>
        public async Task<string> ConfirmTradeAsync(string tradeId)
        {
            var resp = await _api.PostAsync($"/trades/{tradeId}/confirm", null);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            var doc  = JsonSerializer.Deserialize<JsonElement>(json, _opts);
            return doc.TryGetProperty("status", out var s) ? s.GetString() ?? "" : "waiting";
        }

        // ── 7. Cancelar ──────────────────────────────────────────────
        public async Task<bool> CancelTradeAsync(string tradeId)
        {
            var resp = await _api.PostAsync($"/trades/{tradeId}/cancel", null);
            return resp.IsSuccessStatusCode;
        }
    }
}
