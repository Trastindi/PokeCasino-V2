// PokemonApi.kt
package com.example.pokecasino.network

import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.Query
import retrofit2.http.POST

data class LoginRequest(
    val email: String? = null,
    val username: String? = null,
    val password: String
)

data class LoginResponse(
    val mensaje: String,
    val token: String,
    val username: String,
    val email: String
)

data class PokedexEntryDto(
    val Id: Int,
    val Nombre: String,
    val Tipo1: String,
    val Tipo2: String?,
    val Region: String,
    val Descripcion: String,
    val Evoluciones: List<Int>
)

data class PremioDto(
    val pokemon_id: Int,
    val nombre: String,
    val precio: Int
)

data class ComprarPremioResponse(
    val msg: String,
    val pokemon: PremioCompradoDto
)

data class PremioCompradoDto(
    val pokemon_id: Int,
    val nombre: String,
    val fecha_obtenido: String
)

data class MisPokemonDto(
    val pokemon_id: Int,
    val nombre: String,
    val fecha_obtenido: String
)

data class PerfilDto(
    val username: String,
    val nombre: String,
    val apellido: String,
    val edad: Int,
    val email: String,
    val rol: String,
    val fichas: Int,
    val pokes: Int,
    val pokemons: Int // será la cantidad de elementos que contiene el array
)

interface PokemonApi {

    @POST("/auth/login")
    suspend fun login(@Body body: LoginRequest): LoginResponse

    // Lista completa protegida con JWT
    @GET("/pokedex")
    suspend fun getPokedex(
        @Header("Authorization") bearerToken: String
    ): List<PokedexEntryDto>

    @GET("premios")
    suspend fun getPremios(
        @Header("Authorization") auth: String
    ): List<PremioDto>

    @POST("/premios/comprar/{pokemonId}")
    suspend fun comprarPremio(
        @Header("Authorization") bearerToken: String,
        @retrofit2.http.Path("pokemonId") pokemonId: Int
    ): ComprarPremioResponse

    @GET("usuarios/mis_pokemon")
    suspend fun getMisPokemon(
        @Header("Authorization") auth: String
    ): List<MisPokemonDto>

    @GET("auth/me")
    suspend fun getPerfil(
        @Header("Authorization") auth: String
    ): PerfilDto

}
