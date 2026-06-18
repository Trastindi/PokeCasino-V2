using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    /// <summary>
    /// Acceso a la colección PokemonTeams a través del servidor Flask.
    /// Endpoints reales:
    ///   GET    /users/pokemonteams              → List&lt;Equipo&gt;
    ///   POST   /users/pokemonteams              → Equipo creado
    ///   PUT    /users/pokemonteams/{teamId}     → Equipo actualizado
    ///   DELETE /users/pokemonteams/{teamId}     → 204
    /// Todos requieren Bearer token (token_required).
    /// </summary>
    public class EquipoRepository
    {
        /// <summary>Devuelve todos los equipos del usuario autenticado.</summary>
        public List<Equipo> GetMisEquipos()
            => ApiClient.Get<List<Equipo>>("/users/pokemonteams");

        /// <summary>Crea un equipo nuevo con el nombre indicado (sin pokémon aún).</summary>
        public Equipo CrearEquipo(string nombre)
            => ApiClient.Post<Equipo>("/users/pokemonteams", new
            {
                team_name   = nombre,
                pokemon_ids = new string[0]
            });

        /// <summary>Reemplaza la lista de pokémon (_id strings) de un equipo existente.</summary>
        public Equipo ActualizarEquipo(string teamId, List<string> pokemonIds)
            => ApiClient.Put<Equipo>($"/users/pokemonteams/{teamId}", new
            {
                pokemon_ids = pokemonIds
            });

        /// <summary>Elimina un equipo por su _id.</summary>
        public void EliminarEquipo(string teamId)
            => ApiClient.Delete($"/users/pokemonteams/{teamId}");
    }
}
