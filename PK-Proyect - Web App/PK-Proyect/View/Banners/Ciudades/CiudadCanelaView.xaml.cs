using PK_Proyect.ViewModels.Banners.Ciudades;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Ciudades
{
    public partial class CiudadCanelaView : Window
    {
        public CiudadCanelaView(User usuario)
        {
            InitializeComponent();
            DataContext = new CiudadCanelaViewModel(usuario);
        }
    }
}
