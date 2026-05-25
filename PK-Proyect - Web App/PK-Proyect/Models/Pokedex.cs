using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PK_Proyect.Models
{
    [BsonIgnoreExtraElements]
    public class Pokemon
    {
        [BsonId] public ObjectId MongoId { get; set; }


        [BsonElement("numero_pokedex")]
        public int numero_pokedex { get; set; }

        [BsonElement("Nombre")]
        public string Nombre { get; set; }

        [BsonElement("TipoPrincipal")]
        public string TipoPrincipal { get; set; }

        [BsonElement("TipoSecundario")]
        public string TipoSecundario { get; set; }

        [BsonElement("Region")]
        public string Region { get; set; }

        [BsonElement("Descripcion")]
        public string Descripcion { get; set; }

        [BsonElement("Estadisticas_base")]
        public List<int> EstadisticasBase { get; set; } = new List<int> { 0, 0, 0, 0, 0, 0 };
    }
}
