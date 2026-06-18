using System.Windows;
using System.Windows.Input;
using PK_Proyect.Models;
using PK_Proyect.ViewModels;

namespace PK_Proyect.View
{
    public partial class DetallePokemonView : Window
    {
        public DetallePokemonView(PokemonUser pokemon)
        {
            InitializeComponent();
            DataContext = new DetallePokemonViewModel(pokemon);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
            => Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
