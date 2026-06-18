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
        private readonly EquipoRepository      _equipoRepo;
        private readonly PokemonUserRepository _pokemonRepo;

        public bool ModoSeleccion { get; }

        public ObservableCollection<Equipo> Equipos { get; } = new ObservableCollection<Equipo>();

        // EquipoConfirmado ahora lleva el team_id (string) para enviarlo al endpoint
        public event Action<string> EquipoConfirmado;
        public event Action SeleccionCancelada;

        public ICommand CrearEquipoCommand  { get; }
        public ICommand VerDetalleCommand   { get; }
        public ICommand ElegirEquipoCommand { get; }
        public ICommand CancelarCommand     { get; }

        public EquipoPokemonViewModel(bool modoSeleccion = false)
        {
            ModoSeleccion = modoSeleccion;
            _equipoRepo   = new EquipoRepository();
            _pokemonRepo  = new PokemonUserRepository();

            CrearEquipoCommand  = new RelayCommand(_ => CrearEquipo());
            VerDetalleCommand   = new RelayCommand(equipo => AbrirDetalle(equipo as Equipo));
            ElegirEquipoCommand = new RelayCommand(equipo => ElegirEquipo(equipo as Equipo));
            CancelarCommand     = new RelayCommand(_ => SeleccionCancelada?.Invoke());

            _ = CargarEquiposAsync();
        }

        private async Task CargarEquiposAsync()
        {
            try
            {
                var lista = await Task.Run(() => _equipoRepo.GetMisEquipos());
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Equipos.Clear();
                    foreach (var e in lista)
                        Equipos.Add(e);
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show($"Error al cargar equipos: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning));
            }
        }

        private void CrearEquipo()
        {
            var dialog = new CrearEquipoDialog();
            if (dialog.ShowDialog() != true) return;

            var nombre = dialog.NombreEquipo?.Trim();
            if (string.IsNullOrEmpty(nombre)) return;

            _ = Task.Run(async () =>
            {
                var nuevo = await Task.Run(() => _equipoRepo.CrearEquipo(nombre));
                if (nuevo != null)
                    Application.Current.Dispatcher.Invoke(() => Equipos.Add(nuevo));
            });
        }

        /// <summary>Modo normal: abre el detalle del equipo.</summary>
        private void AbrirDetalle(Equipo equipo)
        {
            if (equipo == null) return;
            var vm      = new DetalleEquipoViewModel(equipo);
            var ventana = new DetalleEquipoView(vm);
            ventana.ShowDialog();
        }

        /// <summary>Modo selección: confirma el equipo elegido enviando su ID.</summary>
        private void ElegirEquipo(Equipo equipo)
        {
            if (equipo == null) return;
            EquipoConfirmado?.Invoke(equipo.Id);
        }
    }
}
