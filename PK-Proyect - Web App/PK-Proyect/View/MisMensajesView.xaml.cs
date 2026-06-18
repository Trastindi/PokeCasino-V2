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

            // ── Batalla aceptada (RIVAL) ──────────────────────────────────────────
            // Igual que el retador: primero elegir equipo, luego POST /battles/{id}/teams,
            // y solo después abrir BattleWindowView con el myPlayerId correcto.
            vm.BatallaAceptada += battleId =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var battleService = new BattleService();
                    var currentUserId = vm.CurrentUserId;

                    var equipoVm   = new EquipoPokemonViewModel(modoSeleccion: true);
                    var equipoView = new EquipoPokemonView(equipoVm, modoSeleccion: true);

                    equipoVm.EquipoConfirmado += async teamId =>
                    {
                        // Enviar equipo al servidor
                        var ok = await battleService.SubmitTeamAsync(battleId, teamId);
                        if (ok)
                        {
                            equipoView.Close();
                            this.Close();
                            var battleWindow = new BattleWindowView(
                                battleService,
                                battleId,
                                myPlayerId: currentUserId);
                            battleWindow.Show();
                        }
                        else
                        {
                            MessageBox.Show(
                                "No se pudo enviar el equipo. Inténtalo de nuevo.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    };

                    equipoVm.SeleccionCancelada += () => equipoView.Close();
                    equipoView.Show();
                });
            };

            // ── Intercambio aceptado (RECEPTOR) ───────────────────────────────────
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

            // ── Intercambio abierto por el REMITENTE ──────────────────────────────
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
                    await tradeVM.CargarIntercambioPublicoAsync(tradeId);
                    await tradeVM.CargarMisIntercambiosAsync();
                };

                window.Show();
            };
        }
    }
}
