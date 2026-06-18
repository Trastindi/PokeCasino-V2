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

            vm.BatallaAceptada += battleId =>
            {
                this.Close();
                var battleService = new BattleService();
                var battleWindow  = new BattleWindowView(battleService, battleId, vm.MensajeSeleccionado?.RemitenteId);
                battleWindow.Show();
            };

            vm.IntercambioAceptado += tradeId =>
            {
                this.Close();

                // Crear el ViewModel del intercambio e inyectárselo al UserControl
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

                // Una vez cargado, aceptamos la solicitud usando el tradeId del mensaje
                uc.Loaded += async (_, __) =>
                {
                    await tradeVM.AceptarSolicitudAsync(tradeId);
                };

                window.Show();
            };
        }
    }
}
