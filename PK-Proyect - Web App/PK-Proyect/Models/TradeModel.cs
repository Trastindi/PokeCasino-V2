using System;
using System.Collections.Generic;

namespace PK_Proyect.Models
{
    // Estado posible del intercambio
    public enum TradeStatus
    {
        Pending,
        Offered,
        Confirmed,
        Done,
        Cancelled
    }

    public class TradeModel
    {
        public string Id               { get; set; } = string.Empty;
        public string Player1Id        { get; set; } = string.Empty;
        public string Player2Id        { get; set; } = string.Empty;
        public string Player1Name      { get; set; } = string.Empty;
        public string Player2Name      { get; set; } = string.Empty;

        // "pending" | "offered" | "confirmed" | "done" | "cancelled"
        public string Status           { get; set; } = "pending";

        // ObjectId de PokemonUser que cada jugador ofrece
        public string? Player1Pokemon  { get; set; }
        public string? Player2Pokemon  { get; set; }

        // Detalle enriquecido del pokémon (rellenado por el servidor en GET /trades/{id})
        public PokemonUser? Player1PokemonDetail { get; set; }
        public PokemonUser? Player2PokemonDetail { get; set; }

        public bool Player1Confirmed   { get; set; }
        public bool Player2Confirmed   { get; set; }

        public string CreatedAt        { get; set; } = string.Empty;
        public string? CompletedAt     { get; set; }
    }

    // Mensaje de solicitud de intercambio recibido en el buzón
    public class TradeMensajeModel
    {
        public string Id         { get; set; } = string.Empty;
        public string From       { get; set; } = string.Empty;
        public string FromId     { get; set; } = string.Empty;
        public string To         { get; set; } = string.Empty;
        public string Title      { get; set; } = string.Empty;
        public string Text       { get; set; } = string.Empty;
        public string Fecha      { get; set; } = string.Empty;
        public string Type       { get; set; } = string.Empty;
        public string? TradeId   { get; set; }
        public bool   Responded  { get; set; }
    }
}
