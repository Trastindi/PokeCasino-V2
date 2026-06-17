using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class MisMensajesViewModel
    {
        private readonly MensajeRepository _repo;

        public ObservableCollection<Mensaje> Mensajes { get; set; }

        // Mensaje seleccionado en la lista (binding desde la View)
        public Mensaje MensajeSeleccionado { get; set; }

        // Se dispara cuando el usuario acepta un desafio; lleva el battleId
        public event Action<string> BatallaAceptada;

        public ICommand AceptarDesafioCommand { get; }

        public MisMensajesViewModel()
        {
            _repo = new MensajeRepository();
            Mensajes = new ObservableCollection<Mensaje>();

            AceptarDesafioCommand = new RelayCommand(
                _ => AceptarDesafio(),
                _ => MensajeSeleccionado != null && MensajeSeleccionado.TipoBatallaId != null
            );

            _ = CargarMensajesAsync();
        }

        private void AceptarDesafio()
        {
            if (MensajeSeleccionado?.TipoBatallaId == null)
            {
                MessageBox.Show("Selecciona un mensaje de desafio primero.");
                return;
            }

            // Notifica a la View con el battleId para que abra BattleWindowView
            BatallaAceptada?.Invoke(MensajeSeleccionado.TipoBatallaId);
        }

        private async Task CargarMensajesAsync()
        {
            var lista = await Task.Run(() =>
                _repo.GetMisMensajes()
                    .OrderByDescending(m => m.Fecha)
                    .ToList()
            );

            foreach (var m in lista)
                Mensajes.Add(m);
        }
    }
}
