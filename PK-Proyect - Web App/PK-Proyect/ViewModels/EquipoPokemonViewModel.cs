using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PK_Proyect.ViewModels
{
    public class EquipoPokemonViewModel
    {
        private readonly PokemonUserRepository _repo;

        public ObservableCollection<PokemonUser> Equipo { get; set; }

        public EquipoPokemonViewModel(string userId)
        {
            _repo = new PokemonUserRepository();
            Equipo = new ObservableCollection<PokemonUser>();

            // Cargar de forma asincrónica para no bloquear el UI
            _ = CargarEquipoAsync(userId);
        }

        private async Task CargarEquipoAsync(string userId)
        {
            var lista = await Task.Run(() =>
                _repo.GetPokemonsByUser(userId)
                    .OrderBy(p => p.PokemonId)
                    .ToList()
            );

            foreach (var p in lista)
            {
                Console.WriteLine($"Pokemon: {p.Nombre}, Nivel: {p.Nivel}, Cantidad: {p.Cantidad}");
                Equipo.Add(p);
            }
        }
    }
}
