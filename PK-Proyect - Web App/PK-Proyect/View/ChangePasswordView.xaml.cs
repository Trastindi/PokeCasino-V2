using PK_Proyect.Models;
using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class ChangePasswordView : Window
    {
        private readonly ChangePasswordViewModel _vm;

        public ChangePasswordView(User usuario)
        {
            InitializeComponent();
            _vm = new ChangePasswordViewModel(usuario);
            DataContext = _vm;

            // Cuando el ViewModel diga "cerrar", cerramos la ventana
            _vm.CerrarVentana += () => this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TxtActual_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _vm.PasswordActual = ((PasswordBox)sender).Password;
        }

        private void TxtNueva_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _vm.PasswordNueva = ((PasswordBox)sender).Password;
        }

        private void TxtConfirmar_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _vm.PasswordConfirmar = ((PasswordBox)sender).Password;
        }
        private void BtnCambiar_Click(object sender, RoutedEventArgs e)
        {
            // Forzar actualización de los PasswordBox
            _vm.PasswordActual = TxtActual.Password;
            _vm.PasswordNueva = TxtNueva.Password;
            _vm.PasswordConfirmar = TxtConfirmar.Password;

            _vm.CambiarCommand.Execute(null);
        }



    }
}
