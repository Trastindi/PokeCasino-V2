using PK_Proyect.Services;
using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PK_Proyect.View
{
    public partial class MainMenuView : Window
    {
        private readonly string _userId;

        public MainMenuView(string userId)
        {
            InitializeComponent();
            _userId = userId;
        }

        // Boton Buscar Combate
        private void BuscarCombate_Click(object sender, RoutedEventArgs e)
        {
            var battleService = new BattleService();
            var searchView = new SearchBattleView(battleService, _userId);
            searchView.ShowDialog();
        }
    }
}
