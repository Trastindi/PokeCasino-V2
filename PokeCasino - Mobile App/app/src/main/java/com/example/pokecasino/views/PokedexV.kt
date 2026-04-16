package com.example.pokecasino.views

import android.graphics.Bitmap
import android.graphics.BitmapFactory
import androidx.activity.compose.BackHandler
import androidx.annotation.DrawableRes
import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.*
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.asImageBitmap
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.constraintlayout.compose.ConstraintLayout
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
import com.example.pokecasino.viewModel.PokedexVM

enum class PokemonType(
    val displayName: String,
    val background: Color,
    val content: Color
) {
    BICHO("Bicho", Color(0xFF729F3F), Color.White),
    DRAGON("Dragón", Color(0xFF53A4CF), Color.White),
    ELECTRICO("Eléctrico", Color(0xFFEED535), Color(0xFF212121)),
    LUCHA("Lucha", Color(0xFFD56723), Color.White),
    FUEGO("Fuego", Color(0xFFFD7D24), Color.White),
    VOLADOR("Volador", Color(0xFF3DC7EF), Color(0xFF212121)),
    FANTASMA("Fantasma", Color(0xFF7B62A3), Color.White),
    PLANTA("Planta", Color(0xFF9BCC50), Color(0xFF212121)),
    TIERRA("Tierra", Color(0xFFF7DE3F), Color(0xFF212121)),
    HIELO("Hielo", Color(0xFF51C4E7), Color(0xFF212121)),
    NORMAL("Normal", Color(0xFFA4ACAF), Color(0xFF212121)),
    VENENO("Veneno", Color(0xFFB97FC9), Color.White),
    PSIQUICO("Psíquico", Color(0xFFF366B9), Color.White),
    ROCA("Roca", Color(0xFFA38C21), Color.White),
    AGUA("Agua", Color(0xFF4592C4), Color.White);
}

// Si recibes un String como "Planta", lo mapeas:
fun pokemonTypeFromName(name: String): PokemonType? =
    PokemonType.values().firstOrNull { it.displayName.equals(name, ignoreCase = true) }

class PokedexViewModelFactory(
    private val repo: PokedexRepository,
    private val tokenProvider: () -> String
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        return PokedexVM(repo, tokenProvider) as T
    }
}

@Composable
fun DashboardScreen(
    navController: NavController,
    api: PokemonApi,
    token: String
) {
    val repo = remember { PokedexRepository(api) }
    val pokedexVM: PokedexVM = viewModel(
        factory = PokedexViewModelFactory(repo) { token }
    )

    val context = LocalContext.current
    val resources = context.resources
    val uiState by pokedexVM.uiState.collectAsState()

    val pokemonSprites by remember {
        mutableStateOf(
            splitSprites(
                BitmapFactory.decodeResource(resources, R.drawable.pokedexicons),
                15,
                11
            )
        )
    }

    val maxNumber = uiState.maxNumber
    val clampedNumber = uiState.currentPokedexNo.coerceIn(1, maxNumber)
    val index = (clampedNumber - 1).coerceIn(0, pokemonSprites.size - 1)

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
                pokemon = pokemonSprites[index],
                ClickUp = { pokedexVM.onClickUp() },
                ClickDown = { pokedexVM.onClickDown() },
                ClickRight = { pokedexVM.onClickRight() },
                ClickLeft = { pokedexVM.onClickLeft() },
                descripcion = uiState.descripcion,
                screenMode = uiState.screenMode
            )

            BottomPanel(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth(),
                pokedexNo = uiState.inputPokedexNo,
                onPokedexNoChange = { pokedexVM.onKeypadChange(it) },
                onSearchClick = { pokedexVM.onSearchClick() },
                tipo1 = tipo1,
                tipo2 = tipo2
            )
        }

        // barra inferior igual que antes
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


@Composable
fun TopPanel(
    modifier: Modifier = Modifier,
    pokemon: Bitmap,
    ClickUp:() -> Unit,
    ClickRight:() -> Unit,
    ClickLeft:() -> Unit,
    ClickDown:() -> Unit,
    descripcion: String,
    screenMode: ScreenMode
) {
    Column(
        modifier = modifier
            .background(Color(0xFFCA215A))
    ) {
        // Barra con cámara
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .height(140.dp)
                .padding(start = 24.dp, end = 24.dp)
        ) {
            Image(
                painter = painterResource(R.drawable.bg_pokidex_1),
                contentDescription = null,
                modifier = Modifier.matchParentSize(),
                contentScale = ContentScale.FillBounds
            )

            Row(
                modifier = Modifier
                    .matchParentSize()
                    .padding(top = 20.dp, start = 6.dp)
            ) {
                Image(
                    painter = painterResource(R.drawable.icon_cam),
                    contentDescription = null,
                    modifier = Modifier.size(84.dp)
                )

                Spacer(Modifier.width(120.dp))

                Image(
                    painter = painterResource(R.drawable.icon_radar_red),
                    contentDescription = null,
                    modifier = Modifier.size(32.dp)
                )
                Image(
                    painter = painterResource(R.drawable.icon_radar_yellow),
                    contentDescription = null,
                    modifier = Modifier.size(32.dp)
                )
                Image(
                    painter = painterResource(R.drawable.icon_radar_green),
                    contentDescription = null,
                    modifier = Modifier.size(32.dp)
                )
            }
        }

        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(top = 6.dp)
        ) {
            if (screenMode == ScreenMode.DESCRIPTION) {
                ScreenArea(
                    modifier = Modifier
                        .width(300.dp)
                        .height(300.dp)
                        .padding(all = 15.dp),
                    pokemon = pokemon,
                    screenMode = screenMode,
                    descripcion = descripcion
                )
            } else {
                ScreenArea(
                    modifier = Modifier
                        .width(216.dp)
                        .height(216.dp)
                        .padding(start = 24.dp),
                    pokemon = pokemon,
                    screenMode = screenMode
                )
            }

            DPAD(
                modifier = Modifier
                    .size(108.dp)
                    .padding(start = 6.dp)
                    .align(Alignment.CenterVertically),
                onClickUp = { ClickUp() },
                onClickDown = { ClickDown() },
                onClickLeft = { ClickLeft() },
                onClickRight = { ClickRight() },
                screenMode = screenMode
            )
        }
    }
}

@Composable
fun ScreenArea(
    modifier: Modifier = Modifier,
    pokemon: Bitmap,
    screenMode: ScreenMode,
    descripcion: String = ""
)
 {
    Box(modifier = modifier) {
        Image(
            painter = painterResource(R.drawable.bg_screen),
            contentDescription = null,
            modifier = Modifier.matchParentSize(),
            contentScale = ContentScale.FillBounds
        )

        when (screenMode) {
            ScreenMode.SPRITE -> {
                Image(
                    bitmap = pokemon.asImageBitmap(),
                    contentDescription = null,
                    modifier = Modifier
                        .fillMaxWidth(0.75f)
                        .fillMaxHeight(0.75f)
                        .align(Alignment.Center)
                )
            }

            ScreenMode.DESCRIPTION -> {
                Box(
                    modifier = Modifier
                        .fillMaxWidth(0.95f)
                        .fillMaxHeight(0.80f)
                        .padding(start = 15.dp, top = 20.dp)
                        .verticalScroll(rememberScrollState())
                ) {
                    Text(
                        text = descripcion,
                        color = Color.White
                    )
                }
            }

            ScreenMode.REGION -> {
                Image(
                    painter = painterResource(R.drawable.kanto),
                    contentDescription = null,
                    modifier = Modifier
                        .fillMaxWidth(0.95f)
                        .fillMaxHeight(0.95f)
                        .padding(start = 5.dp)
                )
            }
        }

        Column(
            modifier = Modifier.matchParentSize()
        ) {
            Row(
                modifier = Modifier
                    .wrapContentSize()
                    .align(Alignment.CenterHorizontally)
                    .padding(top = 3.dp)
            ) {
                Image(
                    painter = painterResource(R.drawable.icon_radar_red),
                    contentDescription = null,
                    modifier = Modifier.size(16.dp)
                )
                Image(
                    painter = painterResource(R.drawable.icon_radar_red),
                    contentDescription = null,
                    modifier = Modifier.size(16.dp)
                )
            }

            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(start = 6.dp, end = 10.dp, bottom = 34.dp)
            ) {
                // Preview de la cámara más adelante
            }
        }
    }
}

@Composable
fun DPAD(
    modifier: Modifier = Modifier,
    onClickUp: () -> Unit,
    onClickRight: () -> Unit,
    onClickLeft: () -> Unit,
    onClickDown: () -> Unit,
    screenMode: ScreenMode
) {
    ConstraintLayout(modifier = modifier) {
        val (up, right, left, down, center) = createRefs()

        DPadButton(
            direction = Direction.UP,
            icon = R.drawable.icon_up,
            modifier = Modifier
                .clickable { onClickUp() }
                .constrainAs(up) {
                    top.linkTo(parent.top)
                    start.linkTo(parent.start)
                    end.linkTo(parent.end)
                }
        )

        if (screenMode == ScreenMode.SPRITE) {
            DPadButton(
                direction = Direction.RIGHT,
                icon = R.drawable.icon_right,
                modifier = Modifier
                    .clickable { onClickRight() }
                    .constrainAs(right) {
                        top.linkTo(parent.top)
                        bottom.linkTo(parent.bottom)
                        end.linkTo(parent.end)
                    }
            )

            DPadButton(
                direction = Direction.LEFT,
                icon = R.drawable.icon_left,
                modifier = Modifier
                    .clickable { onClickLeft() }
                    .constrainAs(left) {
                        top.linkTo(parent.top)
                        bottom.linkTo(parent.bottom)
                        start.linkTo(parent.start)
                    }
            )
        }

        DPadButton(
            direction = Direction.DOWN,
            icon = R.drawable.icon_down,
            modifier = Modifier
                .clickable { onClickDown() }
                .constrainAs(down) {
                    bottom.linkTo(parent.bottom)
                    start.linkTo(parent.start)
                    end.linkTo(parent.end)
                }
        )

        // centro igual que antes...
    }
}

@Composable
private fun DPadButton(
    direction: Direction,       // UP, DOWN, LEFT, RIGHT
    modifier: Modifier = Modifier,
    @DrawableRes icon: Int
) {
    val shape = when (direction) {
        Direction.UP -> RoundedCornerShape(topStart = 8.dp, topEnd = 8.dp)
        Direction.DOWN -> RoundedCornerShape(bottomStart = 8.dp, bottomEnd = 8.dp)
        Direction.LEFT -> RoundedCornerShape(topStart = 8.dp, bottomStart = 8.dp)
        Direction.RIGHT -> RoundedCornerShape(topEnd = 8.dp, bottomEnd = 8.dp)
    }

    Box(
        modifier = modifier
            .size(36.dp)
            .background(color = Color(0xFF001A4D), shape = shape),
        contentAlignment = Alignment.Center
    ) {
        Image(
            painter = painterResource(icon),
            contentDescription = null,
            modifier = Modifier.fillMaxSize(0.7f),
            contentScale = ContentScale.Fit
        )
    }
}

enum class Direction { UP, DOWN, LEFT, RIGHT }

@Composable
fun BottomPanel(
    modifier: Modifier = Modifier,
    pokedexNo: String,
    onPokedexNoChange: (String) -> Unit,
    onSearchClick: () -> Unit,
    tipo1:String,
    tipo2:String
) {
    Column(
        modifier = modifier
            .background(Color(0xFFFE1A55))
            .padding(top = 12.dp)
    ) {
        KeysCard(
            pokedexNo = pokedexNo,
            onPokedexNoChange = onPokedexNoChange
        )
        BottomButtonsRow(tipo1, tipo2)
        SearchClearRow(
            onClickSearch = onSearchClick,
            onClickClear = { onPokedexNoChange("") }
        )
    }
}

@Composable
fun NameDisplay(pokedexNo: String) {
    Box(
        modifier = Modifier
            .fillMaxWidth()
            .height(84.dp)
            .padding(start = 12.dp, top = 6.dp, end = 12.dp, bottom = 6.dp)
    ) {
        Image(
            painter = painterResource(R.drawable.bg_input_name),
            contentDescription = null,
            modifier = Modifier.matchParentSize(),
            contentScale = ContentScale.FillBounds
        )

        Box(
            modifier = Modifier.matchParentSize(),
            contentAlignment = Alignment.Center
        ) {
            val texto = if (pokedexNo.isBlank()) "Introduce No Pokemon" else pokedexNo

            Text(
                text = texto,
                fontFamily = PokemonClassicFont,
                fontSize = 16.sp,
                color = Color(0xFFE9E9E9)
            )
        }
    }
}

@Composable
fun KeysCard(
    pokedexNo: String,
    onPokedexNoChange: (String) -> Unit
) {
    NameDisplay(pokedexNo)

    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(12.dp),
        shape = RoundedCornerShape(12.dp)
    ) {
        Column(
            modifier = Modifier.fillMaxWidth()
        ) {
            Row(Modifier.fillMaxWidth()) {
                listOf("0", "1", "2", "3", "4").forEach { digit ->
                    KeyButton(
                        text = digit,
                        modifier = Modifier.weight(1f),
                        onClick = { d ->
                            if (pokedexNo.length < 3) {
                                onPokedexNoChange(pokedexNo + d)
                            }
                        }
                    )
                }
            }
            Row(Modifier.fillMaxWidth()) {
                listOf("5", "6", "7", "8", "9").forEach { digit ->
                    KeyButton(
                        text = digit,
                        modifier = Modifier.weight(1f),
                        onClick = { d ->
                            if (pokedexNo.length < 3) {
                                onPokedexNoChange(pokedexNo + d)
                            }
                        }
                    )
                }
            }
        }
    }
}

@Composable
fun KeyButton(
    text: String,
    modifier: Modifier = Modifier,
    onClick: (String) -> Unit
) {
    Box(
        modifier = modifier
            .height(48.dp)
            .clickable { onClick(text) },
        contentAlignment = Alignment.Center
    ) {
        Image(
            painter = painterResource(R.drawable.bg_btn_keys),
            contentDescription = null,
            modifier = Modifier.matchParentSize(),
            contentScale = ContentScale.FillBounds
        )

        Text(
            text = text,
            color = Color.White
        )
    }
}

@Composable
fun SearchClearRow(
    onClickSearch: () -> Unit,
    onClickClear: () -> Unit
) {
    ConstraintLayout(
        modifier = Modifier
            .fillMaxWidth()
            .padding(6.dp)
    ) {
        val (search, clear, icon) = createRefs()

        Box(
            modifier = Modifier
                .width(125.dp)
                .height(44.dp)
                .clickable { onClickSearch() }   // ← aquí
                .constrainAs(search) {
                    start.linkTo(parent.start)
                    top.linkTo(parent.top)
                    bottom.linkTo(parent.bottom)
                }
        ) {
            Image(
                painter = painterResource(R.drawable.bg_btn_white),
                contentDescription = null,
                modifier = Modifier.matchParentSize(),
                contentScale = ContentScale.FillBounds
            )

            Box(
                modifier = Modifier.matchParentSize(),
                contentAlignment = Alignment.Center
            ) {
                Text(
                    text = "SEARCH",
                    color = Color(0xFF001A4D)
                )
            }
        }

        Box(
            modifier = Modifier
                .width(125.dp)
                .height(44.dp)
                .constrainAs(clear) {
                    start.linkTo(search.end, margin = 12.dp)
                    top.linkTo(parent.top)
                    bottom.linkTo(parent.bottom)
                }
        ) {
            Image(
                painter = painterResource(R.drawable.bg_btn_white),
                contentDescription = null,
                modifier = Modifier.matchParentSize(),
                contentScale = ContentScale.FillBounds
            )

            Box(
                modifier = Modifier
                    .matchParentSize()
                    .clickable { onClickClear() },
                contentAlignment = Alignment.Center
            ) {
                Text(
                    text = "CLEAR",
                    color = Color(0xFF001A4D)
                )
            }
        }

        Image(
            painter = painterResource(R.drawable.icon_radar_yellow),
            contentDescription = null,
            modifier = Modifier
                .size(44.dp)
                .constrainAs(icon) {
                    end.linkTo(parent.end)
                    top.linkTo(parent.top)
                    bottom.linkTo(parent.bottom)
                }
        )
    }
}

@Composable
fun BottomButtonsRow(tipo1: String, tipo2: String) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(3.dp)
    ) {
        val tipoPrincipal = pokemonTypeFromName(tipo1) ?: return
        Button(
            onClick = {},
            modifier = Modifier
                .weight(1f)
                .padding(6.dp),   // texto
                    colors = ButtonDefaults.buttonColors(
                    containerColor = tipoPrincipal.background,
                    contentColor = tipoPrincipal.content
            )
        ) {
            Text(tipo1)
        }

        if(tipo2.isNotBlank()) {
            val tipoSecundino = pokemonTypeFromName(tipo2) ?: return
            Button(
                onClick = {},
                modifier = Modifier
                    .weight(1f)
                    .padding(6.dp),   // texto
                colors = ButtonDefaults.buttonColors(
                    containerColor = tipoSecundino.background,
                    contentColor = tipoSecundino.content
                )
            ) {
                Text(tipo2)
            }
        }
    }
}