using PK_Proyect.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PK_Proyect.Models
{
    [BsonIgnoreExtraElements]
    public class PokemonZona
    {



        [BsonElement("PokemonId")]
        [BsonRepresentation(BsonType.Int32)]
        public int PokemonId { get; set; }

        [BsonElement("prob")]
        public int prob { get; set; }
    }
}