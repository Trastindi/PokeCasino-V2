using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PK_Proyect.ViewModels
{
    public class SlotMachineViewModel : INotifyPropertyChanged
    {




        // =======================
        //  EVENTOS PARA LA VIEW
        // =======================
        public event Action<int[]> Roll1Updated;
        public event Action<int[]> Roll2Updated;
        public event Action<int[]> Roll3Updated;

        public event Action<int> CreditUpdated;
        public event Action<int> PayoutUpdated;

        public event Action<string> MensajeMostrado;
        public event Action PayoutAnimacionTerminada;

        // =======================
        //  CAMPOS INTERNOS
        // =======================
        private readonly Random rng = new();
        private DispatcherTimer timer;

        public int[] Roll1 { get; private set; } = new int[5];
        public int[] Roll2 { get; private set; } = new int[5];
        public int[] Roll3 { get; private set; } = new int[5];

        private int[,] tablero = new int[3, 3];

        private int _creditos = 50;
        public int Creditos
        {
            get => _creditos;
            set { _creditos = value; OnPropertyChanged(); CreditUpdated?.Invoke(value); }
        }

        private int _payout = 0;
        public int Payout
        {
            get => _payout;
            set { _payout = value; OnPropertyChanged(); PayoutUpdated?.Invoke(value); }
        }

        public int Coin { get; set; } = 1;
        public bool CoinSelected { get; set; } = false;
        public bool Pagando { get; set; } = false;

        private int estado = 0;

        // =======================
        //  CONSTRUCTOR
        // =======================
        public SlotMachineViewModel()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            timer.Tick += Timer_Tick;

            // Inicializar rodillos
            for (int i = 0; i < 5; i++)
            {
                Roll1[i] = rng.Next(0, 6);
                Roll2[i] = rng.Next(0, 6);
                Roll3[i] = rng.Next(0, 6);
            }

            Roll1Updated?.Invoke(Roll1);
            Roll2Updated?.Invoke(Roll2);
            Roll3Updated?.Invoke(Roll3);
        }

        // =======================
        //  MÉTODOS PRINCIPALES
        // =======================

        public void HandleEnter()
        {
            switch (estado)
            {
                case 0:
                    estado = 1;
                    timer.Start();
                    break;

                case 1:
                    estado = 2;
                    break;

                case 2:
                    estado = 3;
                    break;

                case 3:
                    timer.Stop();
                    estado = 0;
                    ComprobarGanar();
                    break;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (estado <= 1)
            {
                UpdateRoll(Roll1);
                Roll1Updated?.Invoke(Roll1);
            }
            if (estado <= 2)
            {
                UpdateRoll(Roll2);
                Roll2Updated?.Invoke(Roll2);
            }
            if (estado <= 3)
            {
                UpdateRoll(Roll3);
                Roll3Updated?.Invoke(Roll3);
            }
        }

        private void UpdateRoll(int[] roll)
        {
            for (int i = 0; i < 4; i++)
                roll[i] = roll[i + 1];

            roll[4] = rng.Next(0, 6);
        }

        // =======================
        //  COMPROBAR GANAR
        // =======================
        private async void ComprobarGanar()
        {
            var lineasGanadoras = new List<int>();
            int payoutTotal = 0;

            // Actualizar tablero
            tablero[0, 0] = Roll1[1];
            tablero[0, 1] = Roll1[2];
            tablero[0, 2] = Roll1[3];

            tablero[1, 0] = Roll2[1];
            tablero[1, 1] = Roll2[2];
            tablero[1, 2] = Roll2[3];

            tablero[2, 0] = Roll3[1];
            tablero[2, 1] = Roll3[2];
            tablero[2, 2] = Roll3[3];

            // Líneas horizontales
            bool arriba = tablero[0, 0] == tablero[1, 0] && tablero[0, 0] == tablero[2, 0];
            bool centro = tablero[0, 1] == tablero[1, 1] && tablero[0, 1] == tablero[2, 1];
            bool abajo = tablero[0, 2] == tablero[1, 2] && tablero[0, 2] == tablero[2, 2];

            // Diagonales
            bool diag1 = tablero[0, 0] == tablero[1, 1] && tablero[0, 0] == tablero[2, 2];
            bool diag2 = tablero[0, 2] == tablero[1, 1] && tablero[0, 2] == tablero[2, 0];

            if (Coin >= 1 && centro) lineasGanadoras.Add(tablero[1, 1]);
            if (Coin == 3 && arriba) lineasGanadoras.Add(tablero[1, 0]);
            if (Coin == 3 && abajo) lineasGanadoras.Add(tablero[1, 2]);
            if (Coin >= 2 && diag1) lineasGanadoras.Add(tablero[1, 1]);
            if (Coin >= 2 && diag2) lineasGanadoras.Add(tablero[1, 1]);

            foreach (var simbolo in lineasGanadoras)
                payoutTotal += GetPayout(simbolo);

            if (lineasGanadoras.Count == 0)
            {
                Payout = 0;
                CoinSelected = false;
                Coin = 1;
                return;
            }

            Payout += payoutTotal;

            string mensaje = CrearMensaje(lineasGanadoras, payoutTotal);
            MensajeMostrado?.Invoke(mensaje);

            Pagando = true;
            await PagarAnimacion();
        }

        private int GetPayout(int simbolo)
        {
            return simbolo switch
            {
                5 => 300, // Seven
                0 => 100, // Bar
                1 => 15,  // Meowth
                2 => 15,  // Koffing
                3 => 15,  // Arbok
                4 => 8,   // Cherry
                _ => 0
            };
        }

        private string CrearMensaje(List<int> lineas, int payout)
        {
            if (EsJackpot())
                return $"Jackpot, has ganado {payout} fichas.";

            return lineas.Count switch
            {
                1 => $"Premio por línea simple, has ganado {payout} fichas.",
                2 => $"Premio por doble línea, has ganado {payout} fichas.",
                3 => $"Premio por triple línea, has ganado {payout} fichas.",
                _ => $"Premio con {lineas.Count} líneas, has ganado {payout} fichas."
            };
        }

        private bool EsJackpot()
        {
            int primero = tablero[0, 0];
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    if (tablero[x, y] != primero)
                        return false;
            return true;
        }

        private async Task PagarAnimacion()
        {
            while (Payout > 0)
            {
                Creditos++;
                Payout--;
                await Task.Delay(50);
            }

            Pagando = false;
            CoinSelected = false;
            Coin = 1;

            PayoutAnimacionTerminada?.Invoke();
        }

        // =======================
        //  INotifyPropertyChanged
        // =======================
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
