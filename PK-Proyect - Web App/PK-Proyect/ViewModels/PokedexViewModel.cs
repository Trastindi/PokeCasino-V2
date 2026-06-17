using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PK_Proyect.ViewModels
{
    public class PokedexViewModel
    {
        private readonly PokemonUserRepository _repo;

        public ObservableCollection<PokedexEntry> Pokedex { get; set; }

        public PokedexViewModel(string userId)
        {
            _repo = new PokemonUserRepository();
            Pokedex = new ObservableCollection<PokedexEntry>();

            _ = CargarPokedexAsync(userId);
        }

        private async Task CargarPokedexAsync(string userId)
        {
            // 1. Obtener TODOS los Pokémon del usuario (incluye repetidos)
            var listaUsuario = await Task.Run(() =>
                _repo.GetPokemonsByUser(userId).ToList()
            );

            // 2. Obtener IDs únicos que el usuario posee
            var idsUsuario = listaUsuario
                .Select(p => p.PokemonId)
                .Distinct()
                .ToHashSet();

            // 3. Obtener el ID máximo para saber cuántos Pokémon existen
            int maxId = listaUsuario.Any()
                ? listaUsuario.Max(p => p.PokemonId)
                : 151; // fallback si no tiene ninguno

            // 4. Construir la Pokédex completa
            var listaFinal = Enumerable.Range(1, maxId)
                .OrderBy(id => id) // Ascendente
                .Select(id =>
                {
                    var poke = listaUsuario.FirstOrDefault(p => p.PokemonId == id);

                    return new PokedexEntry
                    {
                        PokemonId = id,
                        Nombre = idsUsuario.Contains(id) ? poke.Nombre : "????"
                    };
                })
                .ToList();

            // 5. Cargar en ObservableCollection
            foreach (var entry in listaFinal)
                Pokedex.Add(entry);
        }
    }

    public class PokedexEntry
    {
        public int PokemonId { get; set; }
        public string Nombre { get; set; }
    }
}
