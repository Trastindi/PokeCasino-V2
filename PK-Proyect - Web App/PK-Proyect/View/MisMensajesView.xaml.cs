using PK_Proyect.Repositories;
using PK_Proyect.Services;
using PK_Proyect.ViewModels;
using PK_Proyect.Views;
using System.Windows;

namespace PK_Proyect.View
{
    public partial class MisMensajesView : Window
    {
        public MisMensajesView(MisMensajesViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            // ── Batalla aceptada ──────────────────────────────────────────
            vm.BatallaAceptada += battleId =>
            {
                this.Close();
                var battleService = new BattleService();
                var battleWindow  = new BattleWindowView(battleService, battleId, vm.MensajeSeleccionado?.RemitenteId);
                battleWindow.Show();
            };

            // ── Intercambio aceptado (RECEPTOR) ───────────────────────────
            // El receptor pulsa "Aceptar": hay que llamar a AceptarSolicitudAsync
            // con el msgId para que el servidor cree el trade y devuelva el trade_id.
            vm.IntercambioAceptado += tradeId =>
            {
                this.Close();

                var tradeVM = new IntercambiosViewModel(
                    new TradeRepository(),
                    new PokemonUserRepository()
                );

                var uc = new IntercambiosView();
                uc.DataContext = tradeVM;

                var window = new Window
                {
                    Title   = "Intercambios",
                    Content = uc,
                    Width   = 900,
                    Height  = 650,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                uc.Loaded += async (_, __) =>
                {
                    await tradeVM.AceptarSolicitudAsync(tradeId);
                };

                window.Show();
            };

            // ── Intercambio abierto por el REMITENTE ──────────────────────
            // El remitente recibe un mensaje tipo "trade_response" con el trade_id
            // ya creado. Solo hay que cargar el trade existente, NO volver a llamar
            // a /trade_requests/.../respond (eso causaría un 404 de estado pending).
            vm.IntercambioAbiertoPorRemitente += tradeId =>
            {
                this.Close();

                var tradeVM = new IntercambiosViewModel(
                    new TradeRepository(),
                    new PokemonUserRepository()
                );

                var uc = new IntercambiosView();
                uc.DataContext = tradeVM;

                var window = new Window
                {
                    Title   = "Intercambios",
                    Content = uc,
                    Width   = 900,
                    Height  = 650,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                uc.Loaded += async (_, __) =>
                {
                    // Cargar directamente el trade por su ID sin intentar aceptar nada
                    await tradeVM.CargarIntercambioPublicoAsync(tradeId);
                    await tradeVM.CargarMisIntercambiosAsync();
                };

                window.Show();
            };
        }
    }
}
