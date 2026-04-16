package com.example.pokecasino.viewModel

import androidx.lifecycle.ViewModel
import com.example.pokecasino.Routes
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

class HomePageVM : ViewModel() {

    private val _options = MutableStateFlow<List<String>>(emptyList())
    val options: StateFlow<List<String>> = _options.asStateFlow()

    private val _titulo = MutableStateFlow("")
    val titulo: StateFlow<String> = _titulo.asStateFlow()

    // Evento de navegación: contiene la ruta o null si no hay
    private val _navigationEvent = MutableStateFlow<String?>(null)
    val navigationEvent: StateFlow<String?> = _navigationEvent.asStateFlow()

    private val tituloAdmin = "Menú Administrador"
    private val tituloUser = "Menú usuario"

    private val opcionesAdmin = listOf(
        "Listar usuarios",
        "Listar usuarios con detalles",
        "Modificar datos de un usuario",
        "Eliminar usuario",
        "Resetear contraseña de un usuario",
        "Exportar un usuario a JSON",
        "Exportar todos los usuarios a JSON",
        "Importar usuarios desde JSON",
        "Cerrar sesión"
    )

    private val opcionesUser = listOf(
        "Ver mi perfil",
        "Jugar al casino",
        "Canjear Pokémon",
        "Pokédex",
        "Mis Pokémon",
        "Cerrar sesión"
    )

    fun setRole(isAdmin: Boolean) {
        _options.value = if (isAdmin) opcionesAdmin else opcionesUser
        _titulo.value = if (isAdmin) tituloAdmin else tituloUser
    }

    fun ejecutarOpcion(opcion: String) {
        when (opcion) {
            "Listar usuarios" -> listarusuarios()
            "Listar usuarios con detalles" -> listarUsuariosDetalles()
            "Modificar datos de un usuario" -> modificarUsuario()
            "Eliminar usuario" -> eliminarUsuarios()
            "Resetear contraseña de un usuario" -> resetearContraseñaUsuario()
            "Exportar un usuario a JSON" -> exportarUsuario()
            "Exportar todos los usuarios a JSON" -> exportarUsuarios()
            "Importar usuarios desde JSON" -> importarUsuarios()
            "Ver mi perfil" -> verPerfil()
            "Jugar al casino" -> jugarCasino()
            "Canjear Pokémon" -> canjearPokemon()
            "Pokédex" -> verPokedex()
            "Mis Pokémon" -> misPokemon()
            "Cerrar sesión" -> cerrarSesion()
        }
    }

    // Navegación: solo marcamos la ruta
    private fun jugarCasino() {
        _navigationEvent.value = Routes.CASINO
    }

    private fun verPokedex() {
        _navigationEvent.value = Routes.POKEDEX
    }

    // El resto los dejas como TODO por ahora
    private fun listarusuarios() { /* TODO */ }
    private fun listarUsuariosDetalles() { /* TODO */ }
    private fun modificarUsuario() { /* TODO */ }
    private fun eliminarUsuarios() { /* TODO */ }
    private fun resetearContraseñaUsuario() { /* TODO */ }
    private fun exportarUsuario() { /* TODO */ }
    private fun exportarUsuarios() { /* TODO */ }
    private fun importarUsuarios() { /* TODO */ }
    private fun verPerfil() {
        _navigationEvent.value = Routes.MIPERFIL
    }
    private fun canjearPokemon() {
        _navigationEvent.value = Routes.TIENDA
    }
    private fun misPokemon() {
        _navigationEvent.value = Routes.MISPOKEMONS
    }
    private fun cerrarSesion() {
        _navigationEvent.value = Routes.LOGIN
    }

    // Para “consumir” el evento desde la UI
    fun onNavigationHandled() {
        _navigationEvent.value = null
    }
}