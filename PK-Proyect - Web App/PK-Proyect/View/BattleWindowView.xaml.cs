using PK_Proyect.Repositories;
using PK_Proyect.Services;
using PK_Proyect.ViewModels;
using System.Windows;

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
                // El equipo ya fue enviado: montar el ViewModel y mostrar directamente
                DataContext = new BattleWindowViewModel(battleService, battleId);
                return;
            }

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
