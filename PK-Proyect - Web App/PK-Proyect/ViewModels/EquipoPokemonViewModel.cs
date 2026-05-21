using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.ObjectModel;

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

            CargarEquipo(userId);
        }

        private void CargarEquipo(string userId)
        {
            Equipo.Clear();

            var lista = _repo.GetPokemonsByUser(userId)
                 .OrderBy(p => p.PokemonId)
                 .ToList();


            foreach (var p in lista)
            {
                Console.WriteLine($"Pokemon: {p.Nombre}, Nivel: {p.Nivel}, Cantidad: {p.Cantidad}");
                Equipo.Add(p);
            }



        }
    }
}
