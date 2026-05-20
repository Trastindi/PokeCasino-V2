using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace PK_Proyect.Models
{
    public class Tipo
    {
        [BsonId]
        public string Nombre { get; set; }

        public List<string> Ventajas { get; set; }
        public List<string> Debilidades { get; set; }
    }
}
