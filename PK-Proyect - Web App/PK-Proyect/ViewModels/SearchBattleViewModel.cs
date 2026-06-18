using PK_Proyect.Commands;
using PK_Proyect.Services;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PK_Proyect.ViewModels
{
    public class SearchBattleViewModel : INotifyPropertyChanged
    {
        private readonly IBattleService _battleService;
        private readonly string _currentUserId;
        private string _idUserToChallenge;
        private string _idBattleToJoin;
        private bool _isBusy;
        private string _statusMessage;
        private string _battleId;
        private bool _teamSubmitted;

        public event PropertyChangedEventHandler PropertyChanged;
        // Ahora lleva el battle_id para que el code-behind pueda abrirlo en EquipoPokemon
        public event System.Action<string> BattleAccepted;

        public RelayCommand DesafiarCommand { get; }
        public RelayCommand UnirseCommand   { get; }
        public RelayCommand CancelCommand   { get; }

        // Propiedades visuales
        public Brush BackgroundBrush  { get; }
        public FontFamily FontFamily  { get; }
        public Brush ButtonBackground { get; }
        public Brush ButtonForeground { get; }
        public double ButtonHeight { get; } = 40;
        public double InputHeight  { get; } = 36;

        /// <summary>ID de la batalla activa (asignado tras aceptación).</summary>
        public string BattleId
        {
            get => _battleId;
            private set { _battleId = value; OnPropertyChanged(); }
        }

        /// <summary>True cuando el equipo ya fue enviado al servidor.</summary>
        public bool TeamSubmitted
        {
            get => _teamSubmitted;
            private set { _teamSubmitted = value; OnPropertyChanged(); }
        }

        public SearchBattleViewModel(IBattleService battleService, string currentUserId, System.Action closeAction)
        {
            _battleService = battleService;
            _currentUserId = currentUserId;

            BackgroundBrush  = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3C04A"));
            FontFamily       = new FontFamily("PokemonClassic");
            ButtonBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2B2B2B"));
            ButtonForeground = Brushes.White;

            DesafiarCommand = new RelayCommand(
                async _ => await DesafiarAsync(),
                _       => IsValidUserId && !IsBusy
            );
            UnirseCommand = new RelayCommand(
                async _ => await UnirseAsync(),
                _       => IsValidBattleId && !IsBusy
            );
            CancelCommand = new RelayCommand(_ => { closeAction?.Invoke(); return Task.CompletedTask; });
        }

        public string IdUserToChallenge
        {
            get => _idUserToChallenge;
            set
            {
                if (_idUserToChallenge == value) return;
                _idUserToChallenge = value;
                OnPropertyChanged();
                DesafiarCommand.RaiseCanExecuteChanged();
            }
        }

        public string IdBattleToJoin
        {
            get => _idBattleToJoin;
            set
            {
                if (_idBattleToJoin == value) return;
                _idBattleToJoin = value;
                OnPropertyChanged();
                UnirseCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged();
                DesafiarCommand.RaiseCanExecuteChanged();
                UnirseCommand.RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { if (_statusMessage == value) return; _statusMessage = value; OnPropertyChanged(); }
        }

        public bool IsValidUserId   => !string.IsNullOrWhiteSpace(IdUserToChallenge);
        public bool IsValidBattleId => !string.IsNullOrWhiteSpace(IdBattleToJoin);

        // ── Enviar equipo al servidor ────────────────────────────────────────
        /// <summary>
        /// Llama a POST /battles/{battleId}/teams con el team_id elegido.
        /// Devuelve true si el servidor confirma el envío.
        /// </summary>
        public async Task<bool> SubmitTeamAsync(string teamId)
        {
            if (string.IsNullOrWhiteSpace(BattleId) || string.IsNullOrWhiteSpace(teamId))
                return false;

            try
            {
                var payload = JsonSerializer.Serialize(new { team_id = teamId });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await ApiClient.PostAsync($"/battles/{BattleId}/teams", content);
                if (response == null)
                {
                    MessageBox.Show("Error al enviar el equipo al servidor.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                TeamSubmitted = true;
                StatusMessage = "¡Equipo enviado! Esperando al rival...";
                return true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al enviar equipo: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // ── Flujos de batalla ────────────────────────────────────────────────
        private async Task DesafiarAsync()
        {
            if (string.IsNullOrWhiteSpace(_currentUserId))
            { MessageBox.Show("Error: No se encontró el ID del usuario actual."); return; }

            if (_currentUserId == IdUserToChallenge)
            { MessageBox.Show("No puedes desafiarte a ti mismo."); return; }

            IsBusy = true;
            StatusMessage = "Enviando desafío...";
            try
            {
                var sent = await _battleService.SendChallengeAsync(_currentUserId, IdUserToChallenge);
                if (!sent) { StatusMessage = "Error al enviar desafío."; return; }

                StatusMessage = "Esperando respuesta...";
                var result = await _battleService.WaitForAcceptanceAsync(IdUserToChallenge);
                if (result.Accepted)
                {
                    BattleId      = result.BattleId;
                    StatusMessage = "¡Desafío aceptado!";
                    OnBattleAccepted(BattleId);
                }
                else
                {
                    StatusMessage = "El desafío fue rechazado o expiró.";
                }
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show($"Error durante el desafío: {ex.Message}");
            }
            finally { IsBusy = false; }
        }

        private async Task UnirseAsync()
        {
            if (string.IsNullOrWhiteSpace(_currentUserId))
            { MessageBox.Show("Error: No se encontró el ID del usuario actual."); return; }

            IsBusy = true;
            StatusMessage = "Uniéndose a la batalla...";
            try
            {
                var sent = await _battleService.RequestJoinAsync(_currentUserId, IdBattleToJoin);
                if (!sent) { StatusMessage = "Error al unirse a la batalla."; return; }

                StatusMessage = "Esperando confirmación...";
                var result = await _battleService.WaitForAcceptanceAsync(IdBattleToJoin);
                if (result.Accepted)
                {
                    BattleId      = result.BattleId;
                    StatusMessage = "¡Te has unido a la batalla!";
                    OnBattleAccepted(BattleId);
                }
                else
                {
                    StatusMessage = "La batalla fue cancelada o expiró.";
                }
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show($"Error al unirse: {ex.Message}");
            }
            finally { IsBusy = false; }
        }

        protected void OnBattleAccepted(string battleId) => BattleAccepted?.Invoke(battleId);

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
