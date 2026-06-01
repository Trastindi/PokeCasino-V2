using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta15View : Window
    {
        public Ruta15View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta15ViewModel(usuario);
        }


    }
}