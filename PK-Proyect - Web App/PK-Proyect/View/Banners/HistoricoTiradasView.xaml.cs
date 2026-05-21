using System.Windows;
using PK_Proyect.ViewModels;

namespace PK_Proyect.Views
{
    public partial class HistoricoTiradasView : Window
    {
        public HistoricoTiradasView(HistoricoTiradasViewModel vm)
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
