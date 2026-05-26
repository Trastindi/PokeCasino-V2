using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PK_Proyect.Commands
{
    /// <summary>
    /// RelayCommand que soporta acciones síncronas y asíncronas (Func&lt;object, Task&gt;).
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Func<object, Task> _executeAsync;
        private readonly Func<object, bool> _canExecute;
        private bool _isExecuting;

        // Constructor para acciones asíncronas
        public RelayCommand(Func<object, Task> executeAsync, Func<object, bool> canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute   = canExecute;
        }

        // Constructor de compatibilidad para acciones síncronas existentes (no rompe ningún ViewModel)
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
            : this(p => { execute(p); return Task.CompletedTask; }, canExecute)
        {
        }

        public bool CanExecute(object parameter)
            => !_isExecuting && (_canExecute == null || _canExecute(parameter));

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            _isExecuting = true;
            RaiseCanExecuteChanged();
            try
            {
                await _executeAsync(parameter);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
