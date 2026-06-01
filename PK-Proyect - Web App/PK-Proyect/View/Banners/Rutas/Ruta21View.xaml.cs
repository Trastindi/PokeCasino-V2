using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta21View : Window
    {
        public Ruta21View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta21ViewModel(usuario);
        }


    }
}