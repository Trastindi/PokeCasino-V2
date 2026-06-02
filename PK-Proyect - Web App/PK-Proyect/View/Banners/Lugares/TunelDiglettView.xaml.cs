using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class TunelDiglettView : Window
    {
        public TunelDiglettView(User usuario)
        {
            InitializeComponent();
            DataContext = new TunelDiglettViewModel(usuario);
        }


    }
}
