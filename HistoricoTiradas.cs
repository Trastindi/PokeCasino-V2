using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class HistoricoTirada
{
    public string UserId { get; set; }
    public int PokemonId { get; set; }
    public string NombrePokemon { get; set; }
    public string Zona { get; set; }
    public string TipoTirada { get; set; }
    public DateTime Fecha { get; set; }
}
