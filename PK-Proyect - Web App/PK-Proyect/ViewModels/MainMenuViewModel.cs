using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Services;
using PK_Proyect.View;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class MainMenuViewModel
    {
        public User UsuarioConectado { get; }

        private readonly UserService _userService;

        public event Action CerrarSesionRequested;

        public ICommand AbrirCasinoCommand { get; }
        public ICommand AbrirMapaCommand { get; }
        public ICommand AbrirEquipoCommand { get; }
        public ICommand AbrirMedallasCommand { get; }
        public ICommand AbrirPerfilCommand { get; }
        public ICommand CerrarSesionCommand { get; }

        public MainMenuViewModel(User usuario, UserService userService)
        {
            UsuarioConectado = usuario;
            _userService     = userService;

            AbrirCasinoCommand   = new RelayCommand(async _ => await AbrirCasinoAsync());
            AbrirMapaCommand     = new RelayCommand(_ => AbrirMapa());
            AbrirEquipoCommand   = new RelayCommand(_ => AbrirEquipo());
            AbrirMedallasCommand = new RelayCommand(_ => AbrirMedallas());
            AbrirPerfilCommand   = new RelayCommand(async _ => await AbrirPerfilAsync());
            CerrarSesionCommand  = new RelayCommand(_ => CerrarSesion());
        }

        private void AbrirMedallas()
        {
            var ventana = new MedallasView(new MedallasViewModel(UsuarioConectado.Id));
            ventana.ShowDialog();
        }

        private async Task AbrirCasinoAsync()
        {
            try
            {
                var userActualizado = await Task.Run(() => _userService.GetUserById(UsuarioConectado.Id));
                var casino = new SlotMachineView(userActualizado, new CasinoService());
                casino.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error abriendo casino: " + ex.Message);
            }
        }

        private void AbrirEquipo()
        {
            var ventana = new EquipoPokemonView(new EquipoPokemonViewModel(UsuarioConectado.Id));
            ventana.ShowDialog();
        }

        private void CerrarSesion()
        {
            if (MessageBox.Show("¿Cerrar sesión?", "Confirmar", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            CerrarSesionRequested?.Invoke();
        }

        private async Task AbrirPerfilAsync()
        {
            try
            {
                var perfil = new PerfilUserView(UsuarioConectado, new UserService());
                await Task.Yield();
                perfil.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error abriendo perfil: " + ex.Message);
            }
        }

        private void AbrirMapa()
        {
            new MapaKantoView(UsuarioConectado).Show();
        }
    }
}
