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

        /// <summary>Disparado cuando el RECEPTOR acepta: tradeId es el msgId a responder.</summary>
        public event Action<string> IntercambioAceptado;

        /// <summary>Disparado cuando el REMITENTE abre su trade_response: tradeId ya existe en BD.</summary>
        public event Action<string> IntercambioAbiertoPorRemitente;

        // --- Comandos ---
        public ICommand SeleccionarMensajeCommand  { get; }
        public ICommand AceptarDesafioCommand      { get; }
        public ICommand RechazarDesafioCommand     { get; }
        public ICommand AceptarIntercambioCommand  { get; }
        public ICommand RechazarIntercambioCommand { get; }

        /// <summary>
        /// Abre directamente un intercambio ya existente (para el remitente
        /// que recibe un mensaje de tipo "trade_response" cuando el rival acepta).
        /// </summary>
        public ICommand AbrirIntercambioCommand { get; }

        public MisMensajesViewModel()
        {
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

            // Disponible solo para mensajes tipo "trade_response" (el remitente recibe
            // este mensaje cuando el rival acepta; ya contiene el trade_id creado).
            AbrirIntercambioCommand = new RelayCommand(
                _ => AbrirIntercambioExistente(),
                _ => MensajeSeleccionado?.Tipo == "trade_response"
            );

            _ = CargarMensajesAsync();
        }

        // ── Acciones ─────────────────────────────────────────────────────

        private void AceptarDesafio()
        {
            if (MensajeSeleccionado == null) return;

            // El servidor espera el _id del documento battle_request,
            // no el campo tipoBatallaId (que no existe en ese documento).
            var id = MensajeSeleccionado.Id;
            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("Este mensaje no tiene un ID de batalla válido.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            BatallaAceptada?.Invoke(id);
        }

        private async void RechazarDesafio()
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

        private void AceptarIntercambio()
        {
            if (MensajeSeleccionado == null) return;

            // trade_id puede no existir en el doc de MongoDB.
            // En ese caso usamos el _id del propio mensaje como identificador del trade.
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

        /// <summary>
        /// Para el REMITENTE: el mensaje es de tipo "trade_response" y ya trae
        /// el trade_id generado por el servidor. Solo hay que abrir la ventana
        /// de intercambio y cargar ese trade directamente (sin volver a llamar
        /// a /trade_requests/.../respond).
        /// </summary>
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
