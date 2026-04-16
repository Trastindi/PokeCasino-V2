package com.example.pokecasino.repository

import com.example.pokecasino.network.PokemonApi
import com.example.pokecasino.network.MisPokemonDto

class PokedexRepository(
    private val api: PokemonApi
) {
    suspend fun loadAll(token: String) =
        api.getPokedex("Bearer $token")

    suspend fun loadPremios(token: String) =
        api.getPremios("Bearer $token")

    suspend fun comprarPremio(token: String, pokemonId: Int) =
        api.comprarPremio("Bearer $token", pokemonId)

    // NUEVO: lista de pokémon del usuario
    suspend fun loadMisPokemon(token: String): List<MisPokemonDto> =
        api.getMisPokemon("Bearer $token")
}