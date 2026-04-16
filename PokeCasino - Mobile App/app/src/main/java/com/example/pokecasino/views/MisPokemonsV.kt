package com.example.pokecasino.views

import android.graphics.BitmapFactory
import androidx.activity.compose.BackHandler
import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.asImageBitmap
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.unit.dp
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavController
import com.example.pokecasino.R
import com.example.pokecasino.network.PokemonApi
import com.example.pokecasino.repository.PokedexRepository
import com.example.pokecasino.utils.ScreenMode
import com.example.pokecasino.utils.handleAppBack
import com.example.pokecasino.utils.splitSprites
import com.example.pokecasino.viewModel.MisPokemonsVM

class MisPokemonsViewModelFactory(
    private val repo: PokedexRepository,
    private val tokenProvider: () -> String
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        return MisPokemonsVM(repo, tokenProvider) as T
    }
}

@Composable
fun MisPokemonsScreen(
    navController: NavController,
    api: PokemonApi,
    token: String
) {
    val repo = remember { PokedexRepository(api) }
    val vm: MisPokemonsVM = viewModel(
        factory = MisPokemonsViewModelFactory(repo) { token }
    )

    val uiState by vm.uiState.collectAsState()
    val context = LocalContext.current
    val resources = context.resources

    val spritesVisibles by remember {
        mutableStateOf(
            splitSprites(
                BitmapFactory.decodeResource(resources, R.drawable.pokedexicons),
                15,
                11
            )
        )
    }
    val spritesOcultos by remember {
        mutableStateOf(
            splitSprites(
                BitmapFactory.decodeResource(resources, R.drawable.pokedexhiddenicons),
                15,
                11
            )
        )
    }

    val maxNumber = uiState.maxNumber
    val clampedNumber = uiState.currentNo.coerceIn(1, maxNumber)
    val index = (clampedNumber - 1).coerceIn(0, spritesVisibles.size - 1)

    val tiene = clampedNumber in uiState.obtenidos
    val sprite = if (tiene) spritesVisibles[index] else spritesOcultos[index]

    val tipo1 = uiState.tipos.getOrNull(0).orEmpty()
    val tipo2 = uiState.tipos.getOrNull(1).orEmpty()

    BackHandler {
        handleAppBack(navController)
    }

    Box(modifier = Modifier.fillMaxSize()) {
        Column(modifier = Modifier.fillMaxSize()) {

            TopPanel(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth(),
                pokemon = sprite,
                ClickUp = { vm.onClickUp() },
                ClickDown = { vm.onClickDown() },
                ClickRight = { vm.onClickRight() },
                ClickLeft = { vm.onClickLeft() },
                descripcion = uiState.descripcion,
                screenMode = uiState.screenMode
            )

            BottomPanel(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth(),
                pokedexNo = uiState.inputNo.takeIf { tiene }
                    ?: "???",
                onPokedexNoChange = { vm.onKeypadChange(it) },
                onSearchClick = { vm.onSearchClick() },
                tipo1 = if (tiene) tipo1 else "???",
                tipo2 = if (tiene) tipo2 else ""
            )
        }

        Box(
            modifier = Modifier
                .align(Alignment.BottomCenter)
                .fillMaxWidth()
                .height(24.dp)
        ) {
            Image(
                painter = painterResource(R.drawable.icon_separator),
                contentDescription = null,
                modifier = Modifier.matchParentSize(),
                contentScale = ContentScale.FillBounds
            )
        }
    }
}