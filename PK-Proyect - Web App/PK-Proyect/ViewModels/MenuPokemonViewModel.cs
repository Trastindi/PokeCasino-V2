using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Repositories;
using PK_Proyect.Services;
using PK_Proyect.View;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class MenuPokemonViewModel
    {
        public User UsuarioConectado { get; }

        private readonly UserService _userService;

        public event Action CerrarSesionRequested;

        public ICommand AbrirGestionarEquipoCommand { get; }
        public ICommand AbrirEquipoCommand { get; }
        public ICommand AbrirPerfilCommand { get; } 

        public ICommand AbrirPokedexCommand { get; }
        public ICommand CerrarSesionCommand { get; }

        public MainMenuViewModel(User usuario, UserService userService)
        {
            UsuarioConectado = usuario;
            _userService     = userService;

            AbrirGestionarEquipoCommand = new RelayCommand(_ => AbrirGestionarEquipo());
            AbrirPokedexCommand = new RelayCommand(_ => AbrirPokedex());
			      AbrirEquipoCommand   = new RelayCommand(_ => AbrirEquipo());
            CerrarSesionCommand  = new RelayCommand(_ => CerrarSesion());
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
		
		private void AbrirPokedex()
        {
            var ventana = new PokedexView(new PokedexViewModel(UsuarioConectado.Id));
            ventana.ShowDialog();
        }

        private void AbrirPerfil()
        {
            var perfil = new PerfilUserView(UsuarioConectado, new UserService());
            perfil.Show();
        }

        private void AbrirGestionarEquipo()
        {
            var ventana = new GestionarEquipoView(new GestionarEquipoViewModel(UsuarioConectado.Id));
			ventana.ShowDialog();
        }
    }
}
