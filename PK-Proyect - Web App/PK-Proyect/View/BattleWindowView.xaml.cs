using PK_Proyect.Repositories;
using PK_Proyect.Services;
using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class BattleWindowView : Window
    {
        /// <param name="skipTeamSelection">
        /// Pasar true cuando el equipo ya fue enviado al servidor antes de abrir
        /// esta ventana (p.ej. desde SearchBattleView). En ese caso se salta la
        /// pantalla de selección de equipo y se muestra la batalla directamente.
        /// </param>
        public BattleWindowView(IBattleService battleService, string battleId = null,
                                string opponentId = null, bool skipTeamSelection = false)
        {
            InitializeComponent();

            if (skipTeamSelection)
            {
                var vm = new BattleWindowViewModel(battleService, battleId);
                vm.OwnerWindow = this;
                DataContext = vm;
                return;
            }

            var equipoVM = new EquipoPokemonViewModel(modoSeleccion: true);
            var equipoView = new EquipoPokemonView(equipoVM, modoSeleccion: true);

            equipoVM.SeleccionCancelada += () => this.Close();

            equipoVM.EquipoConfirmado += equipoSeleccionado =>
            {
                equipoView.Close();
                var vm = new BattleWindowViewModel(battleService, battleId);
                vm.OwnerWindow = this;
                DataContext = vm;
                this.Show();
            };

            this.Hide();
            equipoView.ShowDialog();
        }

        /// <summary>
        /// Backspace vuelve al menú de acciones si el panel de movimientos está abierto.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back && DataContext is BattleWindowViewModel vm && vm.ShowMoves)
            {
                vm.BackToActionsCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}
