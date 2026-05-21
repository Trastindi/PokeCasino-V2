using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.ObjectModel;
using System.Windows;

namespace PK_Proyect.ViewModels
{
    public class HistoricoTiradasViewModel
    {
        private readonly HistoricoTiradasRepository _repo;

        public ObservableCollection<HistoricoTirada> Tiradas { get; set; }

        public HistoricoTiradasViewModel(string userId)
        {
            _repo = new HistoricoTiradasRepository();
            Tiradas = new ObservableCollection<HistoricoTirada>();

            CargarHistorial(userId);
        }

        private void CargarHistorial(string userId)
        {
            Tiradas.Clear();

            var lista = _repo.ObtenerPorUsuario(userId);

            MessageBox.Show("Tiradas encontradas: " + lista.Count);

            foreach (var t in lista)
                Tiradas.Add(t);
        }

    }
}
