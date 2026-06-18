using PK_Proyect.Services;
using PK_Proyect.ViewModels;
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
                var intercambiosView = new IntercambiosView(tradeId);
                intercambiosView.Show();
            };
        }
    }
}
