using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PK_Proyect.Models;
using PK_Proyect.Services;

namespace PK_Proyect.View
{
    public partial class SlotMachineView : Window
    {
        private readonly List<ImageSource> SlotMachineSymbols = new();
        private readonly List<ImageSource> SlotMachineNumbers = new();
        private bool textoMostrandose = false;
        DispatcherTimer LoopTimer;
        TimeSpan puntoCorte = TimeSpan.FromSeconds(77);
        TimeSpan margen = TimeSpan.FromMilliseconds(0);
        TimeSpan inicioLoop = TimeSpan.FromSeconds(6);
        MediaElement mediaGlobal;

        private readonly User _user;
        private readonly CasinoService _casinoService;

        private bool confirmando = false;
        private int opcionConfirmacion = 1;

        Dictionary<int, string> SlotMachineIcons = new()
        {
            {0, "Bar"},
            {1, "Meowth"},
            {2, "Koffing"},
            {3, "Arbok"},
            {4, "Cherry"},
            {5, "Seven"}
        };

        private readonly Random rng = new();

        private int[] roll1 = new int[5];
        private int[] roll2 = new int[5];
        private int[] roll3 = new int[5];

        private int[,] tablero = new int[3, 3];

        private DispatcherTimer timer;
        private int estado = 0;

        private bool coinSeleceted = false;
        private int coin = 1;

        private int creditos;
        private int payout = 0;

        public bool pagando = false;

        public SlotMachineView(User user, CasinoService casinoService)
        {
            InitializeComponent();
            this.Closed += SlotMachineView_Closed;

            _user = user;
            _casinoService = casinoService;

            creditos = _user.FichasCasino;

            CrearSlotMachineSymbols();
            CrearSlotMachineNumbers();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            timer.Tick += Timer_Tick;

            for (int i = 0; i < 5; i++)
            {
                roll1[i] = rng.Next(0, SlotMachineSymbols.Count);
                roll2[i] = rng.Next(0, SlotMachineSymbols.Count);
                roll3[i] = rng.Next(0, SlotMachineSymbols.Count);
            }

            DrawRoll1();
            DrawRoll2();
            DrawRoll3();
            DrawCreditNumbers();
            DrawPayOutNumbers();
        }

        private void CrearSlotMachineSymbols()
        {
            SlotMachineSymbols.Clear();

            try
            {
                var ruta = "../../../Images/SlotMachineSymbols.png";
                var spritesheet = new BitmapImage(new Uri(ruta, UriKind.Relative));

                const int filas = 6;
                const int ancho = 79, alto = 79;

                for (int f = 0; f < filas; f++)
                {
                    Int32Rect rect = new(0 * ancho, f * alto, ancho, alto);
                    SlotMachineSymbols.Add(new CroppedBitmap(spritesheet, rect));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando Simbolos del Casino: {ex.Message}");
            }
        }

        private void CrearSlotMachineNumbers()
        {
            SlotMachineNumbers.Clear();

            try
            {
                var ruta = "../../../Images/SlotMachineNumbers.png";
                var spritesheet = new BitmapImage(new Uri(ruta, UriKind.Relative));

                const int filas = 2;
                const int columnas = 5;
                const int ancho = 41, alto = 41;

                for (int f = 0; f < filas; f++)
                {
                    for (int c = 0; c < columnas; c++)
                    {
                        Int32Rect rect = new(c * ancho, f * alto, ancho, alto);
                        SlotMachineNumbers.Add(new CroppedBitmap(spritesheet, rect));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando Numeros del Casino: {ex.Message}");
            }
        }

        private void DrawCreditNumbers()
        {
            int valuec = creditos;

            int c4 = valuec % 10;
            valuec /= 10;
            int c3 = valuec % 10;
            valuec /= 10;
            int c2 = valuec % 10;
            valuec /= 10;
            int c1 = valuec % 10;

            Credit_1.Source = SlotMachineNumbers[c1];
            Credit_2.Source = SlotMachineNumbers[c2];
            Credit_3.Source = SlotMachineNumbers[c3];
            Credit_4.Source = SlotMachineNumbers[c4];
        }

        private void DrawPayOutNumbers()
        {
            int valuep = payout;

            int p4 = valuep % 10;
            valuep /= 10;
            int p3 = valuep % 10;
            valuep /= 10;
            int p2 = valuep % 10;
            valuep /= 10;
            int p1 = valuep % 10;

            PayOut_1.Source = SlotMachineNumbers[p1];
            PayOut_2.Source = SlotMachineNumbers[p2];
            PayOut_3.Source = SlotMachineNumbers[p3];
            PayOut_4.Source = SlotMachineNumbers[p4];
        }

        private async void DrawPayOutToCredit()
        {
            int delay = 50;
            while (payout > 0)
            {
                creditos++;
                _casinoService.ActualizarFichas(_user, creditos);
                payout--;
                DrawCreditNumbers();
                DrawPayOutNumbers();
                await Task.Delay(delay);
            }

            DrawPayOutNumbers();
            pagando = false;
            textoMostrandose = false;
            LblTextContent.Text = "";
            ImgCoinSelector.Visibility = Visibility.Visible;
            Coin_1.Visibility = Visibility.Visible;
            Coin_2.Visibility = Visibility.Collapsed;
            Coin_3.Visibility = Visibility.Collapsed;
        }

        private void UpdateRoll(int[] roll)
        {
            for (int i = 0; i < 4; i++)
                roll[i] = roll[i + 1];

            roll[4] = rng.Next(0, SlotMachineSymbols.Count);
        }

        private void DrawRoll1()
        {
            Roll_1_Icon_1.Source = SlotMachineSymbols[roll1[1]];
            Roll_1_Icon_2.Source = SlotMachineSymbols[roll1[2]];
            Roll_1_Icon_3.Source = SlotMachineSymbols[roll1[3]];
            tablero[0, 0] = roll1[1];
            tablero[0, 1] = roll1[2];
            tablero[0, 2] = roll1[3];
        }

        private void DrawRoll2()
        {
            Roll_2_Icon_1.Source = SlotMachineSymbols[roll2[1]];
            Roll_2_Icon_2.Source = SlotMachineSymbols[roll2[2]];
            Roll_2_Icon_3.Source = SlotMachineSymbols[roll2[3]];
            tablero[1, 0] = roll2[1];
            tablero[1, 1] = roll2[2];
            tablero[1, 2] = roll2[3];
        }

        private void DrawRoll3()
        {
            Roll_3_Icon_1.Source = SlotMachineSymbols[roll3[1]];
            Roll_3_Icon_2.Source = SlotMachineSymbols[roll3[2]];
            Roll_3_Icon_3.Source = SlotMachineSymbols[roll3[3]];
            tablero[2, 0] = roll3[1];
            tablero[2, 1] = roll3[2];
            tablero[2, 2] = roll3[3];
        }

        


private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            if (textoMostrandose) return;

            if (!pagando)
            {
                if (!coinSeleceted)
                {
                    switch (e.Key)
                    {
                        case Key.Up:
                            if (coin < 3)
                            {
                                coin++;

                                if (coin == 2)
                                {
                                    Coin_2.Visibility = Visibility.Visible;
                                    Coin_1.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    Coin_2.Visibility = Visibility.Collapsed;
                                    Coin_3.Visibility = Visibility.Visible;
                                }
                            }
                            break;

                        case Key.Down:
                            if (coin > 1)
                            {
                                coin--;

                                if (coin == 2)
                                {
                                    Coin_2.Visibility = Visibility.Visible;
                                    Coin_3.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    Coin_2.Visibility = Visibility.Collapsed;
                                    Coin_1.Visibility = Visibility.Visible;
                                }
                            }
                            break;

                        case Key.Enter:
                            coinSeleceted = true;
                            ImgCoinSelector.Visibility = Visibility.Collapsed;
                            Coin_1.Visibility = Visibility.Collapsed;
                            Coin_2.Visibility = Visibility.Collapsed;
                            Coin_3.Visibility = Visibility.Collapsed;
                            creditos -= coin;
                            _casinoService.ActualizarFichas(_user, creditos);

                            if (creditos <= 0)
                            {
                                //TO DO ventana recargar
                                creditos = 3;
                            }
                            else
                            {
                                HandleEnter();
                                DrawCreditNumbers();
                            }
                            break;
                    }
                }
                else
                {
                    HandleEnter();
                }
            }
        }

        private void HandleEnter()
        {
            switch (estado)
            {
                case 0:
                    LblTextContent.Text = "";
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
                UpdateRoll(roll1);
                DrawRoll1();
            }
            if (estado <= 2)
            {
                UpdateRoll(roll2);
                DrawRoll2();
            }
            if (estado <= 3)
            {
                UpdateRoll(roll3);
                DrawRoll3();
            }
        }

        private async void ComprobarGanar()
        {
            var lineasGanadoras = new List<string>(); // símbolo de cada línea ganadora
            int payoutTotal = 0;

            // horizontales
            bool arriba = tablero[0, 0] == tablero[1, 0] && tablero[0, 0] == tablero[2, 0];
            bool centro = tablero[0, 1] == tablero[1, 1] && tablero[0, 1] == tablero[2, 1];
            bool abajo = tablero[0, 2] == tablero[1, 2] && tablero[0, 2] == tablero[2, 2];

            // diagonales
            bool diag1 = tablero[0, 0] == tablero[1, 1] && tablero[0, 0] == tablero[2, 2];
            bool diag2 = tablero[0, 2] == tablero[1, 1] && tablero[0, 2] == tablero[2, 0];

            // según coin, qué líneas están activas
            if (coin >= 1 && centro)
                lineasGanadoras.Add(SlotMachineIcons[tablero[1, 1]]);
            if (coin == 3 && arriba)
                lineasGanadoras.Add(SlotMachineIcons[tablero[1, 0]]);
            if (coin == 3 && abajo)
                lineasGanadoras.Add(SlotMachineIcons[tablero[1, 2]]);
            if (coin >= 2 && diag1)
                lineasGanadoras.Add(SlotMachineIcons[tablero[1, 1]]);
            if (coin >= 2 && diag2)
                lineasGanadoras.Add(SlotMachineIcons[tablero[1, 1]]);

            // calcular payout total
            foreach (var premio in lineasGanadoras)
                payoutTotal += GetPayoutForSymbol(premio);

            if (lineasGanadoras.Count == 0)
            {
                // sin premio
                payout = 0;
                DrawPayOutNumbers();
                
                ImgCoinSelector.Visibility = Visibility.Visible;
                Coin_1.Visibility = Visibility.Visible;
                Coin_2.Visibility = Visibility.Collapsed;
                Coin_3.Visibility = Visibility.Collapsed;
            }
            else
            {
                payout += payoutTotal;
                DrawPayOutNumbers();

                string mensaje = CrearMensajeVictoria(lineasGanadoras, payoutTotal);
                await ResultadoTexto(mensaje);

                pagando = true;
                DrawPayOutToCredit();
                
            }

            // reset selección
            
            coinSeleceted = false;
            coin = 1;
            

        }

        private int GetPayoutForSymbol(string premio)
        {
            return premio switch
            {
                "Seven" => 300,
                "Bar" => 100,
                "Meowth" => 15,
                "Koffing" => 15,
                "Arbok" => 15,
                "Cherry" => 8,
                _ => 0
            };
        }

        private string CrearMensajeVictoria(List<string> lineasGanadoras, int payoutTotal)
        {
            // comprobar jackpot: 9 símbolos iguales
            bool jackpot = EsJackpot();
            if (jackpot)
                return $"Jackpot, has ganado {payoutTotal} fichas.";

            // construir lista de iconos (p.ej. \"Seven, Cherry\")
            string iconos = string.Join(", ", lineasGanadoras.Distinct());

            return lineasGanadoras.Count switch
            {
                1 => $"Premio por línea simple con {iconos}, has ganado {payoutTotal} fichas.",
                2 => $"Premio por doble línea con {iconos}, has ganado {payoutTotal} fichas.",
                3 => $"Premio por triple línea con {iconos}, has ganado {payoutTotal} fichas.",
                _ => $"Premio con {lineasGanadoras.Count} líneas, has ganado {payoutTotal} fichas."
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



        private async Task ResultadoTexto(string texto)
        {
            textoMostrandose = true;
            LblTextContent.Text = "";
            
            foreach (char c in texto)
            {
                LblTextContent.Text += c;
                await Task.Delay(50);
            }
        }


        private void BackgroundMusic_MediaEnded(object sender, RoutedEventArgs e)
        {
            var media = sender as MediaElement;

            media.Position = TimeSpan.Zero; // reinicia desde el inicio
            media.Play();

        }


        private void BackgroundMusic_Loaded(object sender, RoutedEventArgs e)
        {
            var media = sender as MediaElement;

            // Dejamos que arranque automáticamente (LoadedBehavior="Play")
            // y luego lo pasamos a Manual en cuanto el Dispatcher lo permita
            Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new System.Windows.Threading.DispatcherOperationCallback(CambiarAManual),
                media
            );
        }

        private object CambiarAManual(object arg)
        {
            var media = arg as MediaElement;

            media.LoadedBehavior = MediaState.Manual;
            FinalizarSegundosAntes(media);

            return null;
        }

        private void BackgroundMusic_MediaOpened(object sender, RoutedEventArgs e)
        {
           
            BackgroundMusic.Play();

            // Activamos el sistema de corte anticipado
            FinalizarSegundosAntes(BackgroundMusic);
        }



        private void LoopTimer_Tick(object sender, EventArgs e)
        {
            if (mediaGlobal == null)
                return;


            if (!mediaGlobal.NaturalDuration.HasTimeSpan) return;

            Debug.WriteLine(mediaGlobal.Position);

            // Si estamos a menos de 8 segundos del final, reiniciar
            if (mediaGlobal.Position >= puntoCorte - margen) { 
                
                mediaGlobal.Position = inicioLoop; 
                mediaGlobal.Play(); 
            }

            

        }

        private void FinalizarSegundosAntes(MediaElement media)
        {
            mediaGlobal = media;

            LoopTimer = new DispatcherTimer();
            LoopTimer.Interval = TimeSpan.FromSeconds(2);
            LoopTimer.Tick += LoopTimer_Tick;
            LoopTimer.Start();
        }


        private void SlotMachineView_Closed(object sender, EventArgs e)
        {
            try
            {
                // Detener música
                BackgroundMusic.Stop();
                BackgroundMusic.Close();

                // Detener loop manual
                if (LoopTimer != null)
                    LoopTimer.Stop();
            }
            catch { }
        }



    }
}
