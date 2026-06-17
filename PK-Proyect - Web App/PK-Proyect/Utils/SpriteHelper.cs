using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows;

namespace PK_Proyect.Utils
{
    /// <summary>
    /// Utilidades gráficas para recortar sprites de un spritesheet.
    /// Equivalente a GraphicsUtils.kt (splitSprites) de la app Android.
    /// </summary>
    public static class SpriteHelper
    {
        /// <summary>
        /// Divide un spritesheet en una lista de sprites ordenados de
        /// izquierda a derecha, de arriba a abajo.
        /// </summary>
        /// <param name="sheet">Imagen fuente (cualquier BitmapSource: PNG, BitmapImage, etc.)</param>
        /// <param name="cols">Número de columnas del spritesheet. (ej. 15)</param>
        /// <param name="rows">Número de filas del spritesheet.    (ej. 11)</param>
        /// <returns>
        /// Lista de <see cref="CroppedBitmap"/> con cols×rows entradas.
        /// El sprite número N (base-0) está en el índice N.
        /// Para el pokémon con número de pokedéx P usa el índice (P-1).
        /// </returns>
        public static List<CroppedBitmap> SplitSprites(BitmapSource sheet, int cols, int rows)
        {
            if (sheet == null)      throw new ArgumentNullException(nameof(sheet));
            if (cols <= 0)          throw new ArgumentOutOfRangeException(nameof(cols));
            if (rows <= 0)          throw new ArgumentOutOfRangeException(nameof(rows));

            var sprites     = new List<CroppedBitmap>(cols * rows);
            int spriteWidth = sheet.PixelWidth  / cols;
            int spriteHeight = sheet.PixelHeight / rows;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int left = col * spriteWidth;
                    int top  = row * spriteHeight;

                    // Comprobación de límites (igual que en la versión Android)
                    if (left + spriteWidth  > sheet.PixelWidth)  continue;
                    if (top  + spriteHeight > sheet.PixelHeight) continue;

                    var rect   = new Int32Rect(left, top, spriteWidth, spriteHeight);
                    var sprite = new CroppedBitmap(sheet, rect);
                    sprites.Add(sprite);
                }
            }

            return sprites;
        }

        /// <summary>
        /// Sobrecarga de conveniencia: carga el spritesheet desde una URI
        /// relativa al ensamblado (ej. "/Images/pokedexicons.png").
        /// </summary>
        public static List<CroppedBitmap> SplitSprites(string imageUri, int cols, int rows)
        {
            var bmp = new BitmapImage(new Uri(imageUri, UriKind.RelativeOrAbsolute));
            return SplitSprites(bmp, cols, rows);
        }

        /// <summary>
        /// Devuelve el sprite de un pokémon concreto por su número de pokedéx (base-1).
        /// </summary>
        /// <param name="sprites">Lista generada por <see cref="SplitSprites"/>.</param>
        /// <param name="pokedexNumber">Número de pokedéx (1-165, 1-151, etc.)</param>
        public static CroppedBitmap GetSprite(List<CroppedBitmap> sprites, int pokedexNumber)
        {
            int index = (pokedexNumber - 1).Clamp(0, sprites.Count - 1);
            return sprites[index];
        }
    }

    /// <summary>Extensión auxiliar usada internamente por GetSprite.</summary>
    internal static class IntExtensions
    {
        public static int Clamp(this int value, int min, int max)
            => value < min ? min : value > max ? max : value;
    }
}
