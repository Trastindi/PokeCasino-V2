using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace PK_Proyect.Models
{
    [BsonIgnoreExtraElements]
    public class Pokemon
    {
        [BsonId]
        public ObjectId MongoId { get; set; }

        [BsonElement("numero_pokedex")]
        public int numero_pokedex { get; set; }

        [BsonElement("pokemon_id")]
        public string PokemonId { get; set; }

        [BsonElement("nombre")]
        public string Nombre { get; set; }

        /// <summary>Lista de tipos del Pokemon, p.ej. ["Fuego"] o ["Agua", "Volador"]</summary>
        [BsonElement("tipos")]
        public List<string> Tipos { get; set; } = new List<string>();

        // Propiedad de conveniencia: devuelve el primer tipo o cadena vacía
        [BsonIgnore]
        public string TipoPrincipal => Tipos?.Count > 0 ? Tipos[0] : string.Empty;

        // Propiedad de conveniencia: devuelve el segundo tipo o null
        [BsonIgnore]
        public string TipoSecundario => Tipos?.Count > 1 ? Tipos[1] : null;

        [BsonElement("region")]
        public string Region { get; set; }

        [BsonElement("descripcion")]
        public string Descripcion { get; set; }

        [BsonElement("estadisticas_base")]
        public EstadisticasBase EstadisticasBase { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class EstadisticasBase
    {
        [BsonElement("ps")]              public int Ps { get; set; }
        [BsonElement("ataque")]         public int Ataque { get; set; }
        [BsonElement("defensa")]        public int Defensa { get; set; }
        [BsonElement("ataque_especial")] public int AtaqueEspecial { get; set; }
        [BsonElement("defensa_especial")] public int DefensaEspecial { get; set; }
        [BsonElement("velocidad")]      public int Velocidad { get; set; }
    }
}
