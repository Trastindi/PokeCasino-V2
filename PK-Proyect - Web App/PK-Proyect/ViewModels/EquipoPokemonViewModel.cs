using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System;
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

            _ = CargarEquipoAsync(userId);
        }

        private async Task CargarEquipoAsync(string userId)
        {
            try
            {
                Equipo.Clear();
                var lista = await Task.Run(() => _repo.GetPokemonsByUser(userId)
                    .OrderBy(p => p.PokemonId)
                    .ToList());

                foreach (var p in lista)
                    Equipo.Add(p);
            }
            catch
            {
            }
        }
    }
}
