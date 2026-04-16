package com.example.pokecasino.utils

import androidx.navigation.NavController
import com.example.pokecasino.Routes

fun handleAppBack(navController: NavController) {
    when (navController.currentDestination?.route) {
        Routes.CASINO -> {
            // Desde Casino vuelve a Home
            navController.navigate(Routes.HOME) {
                popUpTo(Routes.CASINO) { inclusive = true }
            }
        }
        Routes.HOME -> {
            // Desde Home vuelve a Login (y limpia Home del backstack)
            navController.navigate(Routes.LOGIN) {
                popUpTo(Routes.HOME) { inclusive = true }
            }
        }
        else -> {
            // Para otras pantallas, comportamiento normal
            navController.navigateUp()
        }
    }
}