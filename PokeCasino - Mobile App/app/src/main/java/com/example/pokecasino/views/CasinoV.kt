package com.example.pokecasino.views

import android.content.pm.ActivityInfo
import android.graphics.BitmapFactory
import androidx.activity.compose.BackHandler
import androidx.activity.compose.LocalActivity
import androidx.compose.foundation.Image
import androidx.compose.foundation.combinedClickable
import androidx.compose.foundation.layout.*
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonColors
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
import androidx.compose.ui.unit.sp
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavController
import com.example.pokecasino.R
import com.example.pokecasino.network.PokemonApi
import com.example.pokecasino.utils.handleAppBack
import com.example.pokecasino.utils.splitSprites
import com.example.pokecasino.viewModel.CasinoVM
import com.example.pokecasino.viewModel.CasinoViewModelFactory
import com.example.pokecasino.views.PokemonClassicFont

@Composable
fun CasinoScreen(
    navController: NavController,
    api: PokemonApi,
    token: String
) {
    val activity = LocalActivity.current

    val casinoVM: CasinoVM = viewModel(
        factory = CasinoViewModelFactory(api) { token }
    )
    val uiState by casinoVM.uiState.collectAsState()

    BackHandler {
        handleAppBack(navController)
        activity?.requestedOrientation = ActivityInfo.SCREEN_ORIENTATION_PORTRAIT
    }

    LaunchedEffect(Unit) {
        activity?.requestedOrientation = ActivityInfo.SCREEN_ORIENTATION_LANDSCAPE
    }

    Column(modifier = Modifier.fillMaxSize()) {
        Box(modifier = Modifier.fillMaxSize()) {
            Image(
                painter = painterResource(id = R.drawable.slotmachine),
                contentDescription = "Tragaperras",
                modifier = Modifier.fillMaxSize(),
                contentScale = ContentScale.Fit
            )

            CasinoLayout(
                showFrame = uiState.showDialog || uiState.showWin,
                coin = uiState.coin,
                showWin = uiState.showWin,
                lineasGanadoras = uiState.lineasGanadoras,
                rodillo1 = uiState.rodillo1,
                rodillo2 = uiState.rodillo2,
                rodillo3 = uiState.rodillo3,
                error = uiState.error,
                fichasFinal = uiState.fichasFinal,
                onRestart = { casinoVM.onRestart() },
                onStop1 = { casinoVM.onStopRodillo1() },
                onStop2 = { casinoVM.onStopRodillo2() },
                onStop3 = { casinoVM.onStopRodillo3() }
            )

            if (uiState.showDialog) {
                CasinoDialog(
                    onConfirmAmount = { amount ->
                        casinoVM.onConfirmAmount(amount)
                    },
                    onDismiss = { casinoVM.onDismissDialog() }
                )
            }
        }
    }
}

@Composable
fun CasinoLayout(
    showFrame: Boolean,
    coin: Int,
    showWin: Boolean,
    lineasGanadoras: List<Int>,
    rodillo1: List<Int>,
    rodillo2: List<Int>,
    rodillo3: List<Int>,
    error: String?,
    fichasFinal: Int?,
    onRestart: () -> Unit,
    onStop1: () -> Unit,
    onStop2: () -> Unit,
    onStop3: () -> Unit
) {
    val context = LocalContext.current
    val resources = context.resources

    val symbolsSpriteSheet =
        BitmapFactory.decodeResource(resources, R.drawable.slotmachinesymbols)
    val slotMachineSymbolsSpriteSheet = splitSprites(symbolsSpriteSheet, cols = 1, rows = 6)

    Column(
        modifier = Modifier
            .fillMaxWidth()
            .fillMaxHeight(),
        verticalArrangement = Arrangement.Bottom
    ) {
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .fillMaxHeight()
        ) {
            if (showFrame || showWin) {
                Image(
                    painter = painterResource(id = R.drawable.dialogframe),
                    contentDescription = "Tragaperras",
                    modifier = Modifier
                        .fillMaxWidth()
                        .fillMaxHeight(0.5f)
                        .align(Alignment.BottomCenter),
                    contentScale = ContentScale.Fit
                )

                val texto = when {
                    error != null -> error
                    !showWin -> "Seleccione la cantidad de fichas"
                    showWin && lineasGanadoras.isEmpty() -> {
                        val fichas = fichasFinal ?: 0
                        "Sin premio\nTe quedan $fichas fichas"
                    }
                    else -> {
                        val fichas = fichasFinal ?: 0
                        "Has hecho ${lineasGanadoras.count()} línea(s)!\nTe quedan $fichas fichas"
                    }
                }

                Text(
                    text = texto,
                    fontFamily = PokemonClassicFont,
                    fontSize = 16.sp,
                    color = Color.Black,
                    modifier = Modifier
                        .align(Alignment.Center)
                        .padding(top = 150.dp, start = 125.dp, end = 250.dp)
                        .fillMaxWidth(),
                    textAlign = TextAlign.Center
                )

                if (showWin) {
                    Button(
                        onClick = { onRestart() },
                        modifier = Modifier
                            .align(Alignment.Center)
                            .padding(top = 250.dp, start = 250.dp, end = 250.dp)
                    ) {
                        Text("Tirar de nuevo")
                    }
                }
            } else {
                // Rodillos
                Row(
                    modifier = Modifier
                        .padding(start = 195.dp, top = 121.dp)
                        .fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(79.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Column {
                        rodillo1.forEach { index ->
                            Image(
                                bitmap = slotMachineSymbolsSpriteSheet[index].asImageBitmap(),
                                contentDescription = null,
                                modifier = Modifier
                                    .height(59.dp)
                                    .width(77.dp)
                            )
                        }
                    }
                    Column {
                        rodillo2.forEach { index ->
                            Image(
                                bitmap = slotMachineSymbolsSpriteSheet[index].asImageBitmap(),
                                contentDescription = null,
                                modifier = Modifier
                                    .height(59.dp)
                                    .width(77.dp)
                            )
                        }
                    }
                    Column {
                        rodillo3.forEach { index ->
                            Image(
                                bitmap = slotMachineSymbolsSpriteSheet[index].asImageBitmap(),
                                contentDescription = null,
                                modifier = Modifier
                                    .height(59.dp)
                                    .width(77.dp)
                            )
                        }
                    }
                }

                // Botones parar rodillos
                Row(
                    modifier = Modifier
                        .padding(start = 203.dp, top = 300.dp)
                        .fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(96.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    listOf(onStop1, onStop2, onStop3).forEach { stop ->
                        Button(
                            onClick = { stop() },
                            modifier = Modifier
                                .height(60.dp)
                                .width(60.dp),
                            colors = ButtonColors(
                                containerColor = Color.Transparent,
                                contentColor = Color.Transparent,
                                disabledContentColor = Color.Transparent,
                                disabledContainerColor = Color.Transparent
                            )
                        ) { }
                    }
                }
            }
        }
    }
}

@Composable
fun CasinoDialog(
    onConfirmAmount: (Int) -> Unit,
    onDismiss: () -> Unit
) {
    val opciones = listOf(3, 2, 1)
    var seleccionada by remember { mutableStateOf(opciones.first()) }

    Box(
        modifier = Modifier
            .fillMaxWidth()
            .fillMaxHeight(),
        contentAlignment = Alignment.BottomCenter
    ) {
        Image(
            painter = painterResource(id = R.drawable.dialogframe),
            contentDescription = "Selector de monedas",
            modifier = Modifier
                .fillMaxWidth()
                .fillMaxHeight(0.5f),
            contentScale = ContentScale.Fit
        )

        Column(
            modifier = Modifier
                .fillMaxWidth()
                .fillMaxHeight(0.5f)
                .padding(24.dp),
            verticalArrangement = Arrangement.SpaceEvenly,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Text(
                text = "Seleccione la cantidad de monedas",
                fontFamily = PokemonClassicFont,
                color = Color.Black,
                textAlign = TextAlign.Center,
                modifier = Modifier.fillMaxWidth()
            )

            opciones.forEach { valor ->
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.Center
                ) {
                    ArrowRadio(
                        selected = seleccionada == valor,
                        onSelect = { seleccionada = valor },
                        onConfirm = {
                            onConfirmAmount(seleccionada)
                            onDismiss()
                        }
                    )
                    Text(
                        text = "$valor",
                        fontFamily = PokemonClassicFont,
                        color = Color.Black,
                        modifier = Modifier.padding(start = 8.dp)
                    )
                }
            }
        }
    }
}

@Composable
fun ArrowRadio(
    selected: Boolean,
    onSelect: () -> Unit,
    onConfirm: () -> Unit
) {
    Box(
        modifier = Modifier
            .size(32.dp)
            .combinedClickable(
                onClick = { onSelect() },
                onDoubleClick = { onConfirm() }
            )
    ) {
        if (selected) {
            Image(
                painter = painterResource(id = R.drawable.arrow),
                contentDescription = "Seleccionado",
                modifier = Modifier.matchParentSize()
            )
        }
    }
}