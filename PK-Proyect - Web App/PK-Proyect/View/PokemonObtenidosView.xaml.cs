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
    }
}
