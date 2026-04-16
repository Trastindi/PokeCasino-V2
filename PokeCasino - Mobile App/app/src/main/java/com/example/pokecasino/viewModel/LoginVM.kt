package com.example.pokecasino.viewModel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.pokecasino.network.ApiService
import com.example.pokecasino.network.LoginRequest
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

sealed interface LoginState {
    data object Loading : LoginState
    data class Success(val token: String) : LoginState
    data class Error(val message: String) : LoginState
}

sealed interface AdminState {
    data object Loading : AdminState
    data class Admin(val isAdmin: Boolean) : AdminState
}

sealed interface EasterEgg {
    data object Waiting : EasterEgg
    data class EasterEggObtained(val obtained: Boolean) : EasterEgg
}

class LoginVM : ViewModel() {

    private val nombreAdmin: String = "Oak"

    private val _loginState = MutableStateFlow<LoginState>(LoginState.Loading)
    val loginState: StateFlow<LoginState> = _loginState.asStateFlow()

    private val _adminState = MutableStateFlow<AdminState>(AdminState.Loading)
    val adminState: StateFlow<AdminState> = _adminState.asStateFlow()

    private val konamiCode = listOf("Up", "Up", "Down", "Down", "Left", "Right", "Left", "Right", "B", "A")

    private val _easterEggStatus = MutableStateFlow<EasterEgg>(EasterEgg.Waiting)
    val easterEggStatus: StateFlow<EasterEgg> = _easterEggStatus.asStateFlow()

    fun loginRemoto(user: String, password: String) {
        viewModelScope.launch {
            _loginState.value = LoginState.Loading

            try {
                val body = if ("@" in user) {
                    LoginRequest(email = user, password = password)
                } else {
                    LoginRequest(username = user, password = password)
                }

                val resp = ApiService.api.login(body)

                val isAdmin = resp.username == nombreAdmin
                _adminState.value = AdminState.Admin(isAdmin)
                _loginState.value = LoginState.Success(resp.token)   // ← aquí pasas el JWT

            } catch (e: retrofit2.HttpException) {
                val msg = when (e.code()) {
                    400, 401 -> "Usuario o contraseña incorrectos"
                    else -> "Error del servidor (${e.code()})"
                }
                _loginState.value = LoginState.Error(msg)
            } catch (e: Exception) {
                _loginState.value = LoginState.Error("Error de conexión con el servidor")
            }
        }
    }

    fun entrarEasterEgg(userCode: List<String>) {
        for (y in 0 until konamiCode.count() - 1) {
            if (userCode[y] != konamiCode[y]) {
                _easterEggStatus.value = EasterEgg.EasterEggObtained(obtained = false)
                return
            }
        }
        _easterEggStatus.value = EasterEgg.EasterEggObtained(obtained = true)
    }
}