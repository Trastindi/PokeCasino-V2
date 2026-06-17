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

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("remitente")]
        public string Remitente { get; set; }

        [BsonElement("contenido")]
        public string Contenido { get; set; }

        [BsonElement("fecha")]
        public DateTime Fecha { get; set; }

        [BsonElement("leido")]
        public bool Leido { get; set; }
    }
}
