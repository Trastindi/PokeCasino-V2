using PK_Proyect.Services;
using PK_Proyect.ViewModels;
using System.Windows;

namespace PK_Proyect.View
{
    public partial class BattleWindowView : Window
    {
        public BattleWindowView(IBattleService battleService, string battleId = null, string opponentId = null)
        {
            InitializeComponent();

            // Antes de entrar en la batalla, pedir al jugador que elija su equipo
            var equipoVM = new EquipoPokemonViewModel(
                userId: SessionManager.CurrentUserId,
                modoSeleccion: true
            );

            var equipoView = new EquipoPokemonView(equipoVM, modoSeleccion: true);

            // Si el usuario cancela la seleccion de equipo, cerrar esta ventana
            equipoVM.SeleccionCancelada += () =>
            {
                this.Close();
                return;
            };

            // Cuando el jugador confirme su equipo, inicializar el ViewModel de batalla
            equipoVM.EquipoConfirmado += equipoSeleccionado =>
            {
                equipoView.Close();
                DataContext = new BattleWindowViewModel(battleService, battleId, opponentId, equipoSeleccionado);
                this.Show();
            };

            // Mostrar primero el selector de equipo; la ventana de batalla aparece tras confirmar
            this.Hide();
            equipoView.ShowDialog();
        }
    }
}
