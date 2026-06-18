using PK_Proyect.ViewModels;
using System.Windows;

namespace PK_Proyect.View
{
    public partial class SwitchBattlePokemonWindow : Window
    {
        public SwitchBattlePokemonWindow(SwitchBattlePokemonViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
