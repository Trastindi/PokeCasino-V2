using System.Windows;
using PK_Proyect.ViewModels;

namespace PK_Proyect.Views
{
    public partial class ResultadoTiradasView : Window
    {
        public ResultadoTiradasView(ResultadoTiradasViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Cerrar(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
