using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PK_Proyect.Models
{
    [BsonIgnoreExtraElements]
    public class PokemonUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string numero_pokedex { get; set; }

        public string UserId { get; set; }
        public string Username { get; set; }

        public int PokemonId { get; set; }
        public string Nombre { get; set; }
        public string TipoPrincipal { get; set; }
        public string TipoSecundario{ get; set; }

        public int Nivel { get; set; } = 1;
        public int Cantidad { get; set; } = 1;
        public DateTime FechaObtenido { get; set; }

        public string AbilityId { get; set; } = null;
        public string ItemId { get; set; } = null;
        public List<string> MoveSet { get; set; } = new();
        public int CurrentHp { get; set; } = 0;
        public string Status { get; set; } = null;

        public int HiddenPowerSeed { get; set; } = 0;
        public int HiddenPowerPower { get; set; } = 0;
    }
}
