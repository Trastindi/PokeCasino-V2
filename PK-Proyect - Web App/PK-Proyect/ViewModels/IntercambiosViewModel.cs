using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using PK_Proyect.Models;
using PK_Proyect.Repositories;

namespace PK_Proyect.ViewModels
{
    public class IntercambiosViewModel : INotifyPropertyChanged
    {
        private readonly ITradeRepository      _tradeRepo;
        private readonly IPokemonUserRepository _pokemonRepo;

        // ── Observable collections ───────────────────────────────────
        public ObservableCollection<TradeModel>        MisIntercambios  { get; } = new();
        public ObservableCollection<PokemonUserModel>  MisPokemon       { get; } = new();

        // ── Propiedades enlazadas ────────────────────────────────────

        private string _rivalUsername = string.Empty;
        public string RivalUsername
        {
            get => _rivalUsername;
            set { _rivalUsername = value; OnPropertyChanged(); }
        }

        private string _rivalId = string.Empty;
        public string RivalId
        {
            get => _rivalId;
            set { _rivalId = value; OnPropertyChanged(); }
        }

        private TradeModel? _intercambioActivo;
        public TradeModel? IntercambioActivo
        {
            get => _intercambioActivo;
            set { _intercambioActivo = value; OnPropertyChanged(); OnPropertyChanged(nameof(HayIntercambioActivo)); }
        }
        public bool HayIntercambioActivo => IntercambioActivo != null;

        private PokemonUserModel? _pokemonOfrecido;
        public PokemonUserModel? PokemonOfrecido
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

        // ── Constructor ──────────────────────────────────────────────
        public IntercambiosViewModel(ITradeRepository tradeRepo, IPokemonUserRepository pokemonRepo)
        {
            _tradeRepo   = tradeRepo;
            _pokemonRepo = pokemonRepo;
        }

        // ── Comandos / métodos públicos ──────────────────────────────

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
                var msg = await _tradeRepo.SendTradeRequestAsync(RivalId);
                Mensaje = msg != null
                    ? $"Solicitud enviada a {RivalUsername}. Esperando respuesta..."
                    : "Error al enviar la solicitud.";
            }
            catch (Exception ex)
            {
                Mensaje = $"Error: {ex.Message}";
            }
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
                    await CargarIntercambioAsync(tradeId);
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
                if (ok)
                {
                    Mensaje = "Pokémon ofrecido. Esperando al otro jugador...";
                    await RefrescarIntercambioAsync();
                }
                else
                {
                    Mensaje = "Error al ofrecer el Pokémon.";
                }
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
                    await RefrescarIntercambioAsync();
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
            catch (Exception ex) { Mensaje = $"Error: {ex.Message}"; }
            finally { EstaCargando = false; }
        }

        private async Task CargarIntercambioAsync(string tradeId)
        {
            var trade = await _tradeRepo.GetTradeAsync(tradeId);
            IntercambioActivo = trade;
        }

        private async Task RefrescarIntercambioAsync()
        {
            if (IntercambioActivo == null) return;
            IntercambioActivo = await _tradeRepo.GetTradeAsync(IntercambioActivo.Id);
        }

        private async Task CargarMisPokemonAsync()
        {
            var lista = await _pokemonRepo.GetMisPokemonAsync();
            MisPokemon.Clear();
            foreach (var p in lista) MisPokemon.Add(p);
        }

        // ── INotifyPropertyChanged ───────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
