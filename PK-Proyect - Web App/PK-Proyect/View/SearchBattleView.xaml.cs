using PK_Proyect.Services;
using PK_Proyect.ViewModels;
using System.Windows;

namespace PK_Proyect.View
{
    public partial class SearchBattleView : Window
    {
        public SearchBattleView(IBattleService battleService, string currentUserId)
        {
            InitializeComponent();

            var vm = new SearchBattleViewModel(
                battleService,
                currentUserId,
                closeAction: () => this.Close()
            );

            // Al aceptar la batalla, abrir EquipoPokemonView en modo selección
            vm.BattleAccepted += battleId =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var equipoVm   = new EquipoPokemonViewModel(modoSeleccion: true);
                    var equipoView = new EquipoPokemonView(equipoVm, modoSeleccion: true);

                    // Al confirmar el equipo elegido, enviarlo al servidor
                    equipoVm.EquipoConfirmado += async teamId =>
                    {
                        var ok = await vm.SubmitTeamAsync(teamId);
                        if (ok)
                        {
                            equipoView.Close();
                            this.Close();
                            var battleWindow = new BattleWindowView(battleService, currentUserId);
                            battleWindow.Show();
                        }
                        else
                        {
                            MessageBox.Show(
                                "No se pudo enviar el equipo. Inténtalo de nuevo.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    };

                    // Al cancelar la selección de equipo, solo cerrar esa ventana
                    equipoVm.SeleccionCancelada += () => equipoView.Close();

                    equipoView.Show();
                });
            };

            DataContext = vm;
        }
    }
}
