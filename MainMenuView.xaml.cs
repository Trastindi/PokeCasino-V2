using PK_Proyect.Models;
using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class MainMenuView : Window
    {
        private readonly MainMenuViewModel _viewModel;

        public MainMenuView(User usuario)
        {
            InitializeComponent();

            _viewModel = new MainMenuViewModel(usuario);
            DataContext = _viewModel;
            

            _viewModel.CerrarSesionRequested += OnCerrarSesion;
        }

        private void OnCerrarSesion()
        {
            var login = new LoginView();
            Application.Current.MainWindow = login;
            login.Show();
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
