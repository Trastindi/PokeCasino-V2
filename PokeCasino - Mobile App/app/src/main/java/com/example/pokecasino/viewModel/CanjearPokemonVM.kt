// CanjearPokemonVM.kt
package com.example.pokecasino.viewModel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.pokecasino.repository.PokedexRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class PokemonListItem(
    val id: Int,
    val name: String,
    val price: Int
)

data class CanjearPokemonUiState(
    val items: List<PokemonListItem> = emptyList(),
    val isLoading: Boolean = false,
    val error: String? = null
)

class CanjearPokemonVM(
    private val repo: PokedexRepository,
    private val tokenProvider: () -> String
) : ViewModel() {

    private val _uiState = MutableStateFlow(CanjearPokemonUiState())
    val uiState: StateFlow<CanjearPokemonUiState> = _uiState.asStateFlow()

    private val state get() = _uiState.value

    fun loadPremios() {
        val token = tokenProvider()
        if (token.isBlank()) {
            _uiState.value = state.copy(error = "Sin token")
            return
        }

        viewModelScope.launch {
            _uiState.value = state.copy(isLoading = true, error = null)
            try {
                val premios = repo.loadPremios(token)
                _uiState.value = state.copy(
                    items = premios.map { it.toItem() },
                    isLoading = false
                )
            } catch (e: retrofit2.HttpException) {
                _uiState.value = state.copy(
                    isLoading = false,
                    error = "HTTP ${e.code()}"
                )
            } catch (e: Exception) {
                _uiState.value = state.copy(
                    isLoading = false,
                    error = "EXC: ${e::class.simpleName}"
                )
            }
        }
    }

    fun comprarPokemon(
        item: PokemonListItem,
        onSuccess: (String) -> Unit,
        onError: (String) -> Unit
    ) {
        val token = tokenProvider()
        if (token.isBlank()) {
            onError("No hay token")
            return
        }

        viewModelScope.launch {
            try {
                val resp = repo.comprarPremio(token, item.id)
                onSuccess(resp.msg)
            } catch (e: retrofit2.HttpException) {
                val msg = when (e.code()) {
                    400 -> "No tienes suficientes fichas"
                    404 -> "Pokémon no disponible"
                    else -> "Error del servidor (${e.code()})"
                }
                onError(msg)
            } catch (e: Exception) {
                onError("Error de conexión")
            }
        }
    }
}

// Mapeo DTO → item de UI
private fun com.example.pokecasino.network.PremioDto.toItem() = PokemonListItem(
    id = pokemon_id,
    name = nombre,
    price = precio
)