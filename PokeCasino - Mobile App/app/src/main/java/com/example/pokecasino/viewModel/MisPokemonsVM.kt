package com.example.pokecasino.viewModel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.pokecasino.repository.PokedexRepository
import com.example.pokecasino.utils.ScreenMode
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class MisPokemonsUiState(
    val currentNo: Int = 1,
    val inputNo: String = "001",
    val maxNumber: Int = 151,
    val screenMode: ScreenMode = ScreenMode.SPRITE,
    val obtenidos: Set<Int> = emptySet(),
    val nombre: String = "???",
    val tipos: List<String> = listOf("???"),
    val descripcion: String = "???"
)

class MisPokemonsVM(
    private val repo: PokedexRepository,
    private val tokenProvider: () -> String
) : ViewModel() {

    private val _uiState = MutableStateFlow(MisPokemonsUiState())
    val uiState: StateFlow<MisPokemonsUiState> = _uiState.asStateFlow()

    private val state get() = _uiState.value

    init {
        cargarObtenidos()
    }

    private fun cargarObtenidos() {
        val token = tokenProvider()
        if (token.isBlank()) return

        viewModelScope.launch {
            try {
                val lista = repo.loadMisPokemon(token)
                _uiState.value = state.copy(
                    obtenidos = lista.map { it.pokemon_id }.toSet()
                )
                actualizarInfo(state.currentNo)
            } catch (e: Exception) {
                // si falla, se queda todo oculto
            }
        }
    }

    fun onKeypadChange(newValue: String) {
        val clean = newValue.filter { it.isDigit() }.take(3)
        _uiState.value = state.copy(inputNo = clean)
    }

    fun onSearchClick() {
        val number = state.inputNo.toIntOrNull() ?: 1
        irANumero(number)
    }

    fun onClickRight() {
        val next = if (state.currentNo == state.maxNumber) 1
        else (state.currentNo + 1).coerceIn(1, state.maxNumber)
        irANumero(next)
    }

    fun onClickLeft() {
        val prev = if (state.currentNo == 1) state.maxNumber
        else (state.currentNo - 1).coerceIn(1, state.maxNumber)
        irANumero(prev)
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

    private fun irANumero(no: Int) {
        val clamped = no.coerceIn(1, state.maxNumber)
        _uiState.value = state.copy(
            currentNo = clamped,
            inputNo = clamped.toString().padStart(3, '0')
        )
        actualizarInfo(clamped)
    }

    private fun actualizarInfo(no: Int) {
        val tiene = no in state.obtenidos
        if (!tiene) {
            _uiState.value = state.copy(
                nombre = "???",
                tipos = listOf("???"),
                descripcion = "???"
            )
            return
        }

        // si lo tiene, usamos la pokédex para rellenar datos reales
        val token = tokenProvider()
        if (token.isBlank()) return

        viewModelScope.launch {
            try {
                val list = repo.loadAll(token)
                val entry = list.firstOrNull { it.Id == no }
                if (entry != null) {
                    _uiState.value = state.copy(
                        nombre = entry.Nombre,
                        tipos = listOfNotNull(entry.Tipo1, entry.Tipo2),
                        descripcion = entry.Descripcion
                    )
                }
            } catch (_: Exception) {
                // si falla, dejamos "???"
            }
        }
    }
}