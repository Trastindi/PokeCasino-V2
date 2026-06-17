using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PK_Proyect.ViewModels
{
    public class PokemonObtenidosViewModel
    {
        private readonly PokemonUserRepository _repo;
        private readonly string _userId;

        public ObservableCollection<PokemonUser> Lista { get; set; }

        public PokemonObtenidosViewModel(string userId)
        {
            _repo   = new PokemonUserRepository();
            _userId = userId;
            Lista   = new ObservableCollection<PokemonUser>();

            // Carga en background para no bloquear el hilo UI
            _ = CargarAsync();
        }

        private async Task CargarAsync()
        {
            var pokes = await Task.Run(() =>
                _repo.GetPokemonsByUser(_userId)
                     .OrderByDescending(p => p.FechaObtenido)
                     .ToList()
            );

            // Volver al hilo UI para modificar la ObservableCollection
            Application.Current.Dispatcher.Invoke(() =>
            {
                Lista.Clear();
                foreach (var p in pokes)
                    Lista.Add(p);
            });
        }
    }
}
