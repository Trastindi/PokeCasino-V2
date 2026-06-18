using PK_Proyect.Models;
using PK_Proyect.View;
using PK_Proyect.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PK_Proyect.Views
{
    public partial class IntercambiosView : UserControl
    {
        private DispatcherTimer _pollingTimer;
        private IntercambiosViewModel VM => DataContext as IntercambiosViewModel;

        public IntercambiosView()
        {
            InitializeComponent();

            // Arrancar polling cuando el UserControl esté cargado
            Loaded   += OnLoaded;
            Unloaded += OnUnloaded;
        }

        // ── Polling: refresca el intercambio activo cada 4 segundos ──
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _pollingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4)
            };
            _pollingTimer.Tick += async (_, __) =>
            {
                if (VM?.HayIntercambioActivo == true)
                    await VM.RefrescarIntercambioPublicoAsync();
            };
            _pollingTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _pollingTimer?.Stop();
            _pollingTimer  = null;
        }

        // ── Botón: abrir ventana de selección de Pokémon ─────────────
        private void BtnElegirPokemon_Click(object sender, RoutedEventArgs e)
        {
            if (VM == null) return;

            var selVM  = new SeleccionarPokemonViewModel(VM.MisPokemon);
            var window = new SeleccionarPokemonWindow(selVM)
            {
                Owner = Window.GetWindow(this)
            };

            if (window.ShowDialog() == true && selVM.PokemonElegido != null)
                VM.PokemonOfrecido = selVM.PokemonElegido;
        }

        // ── Otros handlers ───────────────────────────────────────────
        private async void BtnEnviarSolicitud_Click(object sender, RoutedEventArgs e)
            => await VM.EnviarSolicitudAsync();

        private async void BtnOfrecer_Click(object sender, RoutedEventArgs e)
            => await VM.OfrecerPokemonAsync();

        private async void BtnConfirmar_Click(object sender, RoutedEventArgs e)
            => await VM.ConfirmarIntercambioAsync();

        private async void BtnCancelar_Click(object sender, RoutedEventArgs e)
            => await VM.CancelarIntercambioAsync();

        private async void BtnVerIntercambio_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tradeId)
                await VM.CargarIntercambioPublicoAsync(tradeId);
        }
    }
}
