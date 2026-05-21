using PK_Proyect.Models;

public interface IPokedexRepository
{
    List<Pokemon> ObtenerTodos();
    Pokemon ObtenerPorId(int id);
}
