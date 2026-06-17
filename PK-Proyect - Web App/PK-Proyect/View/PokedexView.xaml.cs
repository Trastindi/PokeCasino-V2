using System.Windows;
using System.Windows.Input;
using PK_Proyect.ViewModels;

namespace PK_Proyect.View
{
    public partial class PokedexView : Window
    {
        public PokedexView(string userId)
        {
            InitializeComponent();
            DataContext = new PokedexViewModel(userId);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
