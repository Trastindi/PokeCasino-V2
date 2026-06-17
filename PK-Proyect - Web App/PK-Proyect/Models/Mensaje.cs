using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace PK_Proyect.Models
{
    public class Mensaje
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>ID del destinatario (campo "to" en la BD).</summary>
        [BsonElement("to")]
        public string To { get; set; }

        /// <summary>Nombre o ID del remitente.</summary>
        [BsonElement("remitente")]
        public string Remitente { get; set; }

        /// <summary>ID del usuario remitente (para abrir batalla).</summary>
        [BsonElement("remitenteId")]
        public string RemitenteId { get; set; }

        /// <summary>Texto del mensaje.</summary>
        [BsonElement("contenido")]
        public string Contenido { get; set; }

        /// <summary>Fecha de envío.</summary>
        [BsonElement("Fecha")]
        public DateTime Fecha { get; set; }

        /// <summary>Indica si el destinatario ya lo leyó.</summary>
        [BsonElement("leido")]
        public bool Leido { get; set; }

        /// <summary>
        /// Si el mensaje es un desafío de batalla, contiene el battleId.
        /// Null si es un mensaje normal.
        /// </summary>
        [BsonElement("tipoBatallaId")]
        public string TipoBatallaId { get; set; }
    }
}
