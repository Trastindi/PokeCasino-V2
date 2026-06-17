using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class EquipoPokemonViewModel
    {
        private readonly PokemonUserRepository _repo;

        // Modo normal: gestionar equipo. Modo seleccion: elegir equipo para batalla.
        public bool ModoSeleccion { get; }

        public ObservableCollection<PokemonUser> Equipo { get; set; }

        // Evento: el jugador confirmo su equipo (modo seleccion)
        public event Action<ObservableCollection<PokemonUser>> EquipoConfirmado;

        // Evento: el jugador cancelo la seleccion
        public event Action SeleccionCancelada;

        public ICommand ConfirmarEquipoCommand { get; }
        public ICommand CancelarCommand        { get; }

        public EquipoPokemonViewModel(string userId, bool modoSeleccion = false)
        {
            _repo         = new PokemonUserRepository();
            ModoSeleccion = modoSeleccion;
            Equipo        = new ObservableCollection<PokemonUser>();

            ConfirmarEquipoCommand = new RelayCommand(
                _ => EquipoConfirmado?.Invoke(Equipo),
                _ => Equipo.Count > 0
            );

            CancelarCommand = new RelayCommand(
                _ => SeleccionCancelada?.Invoke()
            );

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
                Equipo.Add(p);
        }
    }
}
