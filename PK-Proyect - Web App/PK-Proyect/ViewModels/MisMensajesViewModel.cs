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

        /// <summary>Lista de mensajes recibidos por el usuario autenticado.</summary>
        public ObservableCollection<Mensaje> Mensajes { get; set; }

        private Mensaje _mensajeSeleccionado;
        /// <summary>Mensaje actualmente seleccionado en la lista.</summary>
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
        public ICommand SeleccionarMensajeCommand    { get; }
        public ICommand AceptarDesafioCommand        { get; }
        public ICommand RechazarDesafioCommand       { get; }
        public ICommand AceptarIntercambioCommand    { get; }
        public ICommand RechazarIntercambioCommand   { get; }

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
                     && !string.IsNullOrEmpty(MensajeSeleccionado.TipoBatallaId)
            );

            RechazarDesafioCommand = new RelayCommand(
                _ => RechazarDesafio(),
                _ => MensajeSeleccionado?.Tipo == "battle_request"
            );

            AceptarIntercambioCommand = new RelayCommand(
                _ => AceptarIntercambio(),
                _ => MensajeSeleccionado?.Tipo == "trade_request"
                     && !string.IsNullOrEmpty(MensajeSeleccionado.TradeId)
            );

            RechazarIntercambioCommand = new RelayCommand(
                _ => RechazarIntercambio(),
                _ => MensajeSeleccionado?.Tipo == "trade_request"
            );

            _ = CargarMensajesAsync();
        }

        // ── Acciones ────────────────────────────────────────────────────────────

        private void AceptarDesafio()
        {
            if (MensajeSeleccionado == null
                || string.IsNullOrEmpty(MensajeSeleccionado.TipoBatallaId))
            {
                MessageBox.Show("Selecciona un mensaje de desafío primero.");
                return;
            }
            BatallaAceptada?.Invoke(MensajeSeleccionado.TipoBatallaId);
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
            if (MensajeSeleccionado == null
                || string.IsNullOrEmpty(MensajeSeleccionado.TradeId))
            {
                MessageBox.Show("Selecciona un mensaje de intercambio primero.");
                return;
            }
            IntercambioAceptado?.Invoke(MensajeSeleccionado.TradeId);
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
