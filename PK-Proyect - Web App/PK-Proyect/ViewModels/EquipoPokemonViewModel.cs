using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Repositories;
using PK_Proyect.View;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class EquipoPokemonViewModel
    {
        private readonly EquipoRepository     _equipoRepo;
        private readonly PokemonUserRepository _pokemonRepo;
        private readonly string               _userId;

        // Modo selección: elegir equipo para batalla
        public bool ModoSeleccion { get; }

        public ObservableCollection<Equipo> Equipos { get; } = new ObservableCollection<Equipo>();

        // Eventos para modo selección (compatibilidad con BattleWindow)
        public event Action<ObservableCollection<PokemonUser>> EquipoConfirmado;
        public event Action SeleccionCancelada;

        public ICommand CrearEquipoCommand   { get; }
        public ICommand VerDetalleCommand    { get; }
        public ICommand CancelarCommand      { get; }

        public EquipoPokemonViewModel(string userId, bool modoSeleccion = false)
        {
            _userId       = userId;
            ModoSeleccion = modoSeleccion;
            _equipoRepo   = new EquipoRepository();
            _pokemonRepo  = new PokemonUserRepository();

            CrearEquipoCommand = new RelayCommand(_ => CrearEquipo());
            VerDetalleCommand  = new RelayCommand(equipo => AbrirDetalle(equipo as Equipo));
            CancelarCommand    = new RelayCommand(_ => SeleccionCancelada?.Invoke());

            _ = CargarEquiposAsync();
        }

        private async Task CargarEquiposAsync()
        {
            var lista = await Task.Run(() => _equipoRepo.GetEquiposByUser(_userId));
            Application.Current.Dispatcher.Invoke(() =>
            {
                Equipos.Clear();
                foreach (var e in lista)
                    Equipos.Add(e);
            });
        }

        private void CrearEquipo()
        {
            var dialog = new CrearEquipoDialog();
            if (dialog.ShowDialog() != true) return;

            var nombre = dialog.NombreEquipo?.Trim();
            if (string.IsNullOrEmpty(nombre)) return;

            _ = Task.Run(async () =>
            {
                var nuevo = await Task.Run(() => _equipoRepo.CrearEquipo(_userId, nombre));
                if (nuevo != null)
                    Application.Current.Dispatcher.Invoke(() => Equipos.Add(nuevo));
            });
        }

        private void AbrirDetalle(Equipo equipo)
        {
            if (equipo == null) return;
            var vm = new DetalleEquipoViewModel(_userId, equipo);
            var ventana = new DetalleEquipoView(vm);

            if (ModoSeleccion)
            {
                // En modo selección confirmamos el equipo elegido
                vm.EquipoSeleccionado += pokes =>
                {
                    EquipoConfirmado?.Invoke(pokes);
                    ventana.Close();
                };
            }

            ventana.ShowDialog();
        }
    }
}
