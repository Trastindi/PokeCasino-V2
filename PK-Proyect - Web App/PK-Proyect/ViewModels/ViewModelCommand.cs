using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class ViewModelCommand : ICommand
    {

        // Campos
        private readonly Action<object> _executeAction;
        private readonly Predicate<object> _canExecuteAction;


        // Constructor
        public ViewModelCommand(Action<object> executeAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = null;
        }

        public ViewModelCommand(Action<object> executeAction, Predicate<object> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }


        // Eventos
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }


        // Metodos
        public bool CanExecute(object parameter)
        {
            if (_canExecuteAction != null)
            {
                return _canExecuteAction(parameter);
            }

            return true;
                
        }

        public void Execute(object parameter)
        {

           _executeAction(parameter);

        }
    }
}
