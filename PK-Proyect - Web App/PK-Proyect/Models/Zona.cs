using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using PK_Proyect.Models;

namespace PK_Proyect.Models
{
    
        public class Zona
        {
            [BsonId]
            public ObjectId Id { get; set; }

            [BsonElement("nombre")]
            public string Nombre { get; set; }

            [BsonElement("region")]
            public string Region { get; set; }

            [BsonElement("tipo")]
            public string Tipo { get; set; }

            [BsonElement("descripcion")]
            public string Descripcion { get; set; }

            // Lista de Pokémon con su ratio
            [BsonElement("pokemon")]
            public List<PokemonZona> Pokemon { get; set; }
        }
    
}