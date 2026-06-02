using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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

            _ = CargarHistorialAsync(userId);
        }

        private async Task CargarHistorialAsync(string userId)
        {
            try
            {
                Tiradas.Clear();
                var lista = await Task.Run(() => _repo.ObtenerPorUsuario(userId));

                foreach (var t in lista.OrderByDescending(x => x.Fecha))
                    Tiradas.Add(t);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando histórico: " + ex.Message);
            }
        }
    }
}
