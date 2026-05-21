using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.ObjectModel;
using System.Linq;

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

            Cargar(userId);
        }

        private void Cargar(string userId)
        {
            Lista.Clear();

            var pokes = _repo.GetPokemonsByUser(userId)
                             .OrderByDescending(p => p.FechaObtenido)
                             .ToList();

            foreach (var p in pokes)
                Lista.Add(p);
        }
    }
}
