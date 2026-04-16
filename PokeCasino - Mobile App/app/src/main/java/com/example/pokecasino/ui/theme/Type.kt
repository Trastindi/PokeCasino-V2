// Type.kt
package com.example.pokecasino.ui.theme

import androidx.compose.material3.Typography
import androidx.compose.ui.text.font.Font
import androidx.compose.ui.text.font.FontFamily
import com.example.pokecasino.R

val PokemonClassicFont = FontFamily(
    Font(R.font.pokemon_classic)
)

// Tipografía base de Material3
private val DefaultTypography = Typography()

val AppTypography = Typography(
    displayLarge = DefaultTypography.displayLarge.copy(fontFamily = PokemonClassicFont),
    displayMedium = DefaultTypography.displayMedium.copy(fontFamily = PokemonClassicFont),
    displaySmall = DefaultTypography.displaySmall.copy(fontFamily = PokemonClassicFont),

    headlineLarge = DefaultTypography.headlineLarge.copy(fontFamily = PokemonClassicFont),
    headlineMedium = DefaultTypography.headlineMedium.copy(fontFamily = PokemonClassicFont),
    headlineSmall = DefaultTypography.headlineSmall.copy(fontFamily = PokemonClassicFont),

    titleLarge = DefaultTypography.titleLarge.copy(fontFamily = PokemonClassicFont),
    titleMedium = DefaultTypography.titleMedium.copy(fontFamily = PokemonClassicFont),
    titleSmall = DefaultTypography.titleSmall.copy(fontFamily = PokemonClassicFont),

    bodyLarge = DefaultTypography.bodyLarge.copy(fontFamily = PokemonClassicFont),
    bodyMedium = DefaultTypography.bodyMedium.copy(fontFamily = PokemonClassicFont),
    bodySmall = DefaultTypography.bodySmall.copy(fontFamily = PokemonClassicFont),

    labelLarge = DefaultTypography.labelLarge.copy(fontFamily = PokemonClassicFont),
    labelMedium = DefaultTypography.labelMedium.copy(fontFamily = PokemonClassicFont),
    labelSmall = DefaultTypography.labelSmall.copy(fontFamily = PokemonClassicFont),
)