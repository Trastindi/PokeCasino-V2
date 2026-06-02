using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class CentralElectricaView : Window
    {
        public CentralElectricaView(User usuario)
        {
            InitializeComponent();
            DataContext = new CentralElectricaViewModel(usuario);
        }


    }
}
