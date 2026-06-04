using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PK_Proyect.ViewModels
{
    public class HistoricoTiradasViewModel
    {
        private readonly HistoricoTiradasRepository _repo;
        private readonly string _userId;

        public ObservableCollection<HistoricoTirada> Tiradas { get; set; }

        /// <summary>
        /// El constructor ya NO carga datos: así no bloquea el UI thread.
        /// Llama a CargarAsync() desde el ViewModel que lo instancia.
        /// </summary>
        public HistoricoTiradasViewModel(string userId)
        {
            _repo   = new HistoricoTiradasRepository();
            _userId = userId;
            Tiradas = new ObservableCollection<HistoricoTirada>();
        }

        /// <summary>
        /// Carga el historial de tiradas en un hilo de fondo.
        /// Debe llamarse con await antes de mostrar la ventana.
        /// </summary>
        public async Task CargarAsync()
        {
            var lista = await Task.Run(() => _repo.ObtenerPorUsuario(_userId));

            Tiradas.Clear();
            foreach (var t in lista)
                Tiradas.Add(t);
        }
    }
}
