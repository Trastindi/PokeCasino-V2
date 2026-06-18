using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PK_Proyect.ViewModels
{
    public class DetalleEquipoViewModel
    {
        private readonly PokemonUserRepository _pokemonRepo;
        private readonly string               _userId;

        public Equipo Equipo { get; }

        public ObservableCollection<PokemonUser> Integrantes { get; } = new ObservableCollection<PokemonUser>();

        /// <summary>Solo usado en modo selección de batalla.</summary>
        public event Action<ObservableCollection<PokemonUser>> EquipoSeleccionado;

        public DetalleEquipoViewModel(string userId, Equipo equipo)
        {
            _userId      = userId;
            Equipo       = equipo;
            _pokemonRepo = new PokemonUserRepository();

            _ = CargarIntegrantesAsync();
        }

        private async Task CargarIntegrantesAsync()
        {
            var todosLosPokemon = await Task.Run(() => _pokemonRepo.GetPokemonsByUser(_userId));

            // Filtrar solo los pokemon cuyo PokemonId está en la lista de integrantes del equipo
            var integrantes = todosLosPokemon
                .Where(p => Equipo.Integrantes.Contains(p.PokemonId))
                .OrderBy(p => Equipo.Integrantes.IndexOf(p.PokemonId))
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Integrantes.Clear();
                foreach (var p in integrantes)
                    Integrantes.Add(p);
            });
        }
    }
}
