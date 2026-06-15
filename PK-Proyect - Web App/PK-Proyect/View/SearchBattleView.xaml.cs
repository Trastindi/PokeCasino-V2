using System.Windows;
using PK_Proyect.Services;
using PK_Proyect.ViewModels;
using PK_Proyect.Models;

namespace PK_Proyect.View
{
    public partial class SearchBattleView : Window
    {
        private readonly SearchBattleViewModel _vm;

        public SearchBattleView(IBattleService battleService, User currentUser)
        {
            InitializeComponent();
            
            if (currentUser == null)
            {
                MessageBox.Show("Error: No se encontró el usuario actual.");
                this.Close();
                return;
            }

            _vm = new SearchBattleViewModel(battleService, currentUser.Id, Close);
            _vm.BattleAccepted += OnBattleAccepted;
            DataContext = _vm;
        }

        private void OnBattleAccepted()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Usar el nombre correcto de la ventana: BattleWindowView
                var battleWindow = new BattleWindowView(
                    (IBattleService)((SearchBattleViewModel)this.DataContext)
                        .GetType()
                        .GetField("_battleService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.GetValue((SearchBattleViewModel)this.DataContext) 
                    ?? new BattleService(new ApiClient())
                );
                
                battleWindow.Owner = this.Owner;
                battleWindow.Show();
                this.Close();
            });
        }
    }
}
