using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta13View : Window
    {
        public Ruta13View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta13ViewModel(usuario);
        }


    }
}