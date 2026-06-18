using System.Windows;

namespace PK_Proyect.View
{
    public partial class CrearEquipoDialog : Window
    {
        public string NombreEquipo { get; private set; }

        public CrearEquipoDialog()
        {
            InitializeComponent();
        }

        private void btnCrear_Click(object sender, RoutedEventArgs e)
        {
            NombreEquipo = txtNombre.Text?.Trim();
            if (string.IsNullOrEmpty(NombreEquipo))
            {
                MessageBox.Show("Introduce un nombre para el equipo.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
