using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PK_Proyect.ViewModels
{
    public class PokemonObtenidosViewModel
    {
        private readonly PokemonUserRepository _repo;

        public ObservableCollection<PokemonUser> Lista { get; set; }

        public PokemonObtenidosViewModel(string userId)
        {
            _repo = new PokemonUserRepository();
            Lista = new ObservableCollection<PokemonUser>();

            _ = CargarAsync(userId);
        }

        private async Task CargarAsync(string userId)
        {
            try
            {
                Lista.Clear();
                var pokes = await Task.Run(() => _repo.GetPokemonsByUser(userId)
                    .OrderByDescending(p => p.FechaObtenido)
                    .ToList());

                foreach (var p in pokes)
                    Lista.Add(p);
            }
            catch
            {
            }
        }
    }
}
