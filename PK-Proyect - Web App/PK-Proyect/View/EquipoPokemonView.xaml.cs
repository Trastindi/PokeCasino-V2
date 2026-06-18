using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class EquipoPokemonView : Window
    {
        // Constructor normal: gestión desde el menú
        public EquipoPokemonView(EquipoPokemonViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        // Constructor modo selección: elegir equipo antes de la batalla
        public EquipoPokemonView(EquipoPokemonViewModel vm, bool modoSeleccion)
        {
            InitializeComponent();
            DataContext = vm;

            if (modoSeleccion)
            {
                Title = "Elige tu equipo para la batalla";
                vm.EquipoConfirmado   += _ => Close();
                vm.SeleccionCancelada += () => Close();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}
