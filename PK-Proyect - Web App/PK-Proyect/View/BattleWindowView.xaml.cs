using PK_Proyect.Repositories;
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

            // El userId ya no es necesario: el servidor lo deduce del JWT
            var equipoVM = new EquipoPokemonViewModel(modoSeleccion: true);
            var equipoView = new EquipoPokemonView(equipoVM, modoSeleccion: true);

            // Si cancela la selección, cerrar esta ventana
            equipoVM.SeleccionCancelada += () => this.Close();

            // Al confirmar equipo, montar el ViewModel de batalla y mostrar esta ventana
            equipoVM.EquipoConfirmado += equipoSeleccionado =>
            {
                equipoView.Close();
                DataContext = new BattleWindowViewModel(battleService, battleId);
                this.Show();
            };

            this.Hide();
            equipoView.ShowDialog();
        }
    }
}
