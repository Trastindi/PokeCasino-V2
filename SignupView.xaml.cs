using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PK_Proyect.View
{
    public partial class SignupView : Window
    {
        // --- Datos de Oak / Intro ---
        private readonly List<ImageSource> Oak = new();
        private readonly List<int> Valores = new();

        private readonly string WelcomeMessage =
            "¡Hola! ¡Qué gusto conocerte!#" +
            "¡Bienvenido al mundo Pokémon!#" +
            "Me llamo Oak.\nLa gente me conoce cariñosamente\ncomo el Profesor Pokémon.#" +
            "Este mundo…\nestá habitado por criaturas\nllamadas Pokémon.#" +
            "Para algunas personas,\nlos Pokémon son mascotas.#" +
            "Otros los usan para combatir.#" +
            "Yo...\nlos estudio como profesión.#" +
            "Pero primero,\ncuéntame algo sobre ti.#" +
            "Dime.\n¿Eres chico o chica?&" +
            "Empecemos con tu nombre.\n¿Cómo te llamas?&" +
            "Bien…\nAsí que tu nombre es <player>.#" +
            "Este es mi nieto.\nHa sido tu rival desde\nque eran bebés.#" +
            "...Ehm,\n¿cómo se llamaba ahora?&" +
            "...Ah, sí, ¡<rival>!\n¡Ya me acordé!\n¡Ese es su nombre!#" +
            "¡<player>!\n¡Tu propia leyenda Pokémon\nestá a punto de comenzar!#" +
            "¡Un mundo de sueños y aventuras\ncon Pokémon te espera!#" +
            "¡Vamos allá!%";

        private string letra = "";
        private int position = 0;
        private int question = 0;
        private bool animando = false;
        private bool OakFin = false;
        private bool pregunta = false;
        private bool preguntaPendiente = false;

        // --- Selector de nombre ---
        private readonly string alphabet = "abcdefghijklmnopqrstuvwxyz ";
        private int letraAlphabet = 0;          // índice en alphabet
        private int letraNombre = 0;          // índice en los nombres
        private string playerName = "";
        private string rivalName = "";
        private bool upperCase = true;
        private bool player = true;           // true = nombre jugador, false = rival

        private Image LetterArrow;

        public SignupView()
        {
            InitializeComponent();
            CrearOak();
            CargarOak();
        }

        // ================== OAK / INTRO ==================

        private void CrearOak()
        {
            Oak.Clear();
            Valores.Clear();

            try
            {
                var ruta = "../../../Images/Oak.png";
                var spritesheet = new BitmapImage(new Uri(ruta, UriKind.Relative));

                const int columnas = 8;
                const int ancho = 56, alto = 56;

                for (int c = 0; c < columnas; c++)
                {
                    Int32Rect rect = new(c * ancho, 0 * alto, ancho, alto);
                    Oak.Add(new CroppedBitmap(spritesheet, rect));
                    Valores.Add(c + 1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando Oak: {ex.Message}");
            }
        }

        private async void CargarOak()
        {
            if (!OakFin)
            {
                // Intro: frames 0 a 5
                for (int i = 0; i < 6; i++)
                {
                    imgOak.Source = Oak[i];
                    await Task.Delay(500);
                }

                MostrarBienvenida();
            }
            else
            {
                // Despedida: frames 6 y 7
                for (int i = 6; i < 8; i++)
                {
                    imgOak.Source = Oak[i];
                    await Task.Delay(500);
                }
                imgOak.Source = null;
                await Task.Delay(500);
                var TrainerDataView = new TrainerDataView(playerName);
                TrainerDataView.Show();
                Close();
            }
        }

        private async void MostrarBienvenida()
        {
            if (animando) return;
            animando = true;

            LblPokemonText.Content = "";

            while (position < WelcomeMessage.Length)
            {
                letra = WelcomeMessage[position].ToString();

                // Insertar nombres <player> / <rival>
                if (letra == "<")
                {
                    if (WelcomeMessage[position + 1] == 'p')
                    {
                        foreach (char ch in playerName)
                        {
                            await Task.Delay(ch is '.' or '!' or '?' ? 300 : 20);
                            LblPokemonText.Content += ch.ToString();
                        }
                        position += "<player>".Length;
                    }
                    else
                    {
                        foreach (char ch in rivalName)
                        {
                            await Task.Delay(ch is '.' or '!' or '?' ? 300 : 20);
                            LblPokemonText.Content += ch.ToString();
                        }
                        position += "<rival>".Length;
                    }
                    continue;
                }

                // Marcadores de pregunta
                if (letra == "&")
                {
                    switch (question)
                    {
                        case 0:
                            // Elegir género – se entra a pregunta inmediatamente
                            pregunta = true;
                            LblGenderDialog.Visibility = Visibility.Visible;
                            BoyArrow.Visibility = Visibility.Visible;
                            GirlArrow.Visibility = Visibility.Hidden;
                            break;

                        case 1:
                        case 2:
                            // Para nombres, solo marcamos que hay pregunta pendiente
                            preguntaPendiente = true;
                            break;
                    }
                }

                if (letra == "#" || letra == "%" || letra == "&")
                    break;

                await Task.Delay(letra is "." or "!" or "?" ? 300 : 20);
                await Task.Delay(letra is "." or "!" or "?" ? 300 : 20);
                LblPokemonText.Content += letra;
                position++;
            }

            animando = false;
        }

        private bool MostrandoFraseTerminada()
        {
            return letra == "#" || letra == "&" || letra == "%";
        }

        private void ExtraerFraseCompleta()
        {
            if (animando) return;
            animando = true;

            while (position < WelcomeMessage.Length)
            {
                letra = WelcomeMessage[position].ToString();

                if (letra == "#" || letra == "&" || letra == "%")
                    break;

                LblPokemonText.Content += letra;
                position++;
            }

            animando = false;
        }

        private void SiguienteFrase()
        {
            if (position >= WelcomeMessage.Length || letra == "%")
            {
                CerrarVentana();
                return;
            }

            position++;  // saltar marcador (#, &, %)
            MostrarBienvenida();
        }

        // ================== INPUT TECLADO GLOBAL ==================

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!pregunta)
            {
                if (e.Key == Key.Enter || e.Key == Key.Space)
                {
                    e.Handled = true;

                    if (position >= WelcomeMessage.Length || letra == "%")
                    {
                        CerrarVentana();
                        return;
                    }

                    // Si aún se está escribiendo la frase, completarla
                    if (position < WelcomeMessage.Length && !MostrandoFraseTerminada())
                    {
                        ExtraerFraseCompleta();
                        return;
                    }

                    // Si la frase ya está completa y hay una pregunta pendiente (nombre)
                    if (preguntaPendiente)
                    {
                        ActivarPreguntaNombre();
                        return;
                    }

                    // Si no hay pregunta, pasar a la siguiente frase
                    SiguienteFrase();
                }
            }
            else
            {
                // Estamos en modo pregunta
                switch (question)
                {
                    case 0:
                        ElegirGenero(e);
                        break;
                    case 1:
                    case 2:
                        ElegirNombre(e);
                        break;
                }
            }
        }

        // ================== PREGUNTA GÉNERO ==================

        private void ElegirGenero(KeyEventArgs e)
        {
            if (e.Key == Key.Up && BoyArrow.Visibility == Visibility.Hidden)
            {
                BoyArrow.Visibility = Visibility.Visible;
                GirlArrow.Visibility = Visibility.Hidden;
            }

            if (e.Key == Key.Down && GirlArrow.Visibility == Visibility.Hidden)
            {
                BoyArrow.Visibility = Visibility.Hidden;
                GirlArrow.Visibility = Visibility.Visible;
            }

            if (e.Key == Key.Enter)
            {
                // Podrías guardar aquí el género según qué flecha está visible

                BoyArrow.Visibility = Visibility.Hidden;
                GirlArrow.Visibility = Visibility.Hidden;
                LblGenderDialog.Visibility = Visibility.Hidden;

                pregunta = false;
                question++;   // siguiente pregunta (nombre jugador)
                position++;   // saltar '&'
                MostrarBienvenida();
            }
        }

        // ================== ACTIVAR PREGUNTA NOMBRE ==================

        private void ActivarPreguntaNombre()
        {
            preguntaPendiente = false;
            pregunta = true;

            WpAlphabet.Visibility = Visibility.Visible;
            WpName.Visibility = Visibility.Visible;
            LblUpperCase.Visibility = Visibility.Visible;

            imgOak.Visibility = Visibility.Hidden;
            LblPokemonText.Visibility = Visibility.Hidden;

            // Empezar siempre en la primera letra
            letraAlphabet = 0;
            letraNombre = 0;
            CargarAlphabet();
            CargarLetras();
        }

        // ================== SELECTOR LETRAS (ALFABETO) ==================

        private void CargarAlphabet()
        {
            WrapPanel alphabetPanel = WpAlphabet;
            WpAlphabet.Children.Clear();
            for (int i = 0; i < alphabet.Length; i++)
            {
                if (i == letraAlphabet)
                {
                    LetterArrow = new Image
                    {
                        Source = new BitmapImage(new Uri("../../../Images/arrow.png", UriKind.Relative)),
                        Stretch = Stretch.Fill,
                        RenderTransformOrigin = new Point(-2, 0.25),
                        Height = 47,
                        Width = 43,
                        Margin = new Thickness(0, 50, 0, 0)
                    };

                    var brush = new ImageBrush
                    {
                        ImageSource = new BitmapImage(new Uri("../../../Images/arrow.png", UriKind.Relative)),
                        Stretch = Stretch.Fill
                    };
                    LetterArrow.OpacityMask = brush;

                    WpAlphabet.Children.Add(LetterArrow);
                }
                // Crear un NUEVO Label por cada letra
                Label letraAbecedario = new Label();

                letraAbecedario.Height = 70;
                letraAbecedario.Width = 50;
                letraAbecedario.FontSize = 50;
                letraAbecedario.Padding = new Thickness(0, 0, 0, 0);
                letraAbecedario.Margin = new Thickness(47, 15, 0, 0);

                if (i == letraAlphabet)
                    letraAbecedario.Margin = new Thickness(0, 0, 0, 0);

                // Usa la misma FontFamily que en XAML
                //letraAbecedario.FontFamily = new FontFamily("./Fonts/#Pokemon Classic Regular");
                // O si la tienes como recurso:
                letraAbecedario.FontFamily = (FontFamily)FindResource("PokemonClassic");

                letraAbecedario.HorizontalContentAlignment = HorizontalAlignment.Center;
                letraAbecedario.VerticalContentAlignment = VerticalAlignment.Center;

                if (upperCase)
                    letraAbecedario.Content = alphabet[i].ToString().ToUpper();
                else
                    letraAbecedario.Content = alphabet[i].ToString();

                alphabetPanel.Children.Add(letraAbecedario);

            }
        }

        private void CargarLetras()
        {
            WrapPanel namePanel = WpName;
            WpName.Children.Clear();
            for (int i = 0; i < 8; i++)
            {
                // Crear un NUEVO Label por cada letra
                Label letraName = new Label();

                letraName.Height = 111;
                letraName.Width = 73;
                letraName.FontSize = 64;
                letraName.Padding = new Thickness(14, 5, 5, 5);
                letraName.Margin = new Thickness(21, 0, 21, 0);
                letraName.HorizontalAlignment = HorizontalAlignment.Center;
                letraName.VerticalAlignment = VerticalAlignment.Center;
                // Usa la misma FontFamily que en XAML
                //letraAbecedario.FontFamily = new FontFamily("./Fonts/#Pokemon Classic Regular");
                // O si la tienes como recurso:
                letraName.FontFamily = (FontFamily)FindResource("PokemonClassic");

                letraName.HorizontalContentAlignment = HorizontalAlignment.Center;
                letraName.VerticalContentAlignment = VerticalAlignment.Center;

                if (player)
                {
                    if (i < playerName.Length)
                    {
                        letraName.Content += playerName[i].ToString();
                    }
                    else if (i == letraNombre)
                    {
                        letraName.Content = "-";
                    }
                    else
                    {
                        letraName.Content = "_";
                    }
                }
                else
                {
                    if (i < rivalName.Length)
                    {
                        letraName.Content += rivalName[i].ToString();
                    }
                    else if (i == letraNombre)
                    {
                        letraName.Content = "-";
                    }
                    else
                    {
                        letraName.Content = "_";
                    }
                }

                namePanel.Children.Add(letraName);
            }
        }

        private void ElegirNombre(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                    if (letraAlphabet < alphabet.Length - 1)
                        letraAlphabet++;
                    break;

                case Key.Left:
                    if (letraAlphabet > 0)
                        letraAlphabet--;
                    break;

                case Key.Up:
                    // subir 9 posiciones (simulando filas de 9 letras)
                    if (letraAlphabet - 9 >= 0)
                        letraAlphabet -= 9;
                    break;

                case Key.Down:
                    if (letraAlphabet + 9 < alphabet.Length)
                        letraAlphabet += 9;
                    break;

                case Key.Enter:
                    // Añadir la letra actual
                    char ch = alphabet[letraAlphabet];
                    string letraFinal = upperCase ? ch.ToString().ToUpper() : ch.ToString();

                    if (player)
                    {
                        playerName += letraFinal;
                        letraNombre++;
                        CargarLetras();
                    }
                    else
                    {
                        rivalName += letraFinal;
                        letraNombre++;
                        CargarLetras();
                    }

                    // Aquí puedes ir mostrando el nombre en algún Label:
                    // LblPlayerName.Content = player ? playerName : LblPlayerName.Content;
                    // LblRivalName.Content = !player ? rivalName : LblRivalName.Content;
                    break;

                case Key.Back:
                    if (player && playerName.Length > 0)
                    {
                        playerName = playerName[..^1];
                        letraNombre--;
                        CargarLetras();
                    }
                    else if (!player && rivalName.Length > 0)
                    {
                        rivalName = rivalName[..^1];
                        letraNombre--;
                        CargarLetras();
                    }
                    break;

                case Key.Tab:
                    // Usar Tab para confirmar nombre
                    if (player && playerName.Length > 0)
                    {
                        player = false;
                        pregunta = false;
                        question++;   // pasa a nombre rival
                        position++;   // salta '&'

                        WpAlphabet.Visibility = Visibility.Hidden;
                        WpName.Visibility = Visibility.Hidden;
                        LblUpperCase.Visibility = Visibility.Hidden;
                        LetterArrow.Visibility = Visibility.Hidden;
                        imgOak.Visibility = Visibility.Visible;
                        LblPokemonText.Visibility = Visibility.Visible;

                        MostrarBienvenida();
                    }
                    else if (!player && rivalName.Length > 0)
                    {
                        pregunta = false;
                        question++;   // sigue historia
                        position++;   // salta '&'

                        WpAlphabet.Visibility = Visibility.Collapsed;
                        LblUpperCase.Visibility = Visibility.Collapsed;
                        LetterArrow.Visibility = Visibility.Collapsed;
                        WpName.Visibility = Visibility.Collapsed;
                        imgOak.Visibility = Visibility.Visible;
                        LblPokemonText.Visibility = Visibility.Visible;

                        MostrarBienvenida();
                    }
                    break;

                case Key.LeftCtrl:
                    // Toggle mayúsculas/minúsculas
                    upperCase = !upperCase;
                    LblUpperCase.Content = upperCase ? "UPPER CASE" : "LOWER CASE";
                    break;
            }

            CargarAlphabet();
            CargarLetras();
        }

        // ================== CIERRE ==================

        private void CerrarVentana()
        {
            OakFin = true;
            CargarOak();
        }

        private void BackgroundMusic_MediaEnded(object sender, RoutedEventArgs e)
        {
            var media = sender as MediaElement;

            media.Position = TimeSpan.Zero; // reinicia desde el inicio
            media.Play();

        }
    }
}