using PK_Proyect.ViewModels.Banners.Ciudades;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Ciudades
{
    /// <summary>
    /// Lógica de interacción para CiudadAzulonaView.xaml
    /// </summary>
    public partial class CiudadAzulonaView : Window
    {
        public CiudadAzulonaView(User usuario)
        {
            InitializeComponent();
            DataContext = new CiudadAzulonaViewModel(usuario);
        }
    }
}

