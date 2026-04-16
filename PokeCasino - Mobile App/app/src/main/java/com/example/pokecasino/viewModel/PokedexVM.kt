package com.example.pokecasino.viewModel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.pokecasino.repository.PokedexRepository
import com.example.pokecasino.utils.ScreenMode
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class PokedexUiState(
    val inputPokedexNo: String = "001",
    val currentPokedexNo: Int = 1,
    val screenMode: ScreenMode = ScreenMode.SPRITE,
    val tipos: List<String> = emptyList(),
    val descripcion: String = "",
    val maxNumber: Int = 151
)

class PokedexVM(
    private val pokedexRepository: PokedexRepository,
    private val tokenProvider: () -> String
) : ViewModel() {

    private val _uiState = MutableStateFlow(PokedexUiState())
    val uiState: StateFlow<PokedexUiState> = _uiState.asStateFlow()

    private val state: PokedexUiState
        get() = _uiState.value

    init {
        loadPokemon(state.currentPokedexNo)
    }

    fun onKeypadChange(newValue: String) {
        val clean = newValue.filter { it.isDigit() }.take(3)
        _uiState.value = state.copy(inputPokedexNo = clean)
    }

    fun onSearchClick() {
        val number = state.inputPokedexNo.toIntOrNull() ?: 1
        val clamped = number.coerceIn(1, state.maxNumber)
        _uiState.value = state.copy(
            currentPokedexNo = clamped,
            inputPokedexNo = clamped.toString().padStart(3, '0')
        )
        loadPokemon(clamped)
    }

    fun onClickRight() {
        val next = if (state.currentPokedexNo == state.maxNumber) 1
        else (state.currentPokedexNo + 1).coerceIn(1, state.maxNumber)

        _uiState.value = state.copy(
            currentPokedexNo = next,
            inputPokedexNo = next.toString().padStart(3, '0')
        )
        loadPokemon(next)
    }

    fun onClickLeft() {
        val prev = if (state.currentPokedexNo == 1) state.maxNumber
        else (state.currentPokedexNo - 1).coerceIn(1, state.maxNumber)

        _uiState.value = state.copy(
            currentPokedexNo = prev,
            inputPokedexNo = prev.toString().padStart(3, '0')
        )
        loadPokemon(prev)
    }

    fun onClickUp() {
        val nextMode = when (state.screenMode) {
            ScreenMode.SPRITE -> ScreenMode.DESCRIPTION
            ScreenMode.DESCRIPTION -> ScreenMode.REGION
            ScreenMode.REGION -> ScreenMode.SPRITE
        }
        _uiState.value = state.copy(screenMode = nextMode)
    }

    fun onClickDown() {
        val nextMode = when (state.screenMode) {
            ScreenMode.SPRITE -> ScreenMode.REGION
            ScreenMode.REGION -> ScreenMode.DESCRIPTION
            ScreenMode.DESCRIPTION -> ScreenMode.SPRITE
        }
        _uiState.value = state.copy(screenMode = nextMode)
    }

    private fun loadPokemon(number: Int) {
        val token = tokenProvider()
        if (token.isBlank()) return

        viewModelScope.launch {
            try {
                val list = pokedexRepository.loadAll(token)
                val entry = list.firstOrNull { it.Id == number }
                if (entry != null) {
                    _uiState.value = state.copy(
                        tipos = listOfNotNull(entry.Tipo1, entry.Tipo2),
                        descripcion = entry.Descripcion
                    )
                } else {
                    _uiState.value = state.copy(
                        tipos = emptyList(),
                        descripcion = "Pokémon no encontrado."
                    )
                }
            } catch (e: Exception) {
                _uiState.value = state.copy(
                    tipos = emptyList(),
                    descripcion = "Error cargando Pokédex."
                )
            }
        }
    }
}