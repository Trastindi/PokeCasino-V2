package com.example.pokecasino.network

import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

private const val BASE_URL = "https://api.pokemoncasino.dpdns.org"

private val retrofit = Retrofit.Builder()
    .baseUrl(BASE_URL)
    .addConverterFactory(GsonConverterFactory.create())
    .build()

object ApiService {
    val api: PokemonApi by lazy { retrofit.create(PokemonApi::class.java) }
}