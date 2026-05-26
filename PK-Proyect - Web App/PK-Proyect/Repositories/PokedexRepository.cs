using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    /// <summary>
    /// Acceso a la Pokédex a través del servidor Flask.
    /// </summary>
    public class PokedexRepository
    {
        public Pokemon ObtenerPorId(int id)
            => ApiClient.Get<Pokemon>($"/pokedex/{id}");

        public Pokemon ObtenerPorNombre(string nombre)
        {
            // Búsqueda local tras obtener todos (no hay endpoint de búsqueda por nombre aún)
            var todos = ObtenerTodos();
            return todos.Find(p =>
                string.Equals(p.Nombre, nombre, System.StringComparison.OrdinalIgnoreCase));
        }

        public List<Pokemon> ObtenerTodos()
            => ApiClient.Get<List<Pokemon>>("/pokedex");
    }
}
