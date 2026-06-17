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
    public class MainMenuViewModel
    {
        public User UsuarioConectado { get; }

        private readonly UserService _userService;

        public event Action CerrarSesionRequested;

        public ICommand AbrirCasinoCommand { get; }
        public ICommand AbrirMapaCommand { get; }
        public ICommand AbrirMenuPokemonCommand { get; }
        public ICommand AbrirMedallasCommand { get; }
        public ICommand AbrirPerfilCommand { get; }

        public ICommand AbrirBatallaCommand { get; }
        public ICommand CerrarSesionCommand { get; }

        public MainMenuViewModel(User usuario, UserService userService)
        {
            UsuarioConectado = usuario;
            _userService     = userService;

            AbrirCasinoCommand   = new RelayCommand(_ => AbrirCasino());
            AbrirMapaCommand     = new RelayCommand(_ => AbrirMapa());
            AbrirMenuPokemonCommand   = new RelayCommand(_ => AbrirMenuPokemon());
            AbrirMedallasCommand = new RelayCommand(_ => AbrirMedallas());
            AbrirPerfilCommand   = new RelayCommand(_ => AbrirPerfil());
            AbrirBatallaCommand = new RelayCommand(_ => AbrirBatalla());
            CerrarSesionCommand  = new RelayCommand(_ => CerrarSesion());
        }

        private void AbrirMedallas()
        {
            var ventana = new MedallasView(new MedallasViewModel(UsuarioConectado.Id));
            ventana.ShowDialog();
        }

        private void AbrirBatalla()
        {
            IBattleService battleService = new BattleService();

            var ventana = new SearchBattleView(
                battleService,
                UsuarioConectado
            );

            ventana.ShowDialog();
        }

        private async void AbrirCasino()
        {
            try
            {
                // Cargar usuario en background para no bloquear UI
                var userActualizado = await Task.Run(() => _userService.GetUserById(UsuarioConectado.Id));
                
                if (userActualizado != null)
                {
                    var casino = new SlotMachineView(userActualizado, new CasinoService());
                    casino.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el casino: {ex.Message}");
            }
        }

        private void AbrirMenuPokemon()
        {
            var ventana = new MenuPokemonView(new MenuPokemonViewModel(UsuarioConectado.Id));
            ventana.ShowDialog();
        }

        private void CerrarSesion()
        {
            if (MessageBox.Show("¿Cerrar sesión?", "Confirmar", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            CerrarSesionRequested?.Invoke();
        }

        private void AbrirPerfil()
        {
            var perfil = new PerfilUserView(UsuarioConectado, new UserService());
            perfil.Show();
        }

        private void AbrirMapa()
        {
            new MapaKantoView(UsuarioConectado).Show();
        }
    }
}
