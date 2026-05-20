using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PK_Proyect.View
{
    public partial class TrainerDataView : Window
    {
        public TrainerDataView(string username)
        {
            InitializeComponent();

            var vm = new TrainerDataViewModel(username);
            DataContext = vm;

            vm.CloseRequested += () => this.Close();
            vm.NavigateToLoginRequested += () =>
            {
                var login = new LoginView();
                login.Show();
                this.Close();
            };
        }



        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is TrainerDataViewModel vm)
                vm.Password = ((PasswordBox)sender).Password;
        }

        private void PasswordBoxConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is TrainerDataViewModel vm)
                vm.PasswordConfirm = ((PasswordBox)sender).Password;
        }

    }
}
