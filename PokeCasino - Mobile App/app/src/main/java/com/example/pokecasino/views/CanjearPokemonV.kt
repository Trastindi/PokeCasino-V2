// CanjearPokemonV.kt
package com.example.pokecasino.views

import android.graphics.BitmapFactory
import android.widget.Toast
import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.asImageBitmap
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavController
import com.example.pokecasino.R
import com.example.pokecasino.network.PokemonApi
import com.example.pokecasino.repository.PokedexRepository
import com.example.pokecasino.utils.splitSprites
import com.example.pokecasino.viewModel.CanjearPokemonVM
import com.example.pokecasino.viewModel.PokemonListItem

class CanjearPokemonViewModelFactory(
    private val repo: PokedexRepository,
    private val tokenProvider: () -> String
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        return CanjearPokemonVM(repo, tokenProvider) as T
    }
}

@Composable
fun CanjearPokemonScreen(
    navController: NavController,
    api: PokemonApi,
    token: String,
    onBack: () -> Unit = {}
) {
    val repo = remember { PokedexRepository(api) }

    val vm: CanjearPokemonVM = viewModel(
        factory = CanjearPokemonViewModelFactory(
            repo = repo,
            tokenProvider = { token }
        )
    )

    val uiState by vm.uiState.collectAsState()
    val context = LocalContext.current

    LaunchedEffect(Unit) {
        vm.loadPremios()
    }

    val resources = LocalContext.current.resources
    val pokemonSprites by remember {
        mutableStateOf(
            splitSprites(
                BitmapFactory.decodeResource(resources, R.drawable.pokedexicons),
                15,
                11
            )
        )
    }

    Box(modifier = Modifier.fillMaxSize()) {
        Image(
            painter = painterResource(id = R.drawable.canjearpokemon),
            contentDescription = "Tienda",
            modifier = Modifier.fillMaxSize(),
            contentScale = ContentScale.Fit
        )

        if (uiState.error != null) {
            Text(
                text = uiState.error ?: "",
                modifier = Modifier.align(Alignment.TopCenter)
            )
        }

        Box(
            modifier = Modifier
                .fillMaxWidth()
                .padding(10.dp)
                .align(Alignment.Center)
        ) {
            val state = rememberLazyListState()

            LazyColumn(
                modifier = Modifier
                    .height(400.dp)
                    .fillMaxWidth()
                    .padding(horizontal = 16.dp, vertical = 16.dp),
                state = state,
                verticalArrangement = Arrangement.spacedBy(8.dp),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                items(uiState.items) { item ->
                    Column(
                        modifier = Modifier
                            .fillMaxWidth(),
                        horizontalAlignment = Alignment.CenterHorizontally
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()          // ancho máximo del contenido
                                .padding(vertical = 4.dp, horizontal = 4.dp)
                                .background(Color.White),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            val index = (item.id - 1).coerceIn(0, pokemonSprites.size - 1)

                            Box(
                                modifier = Modifier
                                    .size(48.dp)
                                    .background(Color.White)
                                    .padding(4.dp),
                                contentAlignment = Alignment.Center
                            ) {
                                Image(
                                    bitmap = pokemonSprites[index].asImageBitmap(),
                                    contentDescription = item.name,
                                    modifier = Modifier.fillMaxSize(),
                                    contentScale = ContentScale.Fit
                                )
                            }

                            Column(
                                modifier = Modifier.weight(1f),
                                horizontalAlignment = Alignment.CenterHorizontally
                            ) {
                                Text(
                                    text = item.name,
                                    textAlign = TextAlign.Center
                                )
                                Text(
                                    text = "${item.price} fichas",
                                    textAlign = TextAlign.Center
                                )
                            }

                            Box(
                                modifier = Modifier
                                    .size(48.dp)
                                    .background(Color.White)
                                    .padding(4.dp),
                                contentAlignment = Alignment.Center
                            ) {
                                Image(
                                    bitmap = pokemonSprites[index].asImageBitmap(),
                                    contentDescription = item.name,
                                    modifier = Modifier.fillMaxSize(),
                                    contentScale = ContentScale.Fit
                                )
                            }
                        }

                        Button(
                            onClick = {
                                vm.comprarPokemon(
                                    item,
                                    onSuccess = { msg ->
                                        Toast.makeText(context, msg, Toast.LENGTH_SHORT).show()
                                    },
                                    onError = { msg ->
                                        Toast.makeText(context, msg, Toast.LENGTH_SHORT).show()
                                    }
                                )
                            }
                        ) {
                            Text("Canjear")
                        }
                    }
                }
            }
        }
    }
}