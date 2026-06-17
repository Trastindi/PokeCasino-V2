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

        public ICommand VerPokemonCommand      { get; }
        public ICommand AbrirPokedexCommand    { get; }
        public ICommand GestionarEquipoCommand { get; }
        public ICommand CerrarSesionCommand    { get; }

        public MenuPokemonViewModel(User usuario, UserService userService)
        {
            UsuarioConectado = usuario;
            _userService     = userService;

            VerPokemonCommand      = new RelayCommand(_ => AbrirPokemonObtenidos());
            AbrirPokedexCommand    = new RelayCommand(_ => AbrirPokedex());
            GestionarEquipoCommand = new RelayCommand(_ => AbrirEquipoPokemon());
            CerrarSesionCommand    = new RelayCommand(_ => CerrarSesion());
        }

        // Ver Pokemon -> PokemonObtenidosView
        private void AbrirPokemonObtenidos()
        {
            var ventana = new PokemonObtenidosView(new PokemonObtenidosViewModel(UsuarioConectado.Id));
            ventana.ShowDialog();
        }

        // Pokedex -> PokedexView
        private void AbrirPokedex()
        {
            var ventana = new PokedexView(UsuarioConectado.Id);
            ventana.ShowDialog();
        }

        // Gestionar Equipo -> EquipoPokemonView
        private void AbrirEquipoPokemon()
        {
            var ventana = new EquipoPokemonView(new EquipoPokemonViewModel(UsuarioConectado.Id));
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
