using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class MesetaAnilView : Window
    {
        public MesetaAnilView(User usuario)
        {
            InitializeComponent();
            DataContext = new MesetaAnilViewModel(usuario);
        }


    }
}
