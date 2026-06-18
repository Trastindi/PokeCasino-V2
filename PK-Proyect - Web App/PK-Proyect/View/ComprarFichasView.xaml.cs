// ﻿using PK_Proyect.Models;
// using PK_Proyect.Repositories;

// using System.Windows;

// namespace PK_Proyect.Views
// {
//     public partial class ComprarFichasWindow : Window
//     {
//         private User _usuario;
//         private UserRepository _repo;

//         public ComprarFichasWindow(User usuario)
//         {
//             InitializeComponent();
//             _usuario = usuario;
//             _repo = new UserRepository();
//         }

//         private void Comprar30(object sender, RoutedEventArgs e)
//         {
//             Comprar(30);
//         }

//         private void Comprar300(object sender, RoutedEventArgs e)
//         {
//             Comprar(300);
//         }

//         private void Comprar(int cantidad)
//         {
//             int precio = cantidad * 4;

//             if (_usuario.Pokes < precio)
//             {
//                 MessageBox.Show("No tienes suficientes pokes.");
//                 return;
//             }

//             _usuario.Pokes -= precio;
//             _usuario.FichasCasino += cantidad;

//             _repo.UpdateUser(_usuario);

//             MessageBox.Show($"Has comprado {cantidad} fichas.");

//             DialogResult = true;
//             Close();
//         }

//         private void Cancelar(object sender, RoutedEventArgs e)
//         {
//             DialogResult = false;
//             Close();
//         }
//     }
// }



using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace PK_Proyect.Views
{
    public partial class ComprarFichasWindow : Window
    {
        private User _usuario;
        private UserRepository _repo;

        // Ajusta este valor si tu precio real es distinto
        private const int PokesPorFicha = 4;

        public ComprarFichasWindow(User usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            _repo = new UserRepository();
        }

        private async void Comprar300(object sender, RoutedEventArgs e)
        {
            await ComprarAsync(30);
        }

        private async void Comprar3000(object sender, RoutedEventArgs e)
        {
            await ComprarAsync(300);
        }

        private async Task ComprarAsync(int cantidad)
        {
            // Evitar reentradas: deshabilitar solo los botones relevantes
            try
            {
                btnComprar30.IsEnabled = false;
                btnComprar300.IsEnabled = false;
                btnCancelar.IsEnabled = false;

                System.Diagnostics.Debug.WriteLine($"[ComprarAsync] Inicio cantidad={cantidad}");

                int precio = cantidad * PokesPorFicha;

                if (_usuario.Pokes < precio)
                {
                    MessageBox.Show("No tienes suficientes pokes.", "Compra fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Actualizar en memoria
                _usuario.Pokes -= precio;
                _usuario.FichasCasino += cantidad;

                // Persistir en background para no bloquear UI
                await Task.Run(() => _repo.UpdateUser(_usuario));
                System.Diagnostics.Debug.WriteLine("[ComprarAsync] UpdateUser terminado");


                this.Dispatcher.Invoke(() =>
                {
                                    

                    MessageBox.Show($"Has comprado {cantidad} fichas.", "Compra OK", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                });

              
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ComprarAsync] Excepción: {ex}");
                MessageBox.Show("Error al procesar la compra. Intenta de nuevo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Si la ventana sigue visible, reactivar botones
                if (this.IsVisible)
                {
                    btnComprar30.IsEnabled = true;
                    btnComprar300.IsEnabled = true;
                    btnCancelar.IsEnabled = true;
                }
            }
        }

        private void Cancelar(object sender, RoutedEventArgs e)
        {

            this.Dispatcher.Invoke(() =>
            {
                DialogResult = false;
            Close();
            });
        }
    }
}
