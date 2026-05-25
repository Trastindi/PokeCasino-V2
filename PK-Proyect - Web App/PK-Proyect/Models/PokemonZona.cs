using PK_Proyect.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PK_Proyect.Models
{
    [BsonIgnoreExtraElements]
    public class PokemonZona
    {



        [BsonElement("numero_pokedex")]
        [BsonRepresentation(BsonType.Int32)]
        public int numero_pokedex { get; set; }

        [BsonElement("prob")]
        public int prob { get; set; }
    }
}