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
        private readonly EquipoRepository      _equipoRepo;

        public Equipo Equipo { get; }

        public ObservableCollection<PokemonUser> Integrantes { get; } = new ObservableCollection<PokemonUser>();

        /// <summary>Solo usado en modo selección de batalla.</summary>
        public event Action<ObservableCollection<PokemonUser>> EquipoSeleccionado;

        public DetalleEquipoViewModel(Equipo equipo)
        {
            Equipo       = equipo;
            _pokemonRepo = new PokemonUserRepository();
            _equipoRepo  = new EquipoRepository();

            _ = CargarIntegrantesAsync();
        }

        private async Task CargarIntegrantesAsync()
        {
            // Usamos /usuarios/mis_pokemon (filtra por JWT, no por userId en la URL)
            var todosLosPokemon = await Task.Run(() => _pokemonRepo.GetMisPokemon());

            // Filtramos por _id (ObjectId string) de PokemonUser
            var integrantes = todosLosPokemon
                .Where(p => Equipo.PokemonIds.Contains(p.Id))
                .OrderBy(p => Equipo.PokemonIds.IndexOf(p.Id))
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Integrantes.Clear();
                foreach (var p in integrantes)
                    Integrantes.Add(p);
            });
        }

        /// <summary>
        /// Confirma el equipo actual y lanza el evento EquipoSeleccionado (modo batalla).
        /// </summary>
        public void ConfirmarEquipo()
            => EquipoSeleccionado?.Invoke(Integrantes);

        /// <summary>
        /// Persiste los cambios en la lista de integrantes llamando al endpoint PUT.
        /// </summary>
        public async Task GuardarCambiosAsync()
        {
            var ids = Integrantes.Select(p => p.Id).ToList();
            await Task.Run(() => _equipoRepo.ActualizarEquipo(Equipo.Id, ids));
        }
    }
}
