using System.Collections.Generic;
using System.Windows;

namespace PK_Proyect.View
{
    public partial class ElegirMovimientoWindow : Window
    {
        /// <summary>Índice del movimiento que el usuario eligió borrar (0-3), o -1 si cerró sin elegir.</summary>
        public int IndiceElegido { get; private set; } = -1;

        public ElegirMovimientoWindow(string movimientoNuevo, List<string> movesetActual)
        {
            InitializeComponent();

            TxtMensaje.Text = $"¡{movimientoNuevo} quiere aprenderse!\n" +
                              "El moveset está lleno. ¿Qué movimiento deseas olvidar?";

            var botones = new[] { BtnMov0, BtnMov1, BtnMov2, BtnMov3 };
            for (int i = 0; i < botones.Length; i++)
            {
                botones[i].Content = i < movesetActual.Count
                    ? movesetActual[i]
                    : "(vacío)";
            }
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            var btn = (System.Windows.Controls.Button)sender;
            IndiceElegido = int.Parse((string)btn.Tag);
            DialogResult = true;
            Close();
        }
    }
}
