using PK_Proyect.Models;
using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class AdminMenuView : Window
    {
        private readonly AdminMenuViewModel _vm;

        public AdminMenuView(User usuario)
        {
            InitializeComponent();

            _vm = new AdminMenuViewModel(usuario);
            DataContext = _vm;

            _vm.CerrarSesionRequested += OnCerrarSesion;
        }

        private void OnCerrarSesion()
        {
            var login = new LoginView();
            login.Show();
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void dgUsuarios_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
