using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace PK_Proyect.Utils
{
    /// <summary>
    /// Equivalente a splitSprites() de PokedexV.kt.
    /// Recibe la ruta relativa al spritesheet (dentro de /Images/) y lo corta
    /// en filas × columnas, devolviendo una lista de CroppedBitmap ordenada
    /// de izquierda a derecha, arriba abajo.
    /// </summary>
    public static class SpriteHelper
    {
        /// <param name="relativeImagePath">Ej: "/Images/pokedexicons.png"</param>
        /// <param name="cols">Número de columnas del spritesheet (15 en Pokédex Gen I)</param>
        /// <param name="rows">Número de filas  del spritesheet (11 en Pokédex Gen I)</param>
        public static List<CroppedBitmap> SplitSprites(string relativeImagePath, int cols, int rows)
        {
            // Construir URI relativa al ejecutable (pack URI para WPF)
            var uri = new Uri("pack://application:,,," + relativeImagePath, UriKind.Absolute);
            var source = new BitmapImage(uri);

            int totalW = source.PixelWidth;
            int totalH = source.PixelHeight;

            int cellW = totalW / cols;
            int cellH = totalH / rows;

            var sprites = new List<CroppedBitmap>(cols * rows);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var rect = new System.Windows.Int32Rect(c * cellW, r * cellH, cellW, cellH);
                    sprites.Add(new CroppedBitmap(source, rect));
                }
            }

            return sprites;
        }
    }
}
