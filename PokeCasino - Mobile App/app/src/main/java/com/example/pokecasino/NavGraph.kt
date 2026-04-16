package com.example.pokecasino

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.pokecasino.views.MiPerfilScreen
import com.example.pokecasino.views.HomePageScreenBackGround
import com.example.pokecasino.views.LoginScreen
import com.example.pokecasino.viewModel.HomePageVM
import com.example.pokecasino.views.CanjearPokemonScreen
import com.example.pokecasino.views.CasinoScreen
import com.example.pokecasino.views.DashboardScreen
import com.example.pokecasino.views.MisPokemonsScreen

object Routes {
    const val LOGIN = "login"
    const val HOME = "home"
    const val CASINO = "casino"
    const val POKEDEX = "pokedex"
    const val TIENDA = "tienda"
    const val MISPOKEMONS = "mispokemons"
    const val MIPERFIL = "miperfil"
}

@Composable
fun AppNavHost() {
    val navController = rememberNavController()

    var isAdmin by remember { mutableStateOf(false) }
    var token by remember { mutableStateOf("") }

    // Retrofit + API
    val retrofit = remember {
        retrofit2.Retrofit.Builder()
            .baseUrl("https://api.pokemoncasino.dpdns.org") // o tu baseUrl
            .addConverterFactory(retrofit2.converter.gson.GsonConverterFactory.create())
            .build()
    }
    val pokemonApi = remember { retrofit.create(com.example.pokecasino.network.PokemonApi::class.java) }

    NavHost(
        navController = navController,
        startDestination = Routes.LOGIN
    ) {
        composable(Routes.LOGIN) {
            LoginScreen(

                onLoginSuccess = { admin, jwtToken ->
                    isAdmin = admin
                    token = jwtToken
                    navController.navigate(Routes.HOME) {
                        popUpTo(Routes.LOGIN) { inclusive = true }
                    }
                }
            )
        }

        composable(Routes.HOME) {
            val homeViewModel: HomePageVM = viewModel()

            LaunchedEffect(isAdmin) {
                homeViewModel.setRole(isAdmin)
            }

            HomePageScreenBackGround(
                isAdmin = isAdmin,
                homeViewModel = homeViewModel,
                navToRoute = { route ->
                    navController.navigate(route)
                }
            )
        }

        composable(Routes.CASINO) {
            CasinoScreen(
                navController = navController,
                api = pokemonApi,
                token = token
            )
        }

        composable(Routes.POKEDEX) {
            DashboardScreen(
                navController = navController,
                api = pokemonApi,
                token = token
            )
        }

        composable(Routes.TIENDA) {
            CanjearPokemonScreen(
                navController = navController,
                api = pokemonApi,
                token = token
            )
        }

        composable(Routes.MISPOKEMONS) {
            MisPokemonsScreen(
                navController = navController,
                api = pokemonApi,
                token = token
            )
        }

        composable(Routes.MIPERFIL) {
            MiPerfilScreen(
                navController = navController,
                api = pokemonApi,
                token = token
            )
        }

    }
}