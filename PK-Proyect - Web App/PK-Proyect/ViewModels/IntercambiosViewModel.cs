using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PK_Proyect.Models;
using PK_Proyect.Repositories;

namespace PK_Proyect.ViewModels
{
    public class IntercambiosViewModel : INotifyPropertyChanged
    {
        private readonly ITradeRepository      _tradeRepo;
        private readonly PokemonUserRepository _pokemonRepo;

        public ObservableCollection<TradeModel>  MisIntercambios { get; } = new();
        public ObservableCollection<PokemonUser> MisPokemon      { get; } = new();

        // ID del intercambio que viene del mensaje aceptado (se asigna desde MisMensajesView)
        public string TradeIdPendiente { get; set; }

        private string _rivalId = string.Empty;
        public string RivalId
        {
            get => _rivalId;
            set { _rivalId = value; OnPropertyChanged(); }
        }

        private TradeModel _intercambioActivo;
        public TradeModel IntercambioActivo
        {
            get => _intercambioActivo;
            set
            {
                _intercambioActivo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HayIntercambioActivo));
                OnPropertyChanged(nameof(NoHayIntercambioActivo));
            }
        }
        public bool HayIntercambioActivo   => IntercambioActivo != null;
        public bool NoHayIntercambioActivo => IntercambioActivo == null;

        private PokemonUser _pokemonOfrecido;
        public PokemonUser PokemonOfrecido
        {
            get => _pokemonOfrecido;
            set { _pokemonOfrecido = value; OnPropertyChanged(); }
        }

        private bool _estaCargando;
        public bool EstaCargando
        {
            get => _estaCargando;
            set { _estaCargando = value; OnPropertyChanged(); }
        }

        private string _mensaje = string.Empty;
        public string Mensaje
        {
            get => _mensaje;
            set { _mensaje = value; OnPropertyChanged(); }
        }

        private bool _intercambioExitoso;
        public bool IntercambioExitoso
        {
            get => _intercambioExitoso;
            set { _intercambioExitoso = value; OnPropertyChanged(); }
        }

        public IntercambiosViewModel(ITradeRepository tradeRepo, PokemonUserRepository pokemonRepo)
        {
            _tradeRepo   = tradeRepo;
            _pokemonRepo = pokemonRepo;
        }

        // ── Métodos públicos ─────────────────────────────────────────

        /// <summary>
        /// Llamado al abrir la ventana y al pulsar Recargar.
        /// Si hay un TradeIdPendiente (del mensaje aceptado) lo carga directamente.
        /// Si no, busca en el historial el primer intercambio en estado 'pending' o 'active'.
        /// </summary>
        public async Task IntentarReanudarIntercambioAsync()
        {
            // 1. Si tenemos un tradeId concreto del mensaje, úsalo directamente
            if (!string.IsNullOrEmpty(TradeIdPendiente))
            {
                await CargarIntercambioPublicoAsync(TradeIdPendiente);
                TradeIdPendiente = null;
                return;
            }

            // 2. Si ya hay uno activo, solo refresca
            if (IntercambioActivo != null)
            {
                await RefrescarIntercambioPublicoAsync();
                return;
            }

            // 3. Buscar en el historial el primer intercambio activo o pendiente
            var activo = MisIntercambios.FirstOrDefault(t =>
                t.Status == "pending" || t.Status == "active" || t.Status == "waiting");

            if (activo != null)
                await CargarIntercambioPublicoAsync(activo.Id);
        }

        public async Task EnviarSolicitudAsync()
        {
            if (string.IsNullOrWhiteSpace(RivalId))
            {
                Mensaje = "Introduce el ID del usuario con quien intercambiar.";
                return;
            }
            EstaCargando = true;
            Mensaje = string.Empty;
            try
            {
                var ok = await _tradeRepo.SendTradeRequestAsync(RivalId);
                Mensaje = ok != null
                    ? "Solicitud enviada. Esperando respuesta..."
                    : "Error al enviar la solicitud.";
            }
            catch (Exception ex) { Mensaje = $"Error: {ex.Message}"; }
            finally { EstaCargando = false; }
        }

        public async Task AceptarSolicitudAsync(string msgId)
        {
            EstaCargando = true;
            Mensaje = string.Empty;
            try
            {
                var tradeId = await _tradeRepo.RespondTradeRequestAsync(msgId, accepted: true);
                if (tradeId != null)
                {
                    Mensaje = "¡Solicitud aceptada! Selecciona el Pokémon que quieres ofrecer.";
                    await CargarIntercambioPublicoAsync(tradeId);
                    await CargarMisPokemonAsync();
                }
                else
                {
                    Mensaje = "Error al aceptar la solicitud.";
                }
            }
            catch (Exception ex) { Mensaje = $"Error: {ex.Message}"; }
            finally { EstaCargando = false; }
        }

        public async Task RechazarSolicitudAsync(string msgId)
        {
            EstaCargando = true;
            try
            {
                await _tradeRepo.RespondTradeRequestAsync(msgId, accepted: false);
                Mensaje = "Solicitud rechazada.";
            }
            catch (Exception ex) { Mensaje = $"Error: {ex.Message}"; }
            finally { EstaCargando = false; }
        }

        public async Task OfrecerPokemonAsync()
        {
            if (IntercambioActivo == null || PokemonOfrecido == null)
            {
                Mensaje = "Selecciona un Pokémon para ofrecer.";
                return;
            }
            EstaCargando = true;
            try
            {
                var ok = await _tradeRepo.OfferPokemonAsync(IntercambioActivo.Id, PokemonOfrecido.Id);
                Mensaje = ok
                    ? "Pokémon ofrecido. Esperando al otro jugador..."
                    : "Error al ofrecer el Pokémon.";
                if (ok) await RefrescarIntercambioPublicoAsync();
            }
            catch (Exception ex) { Mensaje = $"Error: {ex.Message}"; }
            finally { EstaCargando = false; }
        }

        public async Task ConfirmarIntercambioAsync()
        {
            if (IntercambioActivo == null) return;
            EstaCargando = true;
            try
            {
                var status = await _tradeRepo.ConfirmTradeAsync(IntercambioActivo.Id);
                if (status == "done")
                {
                    IntercambioExitoso = true;
                    Mensaje = "¡Intercambio completado con éxito! 🎉";
                    IntercambioActivo = null;
                    await CargarMisIntercambiosAsync();
                }
                else
                {
                    Mensaje = "Confirmación registrada. Esperando al otro jugador...";
                    await RefrescarIntercambioPublicoAsync();
                }
            }
            catch (Exception ex) { Mensaje = $"Error: {ex.Message}"; }
            finally { EstaCargando = false; }
        }

        public async Task CancelarIntercambioAsync()
        {
            if (IntercambioActivo == null) return;
            EstaCargando = true;
            try
            {
                await _tradeRepo.CancelTradeAsync(IntercambioActivo.Id);
                Mensaje = "Intercambio cancelado.";
                IntercambioActivo = null;
            }
            catch (Exception ex) { Mensaje = $"Error: {ex.Message}"; }
            finally { EstaCargando = false; }
        }

        public async Task CargarMisIntercambiosAsync()
        {
            EstaCargando = true;
            try
            {
                var lista = await _tradeRepo.GetMyTradesAsync();
                MisIntercambios.Clear();
                foreach (var t in lista) MisIntercambios.Add(t);
            }
            catch (Exception ex) { Mensaje = $"Error cargando intercambios: {ex.Message}"; }
            finally { EstaCargando = false; }
        }

        public async Task CargarIntercambioPublicoAsync(string tradeId)
        {
            var trade = await _tradeRepo.GetTradeAsync(tradeId);
            IntercambioActivo = trade;
            if (trade != null)
                await CargarMisPokemonAsync();
        }

        public async Task RefrescarIntercambioPublicoAsync()
        {
            if (IntercambioActivo == null) return;
            IntercambioActivo = await _tradeRepo.GetTradeAsync(IntercambioActivo.Id);
        }

        private async Task CargarMisPokemonAsync()
        {
            var lista = await Task.Run(() => _pokemonRepo.GetMisPokemon());
            MisPokemon.Clear();
            foreach (var p in lista) MisPokemon.Add(p);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
