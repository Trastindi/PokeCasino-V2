using System.Collections.Generic;
using System.Threading.Tasks;
using PK_Proyect.Models;

namespace PK_Proyect.Repositories
{
    public interface ITradeRepository
    {
        // ── Solicitudes ──────────────────────────────────────────────
        Task<TradeMensajeModel?> SendTradeRequestAsync(string rivalId);
        Task<string?> RespondTradeRequestAsync(string msgId, bool accepted);

        // ── Estado del intercambio ───────────────────────────────────
        Task<TradeModel?> GetTradeAsync(string tradeId);
        Task<List<TradeModel>> GetMyTradesAsync();

        // ── Acciones dentro del intercambio ─────────────────────────
        Task<bool> OfferPokemonAsync(string tradeId, string pokemonObjectId);
        Task<string> ConfirmTradeAsync(string tradeId);
        Task<bool> CancelTradeAsync(string tradeId);
    }
}
