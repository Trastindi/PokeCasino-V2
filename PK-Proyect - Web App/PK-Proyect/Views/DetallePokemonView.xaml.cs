using PK_Proyect.Models;
using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.Views
{
    public partial class DetallePokemonView : Window
    {
        public DetallePokemonView(PokemonUser pokemon)
        {
            InitializeComponent();
            // Asignamos el ViewModel wrapper como DataContext
            DataContext = new DetallePokemonViewModel(pokemon);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => Close();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => DragMove();
    }
}
