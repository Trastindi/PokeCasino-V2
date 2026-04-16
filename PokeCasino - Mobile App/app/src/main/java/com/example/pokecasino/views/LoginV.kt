package com.example.pokecasino.views

import android.widget.Toast
import androidx.compose.foundation.Image
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Visibility
import androidx.compose.material.icons.filled.VisibilityOff
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.Font
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.unit.IntOffset
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.example.pokecasino.R
import com.example.pokecasino.viewModel.AdminState
import com.example.pokecasino.viewModel.EasterEgg
import com.example.pokecasino.viewModel.LoginState
import com.example.pokecasino.viewModel.LoginVM

val PokemonClassicFont = FontFamily(
    Font(R.font.pokemon_classic)
)

@Composable
fun LoginScreen(
    onLoginSuccess: (isAdmin: Boolean, token: String) -> Unit,
    loginVM: LoginVM = viewModel()
) {
    var userCode by remember { mutableStateOf(listOf<String>()) }
    val easterEggStatus by loginVM.easterEggStatus.collectAsState()
    val context = LocalContext.current

    LaunchedEffect(easterEggStatus) {
        if (easterEggStatus is EasterEgg.EasterEggObtained &&
            (easterEggStatus as EasterEgg.EasterEggObtained).obtained
        ) {
            Toast.makeText(context, "Felicidades, secreto encontrado!", Toast.LENGTH_SHORT).show()
        }
    }

    fun registerInput(input: String) {
        val updated = (userCode + input)
        userCode = if (updated.size > 10) {
            updated.drop(updated.size - 10)
        } else updated

        if (userCode.size == 10) {
            loginVM.entrarEasterEgg(userCode)
        }
    }

    Box(
        modifier = Modifier.fillMaxSize()
    ) {
        // CAPA 1: fondo a pantalla completa (puede ser solo un color)


        // CAPA 2: consola centrada, sin deformar, con botones relativos
        Box(
            modifier = Modifier
                .fillMaxSize(),
            contentAlignment = Alignment.Center
        ) {
            BoxWithConstraints(
                modifier = Modifier.fillMaxWidth()
            ) {
                val widthPx = constraints.maxWidth.toFloat()
                val heightPx = constraints.maxHeight.toFloat()

                Image(
                    painter = painterResource(id = R.drawable.pokebackground),
                    contentDescription = null,
                    modifier = Modifier.fillMaxWidth(),
                    contentScale = ContentScale.Fit
                )

                Login(
                    modifier = Modifier.fillMaxWidth(),
                    onLoginSuccess = onLoginSuccess,
                    loginVM = loginVM,
                    onClickB = { registerInput("B") },
                    onClickA = { registerInput("A") },
                    onClickUp = { registerInput("Up") },
                    onClickDown = { registerInput("Down") },
                    onClickLeft = { registerInput("Left") },
                    onClickRight = { registerInput("Right") },
                    bgWidthPx = widthPx,
                    bgHeightPx = heightPx
                )
            }
        }
    }
}

@Composable
fun Login(
    modifier: Modifier = Modifier,
    onLoginSuccess: (Boolean, String) -> Unit,
    loginVM: LoginVM,
    onClickB: () -> Unit,
    onClickA: () -> Unit,
    onClickUp: () -> Unit,
    onClickDown: () -> Unit,
    onClickLeft: () -> Unit,
    onClickRight: () -> Unit,
    bgWidthPx: Float,
    bgHeightPx: Float
) {
    val userMaxChars = 255
    val passwordMaxChars = 16
    var passwordVisible by remember { mutableStateOf(false) }
    val context = LocalContext.current
    var userText by remember { mutableStateOf("") }
    var passwordText by remember { mutableStateOf("") }

    val loginState by loginVM.loginState.collectAsState()
    val adminState by loginVM.adminState.collectAsState()

    LaunchedEffect(loginState) {
        if (loginState is LoginState.Success && adminState is AdminState.Admin) {
            val isAdmin = (adminState as AdminState.Admin).isAdmin
            val token = (loginState as LoginState.Success).token   // o de donde lo guardes
            onLoginSuccess(isAdmin, token)
        }
    }

    fun posX(fraction: Float) = (bgWidthPx * fraction).toInt()
    fun posY(fraction: Float) = (bgHeightPx * fraction).toInt()

    Column(
        modifier = modifier.fillMaxHeight(),
        verticalArrangement = Arrangement.spacedBy(16.dp),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        // Zona de pantalla superior (inputs, botón, etc.)
        Box(
            modifier = Modifier
                .fillMaxHeight()
        ) {
            OutlinedTextField(
                value = userText,
                onValueChange = { newText ->
                    if (newText.length <= userMaxChars) userText = newText
                },
                textStyle = TextStyle(color = Color.Black, fontFamily = PokemonClassicFont),
                label = { Text("User o Email") },
                modifier = Modifier
                    .fillMaxWidth(0.8f)
                    .offset {
                        IntOffset(
                            x = posX(0f),
                            y = posY(0.08f)
                        )
                    },
                maxLines = 1
            )

            OutlinedTextField(
                value = passwordText,
                onValueChange = { newText ->
                    if (newText.length <= passwordMaxChars) passwordText = newText
                },
                textStyle = TextStyle(color = Color.Black, fontFamily = PokemonClassicFont),
                label = { Text("Password") },
                modifier = Modifier
                    .fillMaxWidth(0.8f)
                    .offset {
                        IntOffset(
                            x = posX(0f),
                            y = posY(0.18f)
                        )
                    },
                maxLines = 1,
                visualTransformation = if (passwordVisible) {
                    VisualTransformation.None
                } else {
                    PasswordVisualTransformation()
                },
                trailingIcon = {
                    val image =
                        if (passwordVisible) Icons.Filled.VisibilityOff else Icons.Filled.Visibility
                    val description =
                        if (passwordVisible) "Ocultar contraseña" else "Mostrar contraseña"

                    IconButton(onClick = { passwordVisible = !passwordVisible }) {
                        Icon(
                            imageVector = image,
                            contentDescription = description,
                            tint = Color.Black
                        )
                    }
                }
            )

            Text(
                text = "Sign up",
                modifier = Modifier.offset {
                    IntOffset(
                        x = posX(0.015f),
                        y = posY(0.26f)
                    )
                }
                    .clickable {
                        Toast.makeText(context, "Hola", Toast.LENGTH_SHORT).show()
                    }
            )

            if (loginState is LoginState.Error) {
                Toast.makeText(
                    context,
                    (loginState as LoginState.Error).message,
                    Toast.LENGTH_SHORT
                ).show()
            }

            Button(
                onClick = { loginVM.loginRemoto(userText, passwordText) },
                modifier = Modifier
                    .fillMaxWidth(0.4f)
                    .fillMaxHeight(0.06f)
                    .offset {
                        IntOffset(
                            x = posX(0.23f),
                            y = posY(0.33f)
                        )
                    },
            ) {
                Text("Sign In")
            }

            // D‑pad
            Button(
                onClick = onClickLeft,
                modifier = Modifier
                    .size(35.dp)
                    .offset {
                        IntOffset(
                            x = posX(0.04f),
                            y = posY(0.72f)
                        )
                    }
            ) { }

            Button(
                onClick = onClickRight,
                modifier = Modifier
                    .size(35.dp)
                    .offset {
                        IntOffset(
                            x = posX(0.21f),
                            y = posY(0.72f)
                        )
                    }
            ) { }

            Button(
                onClick = onClickUp,
                modifier = Modifier
                    .size(35.dp)
                    .offset {
                        IntOffset(
                            x = posX(0.128f),
                            y = posY(0.68f)
                        )
                    }
            ) { }

            Button(
                onClick = onClickDown,
                modifier = Modifier
                    .size(35.dp)
                    .offset {
                        IntOffset(
                            x = posX(0.12f),
                            y = posY(0.759f)
                        )
                    }
            ) { }

            // B y A
            Button(
                onClick = onClickA,
                modifier = Modifier
                    .size(45.dp)
                    .offset {
                        IntOffset(
                            x = posX(0.665f),
                            y = posY(0.68f)
                        )
                    }
            ) { }

            Button(
                onClick = onClickB,
                modifier = Modifier
                    .size(45.dp)
                    .offset {
                        IntOffset(
                            x = posX(0.49f),
                            y = posY(0.705f)
                        )
                    }
            ) { }
        }
    }
}