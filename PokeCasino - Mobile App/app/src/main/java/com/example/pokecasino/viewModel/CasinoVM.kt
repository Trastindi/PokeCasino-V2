package com.example.pokecasino.viewModel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.example.pokecasino.network.CasinoRequest
import com.example.pokecasino.network.CasinoResponse
import com.example.pokecasino.network.PokemonApi
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch
import retrofit2.HttpException

class CasinoViewModelFactory(
    private val api: PokemonApi,
    private val tokenProvider: () -> String
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        return CasinoVM(api, tokenProvider) as T
    }
}

data class CasinoUiState(
    val showDialog: Boolean = true,
    val showWin: Boolean = false,
    val coin: Int = 0,
    val girando1: Boolean = true,
    val girando2: Boolean = true,
    val girando3: Boolean = true,
    val stopCount: Int = 0,
    val rodillo1: List<Int> = listOf(0, 1, 2),
    val rodillo2: List<Int> = listOf(0, 1, 2),
    val rodillo3: List<Int> = listOf(0, 1, 2),
    val lineasGanadoras: List<Int> = emptyList(),
    val payout: Int = 0,
    val fichasFinal: Int? = null,
    val error: String? = null
)

class CasinoVM(
    private val api: PokemonApi,
    private val tokenProvider: () -> String
) : ViewModel() {

    private val _uiState = MutableStateFlow(CasinoUiState())
    val uiState: StateFlow<CasinoUiState> = _uiState.asStateFlow()

    private val state get() = _uiState.value

    init {
        startRodillo(1)
        startRodillo(2)
        startRodillo(3)
    }

    private fun startRodillo(num: Int) {
        viewModelScope.launch {
            while (when (num) {
                    1 -> state.girando1
                    2 -> state.girando2
                    else -> state.girando3
                }
            ) {
                val nuevo = girarRodillo()
                _uiState.value = when (num) {
                    1 -> state.copy(rodillo1 = nuevo)
                    2 -> state.copy(rodillo2 = nuevo)
                    else -> state.copy(rodillo3 = nuevo)
                }
                delay(80)
            }
        }
    }

    fun onConfirmAmount(amount: Int) {
        _uiState.value = state.copy(
            coin = amount,
            showDialog = false,
            showWin = false,
            lineasGanadoras = emptyList(),
            payout = 0,
            fichasFinal = null,
            error = null
        )
        if (!state.girando1 || !state.girando2 || !state.girando3) {
            _uiState.value = state.copy(
                girando1 = true,
                girando2 = true,
                girando3 = true,
                stopCount = 0
            )
            startRodillo(1)
            startRodillo(2)
            startRodillo(3)
        }
    }

    fun onDismissDialog() {
        _uiState.value = state.copy(showDialog = false)
    }

    fun onStopRodillo1() = stopRodillo(1)
    fun onStopRodillo2() = stopRodillo(2)
    fun onStopRodillo3() = stopRodillo(3)

    private fun stopRodillo(num: Int) {
        val shouldStop = when (num) {
            1 -> state.girando1
            2 -> state.girando2
            else -> state.girando3
        }
        if (!shouldStop) return

        _uiState.value = when (num) {
            1 -> state.copy(girando1 = false, stopCount = state.stopCount + 1)
            2 -> state.copy(girando2 = false, stopCount = state.stopCount + 1)
            else -> state.copy(girando3 = false, stopCount = state.stopCount + 1)
        }

        if (_uiState.value.stopCount == 3) {
            enviarAlServidor()
        }
    }

    private fun enviarAlServidor() {
        val token = tokenProvider()
        if (token.isBlank()) {
            _uiState.value = state.copy(
                showWin = true,
                error = "Sin token"
            )
            return
        }

        val tablero = listOf(state.rodillo1, state.rodillo2, state.rodillo3)

        viewModelScope.launch {
            try {
                val resp: CasinoResponse = api.jugarCasino(
                    "Bearer $token",
                    CasinoRequest(
                        apuesta = state.coin,
                        tablero = tablero
                    )
                )
                _uiState.value = state.copy(
                    lineasGanadoras = resp.lineas_ganadoras,
                    payout = resp.payout,
                    fichasFinal = resp.fichas_final,
                    showWin = true,
                    showDialog = false,
                    error = null
                )
            } catch (e: HttpException) {
                _uiState.value = state.copy(
                    showWin = true,
                    error = "HTTP ${e.code()}",
                    lineasGanadoras = emptyList()
                )
            } catch (e: Exception) {
                _uiState.value = state.copy(
                    showWin = true,
                    error = "Error de conexión",
                    lineasGanadoras = emptyList()
                )
            }
        }
    }

    fun onRestart() {
        _uiState.value = state.copy(
            showWin = false,
            showDialog = true,
            lineasGanadoras = emptyList(),
            payout = 0,
            fichasFinal = null,
            error = null,
            stopCount = 0,
            girando1 = true,
            girando2 = true,
            girando3 = true
        )
        startRodillo(1)
        startRodillo(2)
        startRodillo(3)
    }

    private fun girarRodillo(): List<Int> {
        val pos1 = (0..5).random()
        val pos2 = (0..5).random()
        val pos3 = (0..5).random()
        return listOf(pos1, pos2, pos3)
    }
}