package com.example.pokecasino.views

import androidx.activity.compose.BackHandler
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Card
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavController
import com.example.pokecasino.viewModel.MiPerfilVM
import com.example.pokecasino.network.PerfilDto
import com.example.pokecasino.network.PokemonApi
import com.example.pokecasino.utils.handleAppBack
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider


@Composable
fun MiPerfilScreen(
    navController: NavController,
    api: PokemonApi,
    token: String
) {
    val vm: MiPerfilVM = viewModel(
        factory = object : ViewModelProvider.Factory {
            override fun <T : ViewModel> create(modelClass: Class<T>): T {
                return MiPerfilVM(api) { token } as T
            }
        }
    )

    val uiState by vm.uiState.collectAsState()

    BackHandler { handleAppBack(navController) }

    Box(modifier = Modifier.fillMaxSize()) {
        when {
            uiState.cargando -> {
                Text(
                    text = "Cargando perfil...",
                    modifier = Modifier.align(Alignment.Center)
                )
            }
            uiState.error != null -> {
                Text(
                    text = uiState.error ?: "",
                    modifier = Modifier.align(Alignment.Center)
                )
            }
            uiState.perfil != null -> {
                PerfilContent(perfil = uiState.perfil!!)
            }
        }
    }
}

@Composable
fun PerfilContent(perfil: PerfilDto) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        Text(text = "Mi perfil", fontSize = 24.sp)

        // Datos básicos
        Card(modifier = Modifier.fillMaxWidth()) {
            Column(modifier = Modifier.padding(16.dp)) {
                Text("Usuario: ${perfil.username}")
                Text("Nombre: ${perfil.nombre} ${perfil.apellido}")
                Text("Edad: ${perfil.edad}")
                Text("Email: ${perfil.email}")
                Text("Rol: ${perfil.rol}")
            }
        }

        // Fichas y pokes
        Card(modifier = Modifier.fillMaxWidth()) {
            Row(
                modifier = Modifier
                    .padding(16.dp)
                    .fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Column {
                    Text("Fichas")
                    Text("${perfil.fichas}")
                }
                Column {
                    Text("Pokes")
                    Text("${perfil.pokes}")
                }
            }
        }

        // Numero de pokemons
        Card(modifier = Modifier.fillMaxWidth()) {
            Row(
                modifier = Modifier
                    .padding(16.dp)
                    .fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Column {
                    Text("Pokemons obtenidos")
                    Text("${perfil.pokemons}")
                }
                Column {
                    Text("Pokemons totales")
                    Text("151")
                }
            }
        }

    }
}
