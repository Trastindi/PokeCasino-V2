using PK_Proyect.Models;
using PK_Proyect.Services;
using PK_Proyect.View;
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

            // Tras registro exitoso → MainMenuView con el usuario ya autenticado
            vm.NavigateToMainMenuRequested += (user) =>
            {
                // MainMenuView(User) — un solo argumento, el UserService se crea internamente
                var menu = new MainMenuView(user);
                menu.Show();
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

        private void Button_Click(object sender, RoutedEventArgs e) { }
    }
}
