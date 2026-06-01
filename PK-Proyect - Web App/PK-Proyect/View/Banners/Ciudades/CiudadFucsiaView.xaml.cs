using PK_Proyect.ViewModels.Banners.Ciudades;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Ciudades
{
    public partial class CiudadFucsiaView : Window
    {
        public CiudadFucsiaView(User usuario)
        {
            InitializeComponent();
            DataContext = new CiudadFucsiaViewModel(usuario);
        }
    }
}
