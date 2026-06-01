using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta11View : Window
    {
        public Ruta11View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta11ViewModel(usuario);
        }


    }
}