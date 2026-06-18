using PK_Proyect.Services;
using PK_Proyect.ViewModels;
using PK_Proyect.Views;          // namespace correcto de IntercambiosView
using System.Windows;
using System.Windows.Controls;

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

                // IntercambiosView es un UserControl → se envuelve en una Window
                var uc     = new IntercambiosView();
                var window = new Window
                {
                    Title   = "Intercambios",
                    Content = uc,
                    Width   = 900,
                    Height  = 650,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                // Pasamos el tradeId al ViewModel una vez cargado el UserControl
                uc.Loaded += async (_, __) =>
                {
                    if (uc.DataContext is IntercambiosViewModel ivm && !string.IsNullOrEmpty(tradeId))
                        await ivm.CargarMisIntercambiosAsync();
                };

                window.Show();
            };
        }
    }
}
