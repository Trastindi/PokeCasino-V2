using System.Windows;
using PK_Proyect.ViewModels;

namespace PK_Proyect.Views
{
    public partial class MostrarPokemonView : Window
    {
        public MostrarPokemonView(MostrarPokemonViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Cerrar(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
