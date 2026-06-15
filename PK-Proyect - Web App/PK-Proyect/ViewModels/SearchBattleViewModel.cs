using PK_Proyect.Commands;
using PK_Proyect.Services;
using PK_Proyect.Utils;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PK_Proyect.ViewModels
{
    public class SearchBattleViewModel : INotifyPropertyChanged
    {
        private readonly IBattleService _battleService;
        private string _idUserToChallenge;
        private string _idBattleToJoin;
        private bool _isBusy;

        public event PropertyChangedEventHandler PropertyChanged;
        public event System.Action BattleAccepted;

        public RelayCommand DesafiarCommand { get; }
        public RelayCommand UnirseCommand { get; }
        public RelayCommand CancelCommand { get; }

        // Propiedades visuales expuestas desde el VM
        public Brush BackgroundBrush { get; }
        public FontFamily FontFamily { get; }
        public Brush ButtonBackground { get; }
        public Brush ButtonForeground { get; }
        public double ButtonHeight { get; } = 40;
        public double InputHeight { get; } = 36;

        public SearchBattleViewModel(IBattleService battleService, System.Action closeAction)
        {
            _battleService = battleService;

            BackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3C04A"));
            // Asegúrate de que "PokemonClassic" esté registrado en tu proyecto (Resources o sistema)
            FontFamily = new FontFamily("");
            ButtonBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2B2B2B"));
            ButtonForeground = Brushes.White;

            DesafiarCommand = new RelayCommand(async () => await DesafiarAsync(), () => IsValidUserId && !IsBusy);
            UnirseCommand = new RelayCommand(async () => await UnirseAsync(), () => IsValidBattleId && !IsBusy);
            CancelCommand = new RelayCommand(() => closeAction?.Invoke());
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

        public bool IsValidUserId => !string.IsNullOrWhiteSpace(IdUserToChallenge);
        public bool IsValidBattleId => !string.IsNullOrWhiteSpace(IdBattleToJoin);

        private async Task DesafiarAsync()
        {
            IsBusy = true;
            try
            {
                var sent = await _battleService.SendChallengeAsync("currentUserId", IdUserToChallenge);
                if (!sent) return;
                var accepted = await _battleService.WaitForAcceptanceAsync(IdUserToChallenge);
                if (accepted) OnBattleAccepted();
            }
            finally { IsBusy = false; }
        }

        private async Task UnirseAsync()
        {
            IsBusy = true;
            try
            {
                var sent = await _battleService.RequestJoinAsync("currentUserId", IdBattleToJoin);
                if (!sent) return;
                var accepted = await _battleService.WaitForAcceptanceAsync(IdBattleToJoin);
                if (accepted) OnBattleAccepted();
            }
            finally { IsBusy = false; }
        }

        protected void OnBattleAccepted() => BattleAccepted?.Invoke();

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
