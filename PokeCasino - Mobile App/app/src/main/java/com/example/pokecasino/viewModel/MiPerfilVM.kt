package com.example.pokecasino.viewModel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.pokecasino.network.PerfilDto
import com.example.pokecasino.network.PokemonApi
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class PerfilUiState(
    val cargando: Boolean = true,
    val error: String? = null,
    val perfil: PerfilDto? = null
)

class MiPerfilVM(
    private val api: PokemonApi,
    private val tokenProvider: () -> String
) : ViewModel() {

    private val _uiState = MutableStateFlow(PerfilUiState())
    val uiState: StateFlow<PerfilUiState> = _uiState.asStateFlow()

    init {
        cargarPerfil()
    }

    fun cargarPerfil() {
        val token = tokenProvider()
        if (token.isBlank()) {
            _uiState.value = PerfilUiState(
                cargando = false,
                error = "Sin token"
            )
            return
        }

        viewModelScope.launch {
            try {
                val datos = api.getPerfil("Bearer $token")
                _uiState.value = PerfilUiState(
                    cargando = false,
                    perfil = datos
                )
            } catch (e: Exception) {
                _uiState.value = PerfilUiState(
                    cargando = false,
                    error = "No se pudo cargar el perfil"
                )
            }
        }
    }
}
