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

        /// <summary>Tipo de mensaje (ej. "battle_request").</summary>
        [BsonElement("type")]
        [JsonPropertyName("type")]
        public string Tipo { get; set; }

        /// <summary>True si el destinatario ya respondió (acepto/rechazo).</summary>
        [BsonElement("responded")]
        [JsonPropertyName("responded")]
        public bool Respondido { get; set; }

        /// <summary>Lección de compatibilidad con código anterior que usaba "leido".</summary>
        [BsonElement("leido")]
        [JsonPropertyName("leido")]
        public bool Leido { get; set; }

        /// <summary>
        /// Si el mensaje es un desafío de batalla, contiene el battleId.
        /// Null si es un mensaje normal.
        /// </summary>
        [BsonElement("tipoBatallaId")]
        [JsonPropertyName("tipoBatallaId")]
        public string TipoBatallaId { get; set; }
    }
}
