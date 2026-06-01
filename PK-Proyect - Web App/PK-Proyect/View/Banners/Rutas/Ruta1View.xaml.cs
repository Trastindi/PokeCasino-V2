using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta1View : Window
    {
        public Ruta1View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta1ViewModel(usuario);
        }


    }
}