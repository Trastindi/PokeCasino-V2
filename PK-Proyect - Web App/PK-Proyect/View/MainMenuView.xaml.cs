using PK_Proyect.Models;
using PK_Proyect.Services;
using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PK_Proyect.View
{
    public partial class MainMenuView : Window
    {
        private readonly string _userId;
        private readonly User _usuario;

        public MainMenuView(User usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            _userId  = usuario?.Id;
        }

        // Boton Buscar Combate
        private void BuscarCombate_Click(object sender, RoutedEventArgs e)
        {
            var battleService = new BattleService();
            var searchView = new SearchBattleView(battleService, _userId);
            searchView.ShowDialog();
        }

        // Boton Gestion Pokemon
        private void GestionPokemon_Click(object sender, RoutedEventArgs e)
        {
            var menuPokemon = new MenuPokemonView(_usuario, new UserService());
            menuPokemon.ShowDialog();
        }
    }
}
