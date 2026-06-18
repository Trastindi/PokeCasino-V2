using System.Windows;
using System.Windows.Input;
using PK_Proyect.Models;
using PK_Proyect.ViewModels;

namespace PK_Proyect.View
{
    public partial class PokemonObtenidosView : Window
    {
        public PokemonObtenidosView(PokemonObtenidosViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void PokemonCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border &&
                border.DataContext is PokemonUser pokemon)
            {
                var detalle = new DetallePokemonView(pokemon);
                detalle.Owner = this;
                detalle.ShowDialog();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
            => Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnLimpiarBusqueda_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PokemonObtenidosViewModel vm)
                vm.TextoBusqueda = string.Empty;
        }
    }
}
