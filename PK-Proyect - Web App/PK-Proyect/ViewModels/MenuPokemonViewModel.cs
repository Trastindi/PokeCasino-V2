using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Services;
using PK_Proyect.View;
using System;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class MenuPokemonViewModel
    {
        public User UsuarioConectado { get; }

        private readonly UserService _userService;

        public event Action CerrarSesionRequested;

        public ICommand VerPokemonCommand          { get; }
        public ICommand AbrirPokedexCommand        { get; }
        public ICommand GestionarEquipoCommand     { get; }
        public ICommand AbrirMisMensajesCommand    { get; }
        public ICommand AbrirIntercambiosCommand   { get; }
        public ICommand CerrarSesionCommand        { get; }

        public MenuPokemonViewModel(User usuario, UserService userService)
        {
            UsuarioConectado = usuario;
            _userService     = userService;

            VerPokemonCommand         = new RelayCommand(_ => AbrirPokemonObtenidos());
            AbrirPokedexCommand       = new RelayCommand(_ => AbrirPokedex());
            GestionarEquipoCommand    = new RelayCommand(_ => AbrirEquipoPokemon());
            AbrirMisMensajesCommand   = new RelayCommand(_ => AbrirMisMensajes());
            AbrirIntercambiosCommand  = new RelayCommand(_ => AbrirIntercambios());
            CerrarSesionCommand       = new RelayCommand(_ => CerrarSesion());
        }

        private void AbrirPokemonObtenidos()
        {
            var ventana = new PokemonObtenidosView(new PokemonObtenidosViewModel(UsuarioConectado.Id));
            ventana.ShowDialog();
        }

        private void AbrirPokedex()
        {
            var ventana = new PokedexView(UsuarioConectado.Id);
            ventana.ShowDialog();
        }

        private void AbrirEquipoPokemon()
        {
            // El userId ya no es necesario: el servidor lo deduce del JWT
            var ventana = new EquipoPokemonView(new EquipoPokemonViewModel());
            ventana.ShowDialog();
        }

        private void AbrirMisMensajes()
        {
            var ventana = new MisMensajesView(new MisMensajesViewModel());
            ventana.ShowDialog();
        }

        private void AbrirIntercambios()
        {
            var ventana = new IntercambiosView(new IntercambiosViewModel());
            ventana.ShowDialog();
        }

        private void CerrarSesion()
        {
            if (MessageBox.Show("Cerrar sesion?", "Confirmar", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            CerrarSesionRequested?.Invoke();
        }
    }
}
