using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PK_Proyect.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifica un cambio de propiedad.
        /// Con [CallerMemberName] no hace falta pasar el nombre explícitamente
        /// desde los setters (OnPropertyChanged() funciona sin argumento).
        /// Los callers que ya pasaban nameof(...) siguen funcionando igual.
        /// </summary>
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
