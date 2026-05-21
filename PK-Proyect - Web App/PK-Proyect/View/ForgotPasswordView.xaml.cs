using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class ForgotPasswordView : Window
    {
        public ForgotPasswordView()
        {
            InitializeComponent();

            var vm = new ForgotPasswordViewModel();
            DataContext = vm;

            vm.CerrarVentanaRequested += () => this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
