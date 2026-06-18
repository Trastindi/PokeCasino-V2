using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;

namespace PK_Proyect.Models
{
    public class Mensaje
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        /// <summary>Nombre del remitente (campo "from" en la BD).</summary>
        [BsonElement("from")]
        [JsonPropertyName("from")]
        public string Remitente { get; set; }

        /// <summary>ID del usuario remitente.</summary>
        [BsonElement("from_id")]
        [JsonPropertyName("from_id")]
        public string RemitenteId { get; set; }

        /// <summary>ID del destinatario.</summary>
        [BsonElement("to")]
        [JsonPropertyName("to")]
        public string To { get; set; }

        /// <summary>Título del mensaje.</summary>
        [BsonElement("title")]
        [JsonPropertyName("title")]
        public string Titulo { get; set; }

        /// <summary>Texto / cuerpo del mensaje.</summary>
        [BsonElement("text")]
        [JsonPropertyName("text")]
        public string Contenido { get; set; }

        /// <summary>Fecha de envío.</summary>
        [BsonElement("Fecha")]
        [JsonPropertyName("Fecha")]
        public DateTime Fecha { get; set; }

        /// <summary>Tipo de mensaje (ej. "battle_request", "trade_request").</summary>
        [BsonElement("type")]
        [JsonPropertyName("type")]
        public string Tipo { get; set; }

        /// <summary>True si el destinatario ya respondió (acepto/rechazo).</summary>
        [BsonElement("responded")]
        [JsonPropertyName("responded")]
        public bool Respondido { get; set; }

        /// <summary>Compatibilidad con campo "leido" anterior.</summary>
        [BsonElement("leido")]
        [JsonPropertyName("leido")]
        public bool Leido { get; set; }

        /// <summary>
        /// ID de la batalla asociada al mensaje (campo "battle_id" en el servidor).
        /// Presente en mensajes de tipo "battle_request" y "battle_response".
        /// </summary>
        [BsonElement("battle_id")]
        [JsonPropertyName("battle_id")]
        public string TipoBatallaId { get; set; }

        /// <summary>
        /// ID del intercambio asociado (campo "trade_id" en el servidor).
        /// Presente en mensajes de tipo "trade_request" y "trade_response".
        /// </summary>
        [BsonElement("trade_id")]
        [JsonPropertyName("trade_id")]
        public string TradeId { get; set; }
    }
}
