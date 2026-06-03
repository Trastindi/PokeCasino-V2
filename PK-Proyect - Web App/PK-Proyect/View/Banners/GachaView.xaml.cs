using PK_Proyect.ViewModels;
using System.Windows;
using PK_Proyect.Models;
using PK_Proyect.ViewModels.Banners;

namespace PK_Proyect.View.Banners
{
    public partial class GachaView : Window
    {
        public GachaView(User usuario, string zonaNombre)
        {
            InitializeComponent();
            DataContext = new GachaViewModel(usuario, zonaNombre);
        }
    }
}
