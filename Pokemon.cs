using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PK_Proyect.Models
{
    public class Pokedex
    {
        [BsonId]
        public int Id { get; set; } 

        public string Nombre { get; set; }
        public string Tipo1 { get; set; }
        public string Tipo2 { get; set; } 
        public string Descripcion { get; set; }
        public string Region { get; set; }
    }
}
