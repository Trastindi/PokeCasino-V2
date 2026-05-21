using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PK_Proyect.Models
{
    public class PokemonUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }          // ID del usuario dueño del Pokémon

        public string Username {  get; set; }       //Nombre Usuario del pokemon

        public int PokemonId { get; set; }          // ID del Pokémon (coincide con Pokedex)
        public string Nombre { get; set; }          // Nombre del Pokémon
        public string TipoPrincipal { get; set; }   // Tipo principal (Fuego, Agua, etc.)
        public string TipoSecundario {  get; set; } //Tipo Secundario

        public int Nivel { get; set; } = 1;         // Nivel actual del Pokémon
        public int Cantidad { get; set; } = 1;      // Cuántas veces lo ha obtenido

        public DateTime FechaObtenido { get; set; }   //Fecha Obtenido


    }
}
