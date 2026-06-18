using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class MisMensajesViewModel
    {
        private readonly MensajeRepository _repo;

        /// <summary>ID del usuario que está logueado (el rival que acepta el desafío).</summary>
        public string CurrentUserId { get; }

        public ObservableCollection<Mensaje> Mensajes { get; set; }

        private Mensaje _mensajeSeleccionado;
        public Mensaje MensajeSeleccionado
        {
            get => _mensajeSeleccionado;
            set
            {
                _mensajeSeleccionado = value;
                ((RelayCommand)AceptarDesafioCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RechazarDesafioCommand).RaiseCanExecuteChanged();
                ((RelayCommand)AceptarIntercambioCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RechazarIntercambioCommand).RaiseCanExecuteChanged();
                ((RelayCommand)AbrirIntercambioCommand).RaiseCanExecuteChanged();
            }
        }

        // --- Eventos hacia la View ---
        public event Action<string> BatallaAceptada;
        public event Action<string> IntercambioAceptado;
        public event Action<string> IntercambioAbiertoPorRemitente;

        // --- Comandos ---
        public ICommand SeleccionarMensajeCommand  { get; }
        public ICommand AceptarDesafioCommand      { get; }
        public ICommand RechazarDesafioCommand     { get; }
        public ICommand AceptarIntercambioCommand  { get; }
        public ICommand RechazarIntercambioCommand { get; }
        public ICommand AbrirIntercambioCommand    { get; }

        public MisMensajesViewModel(string currentUserId = "")
        {
            CurrentUserId = currentUserId;
            _repo    = new MensajeRepository();
            Mensajes = new ObservableCollection<Mensaje>();

            SeleccionarMensajeCommand = new RelayCommand(
                param => MensajeSeleccionado = param as Mensaje
            );

            AceptarDesafioCommand = new RelayCommand(
                _ => AceptarDesafio(),
                _ => MensajeSeleccionado?.Tipo == "battle_request"
            );

            RechazarDesafioCommand = new RelayCommand(
                _ => RechazarDesafio(),
                _ => MensajeSeleccionado?.Tipo == "battle_request"
            );

            AceptarIntercambioCommand = new RelayCommand(
                _ => AceptarIntercambio(),
                _ => MensajeSeleccionado?.Tipo == "trade_request"
            );

            RechazarIntercambioCommand = new RelayCommand(
                _ => RechazarIntercambio(),
                _ => MensajeSeleccionado?.Tipo == "trade_request"
            );

            AbrirIntercambioCommand = new RelayCommand(
                _ => AbrirIntercambioExistente(),
                _ => MensajeSeleccionado?.Tipo == "trade_response"
            );

            _ = CargarMensajesAsync();
        }

        // ── Acciones ────────────────────────────────────────────────────────────

        private void AceptarDesafio()
        {
            if (MensajeSeleccionado == null) return;

            var battleId = MensajeSeleccionado.TipoBatallaId;
            if (string.IsNullOrEmpty(battleId))
            {
                MessageBox.Show(
                    "Este mensaje no contiene un ID de batalla válido (campo battle_id vacío).\n" +
                    "Puede ser un mensaje antiguo sin ese campo.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _ = ResponderDesafioAsync(MensajeSeleccionado.Id, battleId, accepted: true);
        }

        private async Task ResponderDesafioAsync(string msgId, string battleId, bool accepted)
        {
            try
            {
                var ok = await Task.Run(() => _repo.ResponderDesafio(msgId, accepted));

                if (!ok)
                {
                    MessageBox.Show("Error al comunicar la respuesta al servidor.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    Mensajes.Remove(MensajeSeleccionado);
                    MensajeSeleccionado = null;
                });

                if (accepted)
                    BatallaAceptada?.Invoke(battleId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aceptar desafío: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RechazarDesafio()
        {
            if (MensajeSeleccionado == null) return;
            var id = MensajeSeleccionado.Id;
            await Task.Run(() => _repo.ResponderDesafio(id, accepted: false));
            App.Current.Dispatcher.Invoke(() =>
            {
                Mensajes.Remove(MensajeSeleccionado);
                MensajeSeleccionado = null;
            });
        }

        private void AceptarIntercambio()
        {
            if (MensajeSeleccionado == null) return;
            var tradeId = MensajeSeleccionado.TradeId;
            if (string.IsNullOrEmpty(tradeId))
                tradeId = MensajeSeleccionado.Id;
            IntercambioAceptado?.Invoke(tradeId);
        }

        private async void RechazarIntercambio()
        {
            if (MensajeSeleccionado == null) return;
            var id = MensajeSeleccionado.Id;
            await Task.Run(() => _repo.EliminarMensaje(id));
            App.Current.Dispatcher.Invoke(() =>
            {
                Mensajes.Remove(MensajeSeleccionado);
                MensajeSeleccionado = null;
            });
        }

        private void AbrirIntercambioExistente()
        {
            if (MensajeSeleccionado == null) return;
            var tradeId = MensajeSeleccionado.TradeId;
            if (string.IsNullOrEmpty(tradeId))
            {
                MessageBox.Show(
                    "Este mensaje no contiene un ID de intercambio válido.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            IntercambioAbiertoPorRemitente?.Invoke(tradeId);
        }

        private async Task CargarMensajesAsync()
        {
            var lista = await Task.Run(() =>
                _repo.GetMisMensajes()
                     .OrderByDescending(m => m.Fecha)
                     .ToList()
            );
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var m in lista)
                    Mensajes.Add(m);
            });
        }
    }
}
