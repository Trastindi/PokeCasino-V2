using System;
using System.Windows.Input;

namespace PK_Proyect.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        // Constructor para comandos CON parámetro
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        // Constructor para comandos SIN parámetro
        public RelayCommand(Action execute)
        {
            _execute = _ => execute();
            _canExecute = null;
        }

        public bool CanExecute(object parameter) =>
            _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) =>
            _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
