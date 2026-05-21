using PK_Proyect.ViewModels.Banners.Ciudades;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Ciudades
{
    public partial class CiudadCelesteView : Window
    {
        public CiudadCelesteView(User usuario)
        {
            InitializeComponent();
            DataContext = new CiudadCelesteViewModel(usuario);
        }
    }
}
