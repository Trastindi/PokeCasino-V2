using PK_Proyect.View;

using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PK_Proyect
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLogIn_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginView();
            login.Show();
        }

        private void BtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            var signup = new SignupView();
            signup.Show();
        }
    }
}
