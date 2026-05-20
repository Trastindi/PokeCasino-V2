using PK_Proyect.ViewModels.Banners;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners
{
    public partial class LaboratorioView : Window
    {
        public LaboratorioView(User usuario)
        {
            InitializeComponent();
            DataContext = new LaboratorioViewModel(usuario);
        }

        
    }
}
