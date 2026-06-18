using PK_Proyect.Services;
using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class BattleWindowView : Window
    {
        /// <param name="battleId">Id de la batalla en BD. Null → modo demo.</param>
        /// <param name="myPlayerId">Id del jugador local.</param>
        public BattleWindowView(IBattleService battleService,
                                string battleId   = null,
                                string myPlayerId = null)
        {
            InitializeComponent();

            var vm = new BattleWindowViewModel(battleService, battleId, myPlayerId);
            vm.OwnerWindow = this;
            DataContext = vm;
        }

        // Backspace: volver del panel de movimientos al menú de acciones
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back &&
                DataContext is BattleWindowViewModel vm &&
                vm.ShowMoves)
            {
                vm.BackToActionsCommand.Execute(null);
                e.Handled = true;
            }
        }

        // Parar polling al cerrar la ventana
        private void Window_Closed(object sender, System.EventArgs e)
        {
            if (DataContext is BattleWindowViewModel vm)
                vm.Dispose();
        }
    }
}
