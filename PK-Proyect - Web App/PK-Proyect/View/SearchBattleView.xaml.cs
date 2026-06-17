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

            // Al aceptar la batalla, cerrar esta ventana y abrir BattleWindowView
            vm.BattleAccepted += () =>
            {
                this.Close();
                var battleWindow = new BattleWindowView(battleService, currentUserId);
                battleWindow.Show();
            };

            DataContext = vm;
        }
    }
}
