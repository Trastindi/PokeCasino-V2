using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta17View : Window
    {
        public Ruta17View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta17ViewModel(usuario);
        }


    }
}