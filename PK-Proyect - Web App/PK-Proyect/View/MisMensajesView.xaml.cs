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

            // Cuando el VM notifique que se acepto un desafio, abrir BattleWindow
            vm.BatallaAceptada += battleId =>
            {
                this.Close();
                var battleService = new BattleService();
                var battleWindow  = new BattleWindowView(battleService, battleId, vm.MensajeSeleccionado?.RemitenteId);
                battleWindow.Show();
            };
        }
    }
}
