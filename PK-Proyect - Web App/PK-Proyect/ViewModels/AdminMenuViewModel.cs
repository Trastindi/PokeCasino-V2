using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Services;
using PK_Proyect.View;
using PK_Proyect.Repositories;
using PK_Proyect.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class AdminMenuViewModel : INotifyPropertyChanged
    {
        private readonly AdminService _adminService;

        public User UsuarioConectado { get; }

        private User _usuarioSeleccionado;
        public User UsuarioSeleccionado
        {
            get => _usuarioSeleccionado;
            set
            {
                _usuarioSeleccionado = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<User> Usuarios { get; set; }

        public event Action CerrarSesionRequested;

        public ICommand VerPokemonObtenidosCommand { get; }
        public ICommand VerEquipoCommand { get; }
        public ICommand VerMedallasCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand CambiarRolCommand { get; }
        public ICommand ResetPassCommand { get; }
        public ICommand VerPokesCommand { get; }
        public ICommand VerFichasCasinoCommand { get; }
        public ICommand CerrarSesionCommand { get; }
        public ICommand ActualizarCommand { get; }

        public AdminMenuViewModel(User usuario, AdminService adminService)
        {
            UsuarioConectado = usuario;
            _adminService = adminService;

            Usuarios = new ObservableCollection<User>();
            _ = ActualizarListaAsync(false);

            VerPokemonObtenidosCommand = new RelayCommand(_ => VerPokemonObtenidos());
            VerEquipoCommand = new RelayCommand(_ => VerEquipoPokemon());
            VerMedallasCommand = new RelayCommand(_ => VerMedallas());
            EditarCommand = new RelayCommand(_ => Pendiente("Editar usuario"));
            EliminarCommand = new RelayCommand(async _ => await EliminarUsuarioAsync());
            CambiarRolCommand = new RelayCommand(async _ => await CambiarRolAsync());
            ResetPassCommand = new RelayCommand(async _ => await ResetearPasswordAsync());
            VerPokesCommand = new RelayCommand(async _ => await EditarPokesAsync());
            VerFichasCasinoCommand = new RelayCommand(async _ => await EditarFichasCasinoAsync());
            CerrarSesionCommand = new RelayCommand(_ => CerrarSesion());
            ActualizarCommand = new RelayCommand(async _ => await ActualizarListaAsync(true));
        }

        private void Pendiente(string msg)
        {
            MessageBox.Show($"Función pendiente ({msg}).");
        }

        private void VerEquipoPokemon()
        {
            if (UsuarioSeleccionado == null)
            {
                MessageBox.Show("Selecciona un usuario.");
                return;
            }

            var vm = new EquipoPokemonViewModel(UsuarioSeleccionado.Id);
            var ventana = new EquipoPokemonView(vm);
            ventana.ShowDialog();
        }

        private void VerPokemonObtenidos()
        {
            if (UsuarioSeleccionado == null)
            {
                MessageBox.Show("Selecciona un usuario.");
                return;
            }

            var vm = new PokemonObtenidosViewModel(UsuarioSeleccionado.Id);
            var ventana = new PokemonObtenidosView(vm);
            ventana.ShowDialog();
        }

        private async Task EliminarUsuarioAsync()
        {
            if (UsuarioSeleccionado == null)
            {
                MessageBox.Show("Selecciona un usuario.");
                return;
            }

            var result = MessageBox.Show(
                $"¿Seguro que deseas eliminar a {UsuarioSeleccionado.Username}?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
                return;

            await Task.Run(() => _adminService.DeleteUser(UsuarioSeleccionado.Id));

            MessageBox.Show("Usuario eliminado correctamente.");
            await ActualizarListaAsync(false);
        }

        private async Task CambiarRolAsync()
        {
            if (UsuarioSeleccionado == null)
            {
                MessageBox.Show("Selecciona un usuario.");
                return;
            }

            string nuevoRol = UsuarioSeleccionado.Role == "admin" ? "user" : "admin";
            await Task.Run(() => _adminService.ChangeRole(UsuarioSeleccionado, nuevoRol));

            MessageBox.Show($"Rol cambiado a: {nuevoRol}");
            await ActualizarListaAsync(false);
        }

        private async Task ResetearPasswordAsync()
        {
            if (UsuarioSeleccionado == null)
            {
                MessageBox.Show("Selecciona un usuario.");
                return;
            }

            await Task.Run(() => _adminService.ResetPassword(UsuarioSeleccionado));
            MessageBox.Show("Contraseña reseteada y correo enviado.");
        }

        private async Task EditarPokesAsync()
        {
            if (UsuarioSeleccionado == null)
            {
                MessageBox.Show("Selecciona un usuario.");
                return;
            }

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                $"Pokes actuales: {UsuarioSeleccionado.Pokes}\nIntroduce el nuevo valor:",
                "Editar Pokes",
                UsuarioSeleccionado.Pokes.ToString()
            );

            if (int.TryParse(input, out int nuevoValor))
            {
                UsuarioSeleccionado.Pokes = nuevoValor;
                await Task.Run(() => _adminService.UpdateUser(UsuarioSeleccionado));

                MessageBox.Show("Pokes actualizados correctamente.");
                await ActualizarListaAsync(false);
            }
            else
            {
                MessageBox.Show("Valor inválido. Introduce un número.");
            }
        }

        private async Task EditarFichasCasinoAsync()
        {
            if (UsuarioSeleccionado == null)
            {
                MessageBox.Show("Selecciona un usuario.");
                return;
            }

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                $"Fichas actuales: {UsuarioSeleccionado.FichasCasino}\nIntroduce el nuevo valor:",
                "Editar Fichas del Casino",
                UsuarioSeleccionado.FichasCasino.ToString()
            );

            if (int.TryParse(input, out int nuevoValor))
            {
                UsuarioSeleccionado.FichasCasino = nuevoValor;
                await Task.Run(() => _adminService.UpdateUser(UsuarioSeleccionado));

                MessageBox.Show("Fichas actualizadas correctamente.");
                await ActualizarListaAsync(false);
            }
            else
            {
                MessageBox.Show("Valor inválido. Introduce un número.");
            }
        }

        private async Task ActualizarListaAsync(bool mostrarMensaje)
        {
            try
            {
                var usuarios = await Task.Run(() => _adminService.GetAllUsers());

                Usuarios.Clear();
                foreach (var u in usuarios)
                    Usuarios.Add(u);

                if (mostrarMensaje)
                    MessageBox.Show("Lista de usuarios actualizada.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error actualizando lista: " + ex.Message);
            }
        }

        private void CerrarSesion()
        {
            CerrarSesionRequested?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void VerMedallas()
        {
            if (UsuarioSeleccionado == null)
            {
                MessageBox.Show("Selecciona un usuario.");
                return;
            }

            var vm = new MedallasViewModel(UsuarioSeleccionado.Id);
            var ventana = new MedallasView(vm);
            ventana.ShowDialog();
        }
    }
}
