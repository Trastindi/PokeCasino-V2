using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PK_Proyect.Models
{
    public class MedallasUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }      // Usuario que ganó la medalla
        public string Tipo { get; set; }        // Tipo de la medalla (Fuego, Agua, Planta...)
        public DateTime Fecha { get; set; }     // Cuándo la obtuvo
    }
}
