using PK_Proyect.Models;
using PK_Proyect.Services;
using PK_Proyect.View;
using System;
using System.Windows;
using System.Windows.Input;
using PK_Proyect.Commands;

namespace PK_Proyect.ViewModels
{
    public class MainMenuViewModel
    {
        public User UsuarioConectado { get; }

        private readonly UserService _userService = new UserService();

        public event Action CerrarSesionRequested;

        public ICommand AbrirCasinoCommand { get; }
        public ICommand AbrirMapaCommand { get; }
        public ICommand AbrirEquipoCommand { get; }
        public ICommand AbrirMedallasCommand { get; }
        public ICommand AbrirPerfilCommand { get; }
        public ICommand CerrarSesionCommand { get; }

        public MainMenuViewModel(User usuario)
        {
            UsuarioConectado = usuario;

            AbrirCasinoCommand = new RelayCommand(_ => AbrirCasino());
            AbrirMapaCommand = new RelayCommand(_ => AbrirMapa());
            AbrirEquipoCommand = new RelayCommand(_ => AbrirEquipo());

            AbrirMedallasCommand = new RelayCommand(_ => AbrirMedallas());

            AbrirPerfilCommand = new RelayCommand(_ => AbrirPerfil());
            CerrarSesionCommand = new RelayCommand(_ => CerrarSesion());
        }
        private void AbrirMedallas()
        {
            var vm = new MedallasViewModel(UsuarioConectado.Id);
            var ventana = new MedallasView(vm);
            ventana.ShowDialog();
        }

        private void AbrirCasino()
        {
            // Recargar usuario desde la base de datos
            var userActualizado = _userService.GetUserById(UsuarioConectado.Id);

            var casino = new SlotMachineView(userActualizado);
            casino.ShowDialog();
        }

        private void AbrirEquipo()
        {
            // Cargar el equipo del usuario
            var vm = new EquipoPokemonViewModel(UsuarioConectado.Id);

            // Abrir la ventana retro
            var ventana = new EquipoPokemonView(vm);
            ventana.ShowDialog();
        }


        private void Pendiente(string msg)
        {
            MessageBox.Show($"Abrir {msg} (pendiente)");
        }

        private void CerrarSesion()
        {
            if (MessageBox.Show("¿Cerrar sesión?", "Confirmar", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            CerrarSesionRequested?.Invoke();
        }

        private void AbrirPerfil()
        {
            var perfil = new PerfilUserView(UsuarioConectado);
            perfil.Show();
        }

        private void AbrirMapa()
        {
            var mapa = new MapaKantoView(UsuarioConectado);
            mapa.Show();
        }

    }
}
