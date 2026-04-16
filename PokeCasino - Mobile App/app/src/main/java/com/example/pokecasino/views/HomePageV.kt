package com.example.pokecasino.views

import androidx.activity.compose.BackHandler
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.pokecasino.R
import com.example.pokecasino.utils.handleAppBack
import com.example.pokecasino.viewModel.HomePageVM

@Composable
fun HomePage(
    homeViewModel: HomePageVM,
    onOptionClick: (String) -> Unit
) {
    val options by homeViewModel.options.collectAsState()
    val titulo by homeViewModel.titulo.collectAsState()
    val state = rememberLazyListState()

    Text(
        text = titulo,
        fontFamily = PokemonClassicFont,
        color = Color.Yellow,
        textAlign = TextAlign.Center,
        modifier = Modifier
            .fillMaxWidth()
            .padding(40.dp)
    )

    Box(
        modifier = Modifier
            .fillMaxSize()
            .padding(10.dp),
        contentAlignment = Alignment.Center
    ) {
        Box(
            modifier = Modifier
                .height(400.dp)
                .fillMaxWidth()
        ) {
            val state = rememberLazyListState()

            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(horizontal = 16.dp, vertical = 8.dp),
                state = state,
                verticalArrangement = Arrangement.spacedBy(16.dp),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                items(options) { text ->
                    Button(
                        onClick = { onOptionClick(text) }
                    ) {
                        Text(
                            text,
                            textAlign = TextAlign.Center
                        )
                    }
                }
            }
        }
    }
}

@Composable
fun HomePageScreenBackGround(
    isAdmin: Boolean,
    homeViewModel: HomePageVM,
    navToRoute: (String) -> Unit
) {
    val options by homeViewModel.options.collectAsState()
    val navigationEvent by homeViewModel.navigationEvent.collectAsState()

    LaunchedEffect(navigationEvent) {
        navigationEvent?.let { route ->
            navToRoute(route)
            homeViewModel.onNavigationHandled()
        }
    }

    Box(modifier = Modifier.fillMaxSize()) {
        val bg = if (isAdmin) {
            R.drawable.adminhomepagebackground
        } else {
            R.drawable.userhomepagebackground
        }

        Image(
            painter = painterResource(id = bg),
            contentDescription = null,
            modifier = Modifier.fillMaxSize(),
            contentScale = ContentScale.Crop
        )

        HomePage(
            homeViewModel = homeViewModel,
            onOptionClick = { opcion ->
                homeViewModel.ejecutarOpcion(opcion)
            }
        )
    }
}