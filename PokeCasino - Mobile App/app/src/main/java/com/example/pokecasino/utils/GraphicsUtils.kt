package com.example.pokecasino.utils

import android.graphics.Bitmap

fun splitSprites(
    sheet: Bitmap,
    cols: Int,
    rows: Int
): List<Bitmap> {
    val sprites = mutableListOf<Bitmap>()

    // Tamaño de cada sprite en una rejilla cols x rows
    val spriteWidth = sheet.width / cols
    val spriteHeight = sheet.height / rows

    for (y in 0 until rows) {
        for (x in 0 until cols) {
            val left = x * spriteWidth
            val top = y * spriteHeight

            // Comprobación correcta de límites
            if (left + spriteWidth <= sheet.width && top + spriteHeight <= sheet.height) {
                val sprite = Bitmap.createBitmap(
                    sheet,
                    left,
                    top,
                    spriteWidth,
                    spriteHeight
                )
                sprites.add(sprite)
            }
        }
    }

    return sprites
}