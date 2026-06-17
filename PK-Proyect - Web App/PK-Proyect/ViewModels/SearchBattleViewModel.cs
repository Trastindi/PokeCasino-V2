using PK_Proyect.Commands;
using PK_Proyect.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

        public event PropertyChangedEventHandler PropertyChanged;
        public event System.Action BattleAccepted;

        // Commands — RelayCommand(Func<object,Task>, Func<object,bool>)
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

        public SearchBattleViewModel(IBattleService battleService, string currentUserId, System.Action closeAction)
        {
            _battleService = battleService;
            _currentUserId = currentUserId;

            BackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3C04A"));
            FontFamily      = new FontFamily("PokemonClassic");
            ButtonBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2B2B2B"));
            ButtonForeground = Brushes.White;

            // Lambdas con parámetro _ para Func<object,Task> y Func<object,bool>
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

        public bool IsValidUserId  => !string.IsNullOrWhiteSpace(IdUserToChallenge);
        public bool IsValidBattleId => !string.IsNullOrWhiteSpace(IdBattleToJoin);

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
                var accepted = await _battleService.WaitForAcceptanceAsync(IdUserToChallenge);
                StatusMessage = accepted ? "¡Desafío aceptado!" : "El desafío fue rechazado o expiró.";
                if (accepted) OnBattleAccepted();
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
                var accepted = await _battleService.WaitForAcceptanceAsync(IdBattleToJoin);
                StatusMessage = accepted ? "¡Te has unido a la batalla!" : "La batalla fue cancelada o expiró.";
                if (accepted) OnBattleAccepted();
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show($"Error al unirse: {ex.Message}");
            }
            finally { IsBusy = false; }
        }

        protected void OnBattleAccepted() => BattleAccepted?.Invoke();

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
