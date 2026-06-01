using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta18View : Window
    {
        public Ruta18View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta18ViewModel(usuario);
        }


    }
}