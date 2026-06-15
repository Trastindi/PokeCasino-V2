using System.Windows;
using PK_Proyect.Services;
using PK_Proyect.ViewModels;

namespace PK_Proyect.View
{
    public partial class BattleWindowView : Window
    {
        public BattleWindowView(IBattleService battleService, string battleId = null)
        {
            InitializeComponent();
            DataContext = new BattleWindowViewModel(battleService, battleId);
        }
    }
}
