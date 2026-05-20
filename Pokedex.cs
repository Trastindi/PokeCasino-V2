using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PK_Proyect.Models
{
    [BsonIgnoreExtraElements]
    public class Pokemon
    {
        [BsonId] public ObjectId MongoId { get; set; }


        [BsonElement("Id")]
        public int Id { get; set; }

        [BsonElement("Nombre")]
        public string Nombre { get; set; }

        [BsonElement("Tipo1")]
        public string Tipo1 { get; set; }

        [BsonElement("Tipo2")]
        public string Tipo2 { get; set; }

        [BsonElement("Region")]
        public string Region { get; set; }

        [BsonElement("Descripcion")]
        public string Descripcion { get; set; }


    }
}
