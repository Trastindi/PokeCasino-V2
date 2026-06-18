using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class DetalleEquipoView : Window
    {
        public DetalleEquipoView(DetalleEquipoViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}
