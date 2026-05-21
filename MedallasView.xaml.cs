using System.Windows;
using PK_Proyect.ViewModels;

namespace PK_Proyect.View
{
    public partial class MedallasView : Window
    {
        public MedallasView(MedallasViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
