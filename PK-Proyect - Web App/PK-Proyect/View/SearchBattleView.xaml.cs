using System.Windows;
using PK_Proyect.Services;
using PK_Proyect.ViewModels;

namespace PK_Proyect.View
{
    public partial class SearchBattle : Window
    {
        private readonly SearchBattleViewModel _vm;

        public SearchBattle(IBattleService battleService)
        {
            InitializeComponent();
            _vm = new SearchBattleViewModel(battleService, Close);
            _vm.BattleAccepted += OnBattleAccepted;
            DataContext = _vm;
        }

        private void OnBattleAccepted()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var battleWindow = new BattleWindow();
                battleWindow.Owner = this.Owner;
                battleWindow.Show();
                this.Close();
            });
        }
    }
}
