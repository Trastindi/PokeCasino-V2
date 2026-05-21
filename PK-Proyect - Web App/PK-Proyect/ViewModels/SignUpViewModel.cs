using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PK_Proyect.Services;
using PK_Proyect.Commands;

namespace PK_Proyect.ViewModels
{
    public class SignupViewModel : INotifyPropertyChanged
    {
        private readonly SignupService _signupService = new();

        // ============================
        // PROPIEDADES DEL JUGADOR
        // ============================

        private string _playerName = "";
        public string PlayerName
        {
            get => _playerName;
            set { _playerName = value; OnPropertyChanged(); }
        }

        private string _rivalName = "";
        public string RivalName
        {
            get => _rivalName;
            set { _rivalName = value; OnPropertyChanged(); }
        }

        private string _gender = "";
        public string Gender
        {
            get => _gender;
            set { _gender = value; OnPropertyChanged(); }
        }

        // ============================
        // ESTADO DEL FLUJO
        // ============================

        private int _question = 0;
        public int Question
        {
            get => _question;
            set { _question = value; OnPropertyChanged(); }
        }

        private int _position = 0;
        public int Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(); }
        }

        private bool _isAsking;
        public bool IsAsking
        {
            get => _isAsking;
            set { _isAsking = value; OnPropertyChanged(); }
        }

        private bool _pendingQuestion;
        public bool PendingQuestion
        {
            get => _pendingQuestion;
            set { _pendingQuestion = value; OnPropertyChanged(); }
        }

        // ============================
        // EVENTO PARA LA VIEW
        // ============================

        public event Action<string> SignupCompleted;

        // ============================
        // COMANDOS
        // ============================

        public ICommand FinishSignupCommand { get; }

        public SignupViewModel()
        {
            FinishSignupCommand = new RelayCommand(FinishSignup);
        }

        // ============================
        // LÓGICA DE REGISTRO
        // ============================

        private void FinishSignup(object obj)
        {
            if (string.IsNullOrWhiteSpace(PlayerName))
            {
                System.Windows.MessageBox.Show("Debes introducir un nombre para tu personaje.");
                return;
            }

            if (string.IsNullOrWhiteSpace(RivalName))
            {
                System.Windows.MessageBox.Show("Debes introducir un nombre para tu rival.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Gender))
            {
                System.Windows.MessageBox.Show("Debes seleccionar un género.");
                return;
            }

            // Guardar en BD
            _signupService.RegisterNewTrainer(PlayerName, Gender);

            // Notificar a la View que debe abrir TrainerDataView
            SignupCompleted?.Invoke(PlayerName);
        }

        // ============================
        // INotifyPropertyChanged
        // ============================

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
