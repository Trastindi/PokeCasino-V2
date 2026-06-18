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
            }
        }

        // --- Eventos hacia la View ---
        public event Action<string> BatallaAceptada;
        public event Action<string> IntercambioAceptado;

        // --- Comandos ---
        public ICommand SeleccionarMensajeCommand  { get; }
        public ICommand AceptarDesafioCommand      { get; }
        public ICommand RechazarDesafioCommand     { get; }
        public ICommand AceptarIntercambioCommand  { get; }
        public ICommand RechazarIntercambioCommand { get; }

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

            _ = CargarMensajesAsync();
        }

        // ── Acciones ─────────────────────────────────────────────────────

        private void AceptarDesafio()
        {
            if (MensajeSeleccionado == null) return;

            // Usa TipoBatallaId si existe, si no cae en el propio Id del mensaje
            var id = MensajeSeleccionado.TipoBatallaId;
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
