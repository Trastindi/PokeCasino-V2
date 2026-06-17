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

            // Cuando la batalla sea aceptada, cerrar esta ventana y abrir BattleWindow
            vm.BattleAccepted += () =>
            {
                this.Close();
                var battleWindow = new BattleWindowView();
                battleWindow.Show();
            };

            DataContext = vm;
        }
    }
}
