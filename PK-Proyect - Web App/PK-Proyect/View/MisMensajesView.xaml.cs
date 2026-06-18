using PK_Proyect.Services;
using PK_Proyect.ViewModels;
using PK_Proyect.View;
using System.Windows;

namespace PK_Proyect.View
{
    public partial class MisMensajesView : Window
    {
        public MisMensajesView(MisMensajesViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            // Cuando el VM notifique que se aceptó un desafío de batalla,
            // cerrar esta ventana y abrir BattleWindowView.
            vm.BatallaAceptada += battleId =>
            {
                this.Close();
                var battleService = new BattleService();
                var battleWindow  = new BattleWindowView(battleService, battleId, vm.MensajeSeleccionado?.RemitenteId);
                battleWindow.Show();
            };

            // Cuando el VM notifique que se aceptó un intercambio,
            // cerrar esta ventana y abrir IntercambiosView.
            vm.IntercambioAceptado += tradeId =>
            {
                this.Close();
                var intercambiosView = new IntercambiosView(tradeId);
                intercambiosView.Show();
            };
        }
    }
}
