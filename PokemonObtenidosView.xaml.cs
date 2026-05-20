using System.Windows;
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
    }
}
