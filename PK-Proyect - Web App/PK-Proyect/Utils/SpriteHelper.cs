using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace PK_Proyect.Utils
{
    /// <summary>
    /// Equivalente a splitSprites() de PokedexV.kt.
    /// Carga el spritesheet (15×11 celdas para Pokédex Gen I) y lo corta
    /// devolviendo una lista de CroppedBitmap ordenada de izquierda a derecha, arriba abajo.
    ///
    /// Estrategia de carga (en orden de prioridad):
    ///   1. Pack URI  — solo funciona si el archivo está declarado como Resource en el .csproj
    ///   2. Ruta en disco junto al ejecutable  — funciona si está como Content con CopyToOutputDirectory
    /// </summary>
    public static class SpriteHelper
    {
        /// <param name="relativeImagePath">Ej: "/Images/pokedexicons.png"</param>
        /// <param name="cols">Columnas del spritesheet (15 en Pokédex Gen I)</param>
        /// <param name="rows">Filas del spritesheet (11 en Pokédex Gen I)</param>
        public static List<CroppedBitmap> SplitSprites(string relativeImagePath, int cols, int rows)
        {
            var source = TryLoadBitmap(relativeImagePath)
                         ?? throw new FileNotFoundException(
                             $"No se encontró el spritesheet: {relativeImagePath}");

            int totalW = source.PixelWidth;
            int totalH = source.PixelHeight;
            int cellW  = totalW / cols;
            int cellH  = totalH / rows;

            var sprites = new List<CroppedBitmap>(cols * rows);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    sprites.Add(new CroppedBitmap(source,
                        new System.Windows.Int32Rect(c * cellW, r * cellH, cellW, cellH)));

            return sprites;
        }

        // ── Helpers de carga ──────────────────────────────────────────────────────

        private static BitmapImage? TryLoadBitmap(string relativeImagePath)
        {
            // 1) Pack URI (requiere Build Action = Resource en el .csproj)
            try
            {
                var packUri = new Uri("pack://application:,,," + relativeImagePath, UriKind.Absolute);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource    = packUri;
                bmp.CacheOption  = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch { /* no está embebido como Resource */ }

            // 2) Fallback: buscar el archivo junto al ejecutable
            try
            {
                // relativeImagePath = "/Images/pokedexicons.png"  →  "Images\pokedexicons.png"
                var relativePart = relativeImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var exeDir       = AppDomain.CurrentDomain.BaseDirectory;
                var fullPath     = Path.Combine(exeDir, relativePart);

                if (File.Exists(fullPath))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource   = new Uri(fullPath, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }
            }
            catch { /* error de disco */ }

            return null;
        }
    }
}
