using System.Windows;
using PK_Proyect.ViewModels;

namespace PK_Proyect.View
{
    public partial class EquipoPokemonView : Window
    {
        public EquipoPokemonView(EquipoPokemonViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
