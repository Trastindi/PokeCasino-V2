using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PK_Proyect.Services
{
    public class PokemonUserService
    {
        private readonly PokemonUserRepository _repo;
        private readonly UserRepository _userRepo;
        private readonly PokedexRepository _pokedexRepo;

        public PokemonUserService()
        {
            _repo        = new PokemonUserRepository();
            _userRepo    = new UserRepository();
            _pokedexRepo = new PokedexRepository();
        }

        // ----------------------------------------------------------------
        // Punto de entrada principal: llamado desde GachaViewModel
        // ----------------------------------------------------------------
        public LevelUpResultado ObtenerPokemon(
            string userId, int pokemonId, string nombre,
            string tipo1, string tipo2, int currentHp)
        {
            var resultado = new LevelUpResultado();
            var user      = _userRepo.GetUserById(userId);
            var existente = _repo.GetPokemon(userId, pokemonId);

            // ── NUEVO ────────────────────────────────────────────────────
            if (existente == null)
            {
                var nuevo = new PokemonUser
                {
                    UserId         = userId,
                    Username       = user.Username,
                    PokemonId      = pokemonId,
                    numero_pokedex = pokemonId,
                    Nombre         = nombre,
                    TipoPrincipal  = tipo1,
                    TipoSecundario = tipo2,
                    Nivel          = 1,
                    Cantidad       = 1,
                    FechaObtenido  = DateTime.Now,
                    HiddenPowerSeed  = Random.Shared.Next(0, 16),
                    HiddenPowerPower = (Random.Shared.Next(31, 71) + Random.Shared.Next(31, 71)) / 2,
                    CurrentHp      = currentHp,
                    MoveSet        = new List<string>()
                };

                // Movimientos de nivel 1
                var datosPokedex = _pokedexRepo.ObtenerPorId(pokemonId);
                if (datosPokedex?.Movimientos != null)
                {
                    var movsNivel1 = datosPokedex.Movimientos
                        .Where(m => m.Metodo == "nivel" && m.Nivel == 1)
                        .Select(m => m.Nombre)
                        .ToList();

                    foreach (var mov in movsNivel1)
                    {
                        if (nuevo.MoveSet.Count < 4)
                            nuevo.MoveSet.Add(mov);
                    }
                }

                _repo.InsertPokemon(nuevo);
                RecalcularPokemon(userId);

                resultado.Pokemon = nuevo;
                return resultado;
            }

            // ── EXISTENTE: subir nivel ────────────────────────────────────
            existente.Cantidad++;
            existente.Nivel++;
            existente.Username = user.Username;

            // Backfill campos antiguos
            if (existente.HiddenPowerSeed == 0 && existente.HiddenPowerPower == 0)
            {
                existente.HiddenPowerSeed  = Random.Shared.Next(0, 16);
                existente.HiddenPowerPower = (Random.Shared.Next(31, 71) + Random.Shared.Next(31, 71)) / 2;
            }
            if (existente.CurrentHp == 0) existente.CurrentHp = currentHp;
            if (existente.MoveSet == null) existente.MoveSet = new List<string>();

            // ── 1. MOVIMIENTO NUEVO POR NIVEL ─────────────────────────────
            var pokedex = _pokedexRepo.ObtenerPorId(pokemonId);
            var movNuevo = pokedex?.Movimientos
                ?.FirstOrDefault(m =>
                    m.Metodo == "nivel" &&
                    m.Nivel  == existente.Nivel &&
                    !existente.MoveSet.Contains(m.Nombre));

            if (movNuevo != null)
            {
                resultado.MovimientoAprendido = movNuevo.Nombre;

                if (existente.MoveSet.Count < 4)
                {
                    existente.MoveSet.Add(movNuevo.Nombre);
                    resultado.MovimientoAprendidoDirectamente = true;
                }
                else
                {
                    // moveset lleno → el ViewModel preguntará qué borrar
                    resultado.MovimientoAprendidoDirectamente = false;
                    // NO añadimos aún; lo añade el ViewModel tras la elección
                }
            }

            // ── 2. EVOLUCIÓN ──────────────────────────────────────────────
            var evo = pokedex?.Evolucion;
            bool debeEvolucionar = evo != null
                && evo.Metodo == "subida_nivel"
                && evo.Nivel.HasValue
                && existente.Nivel >= evo.Nivel.Value;

            if (debeEvolucionar)
            {
                // Buscar datos de la evolución en Pokédex por nombre
                var datosEvo = _pokedexRepo.ObtenerPorNombre(evo.Nombre);

                existente.Nombre        = evo.Nombre;
                existente.PokemonId     = datosEvo?.numero_pokedex ?? existente.PokemonId;
                existente.numero_pokedex = datosEvo?.numero_pokedex ?? existente.numero_pokedex;
                existente.TipoPrincipal  = datosEvo?.TipoPrincipal  ?? existente.TipoPrincipal;
                existente.TipoSecundario = datosEvo?.TipoSecundario ?? existente.TipoSecundario;
                if (datosEvo?.EstadisticasBase != null)
                    existente.CurrentHp = datosEvo.EstadisticasBase.Ps;

                resultado.Evoluciono    = true;
                resultado.NombreEvolucion = evo.Nombre;

                // Movimiento de nivel 1 de la evolución que aún no tenga
                var movEvo = datosEvo?.Movimientos
                    ?.FirstOrDefault(m =>
                        m.Metodo == "nivel" &&
                        m.Nivel  == 1 &&
                        !existente.MoveSet.Contains(m.Nombre));

                if (movEvo != null)
                {
                    resultado.MovimientoEvolucion = movEvo.Nombre;

                    if (existente.MoveSet.Count < 4)
                    {
                        existente.MoveSet.Add(movEvo.Nombre);
                        resultado.MovimientoEvolucionDirectamente = true;
                    }
                    else
                    {
                        resultado.MovimientoEvolucionDirectamente = false;
                    }
                }
            }

            // Persistir (si el moveset estaba lleno el MoveSet aún no tiene el nuevo;
            // el ViewModel lo añadirá y llamará a AplicarMovimiento())
            _repo.UpdatePokemon(existente);
            RecalcularPokemon(userId);

            resultado.Pokemon = existente;
            return resultado;
        }

        // ----------------------------------------------------------------
        // Llamado por el ViewModel cuando el usuario elige qué borrar
        // ----------------------------------------------------------------
        public void AplicarMovimiento(PokemonUser pokemon, int indiceABorrar, string movimientoNuevo)
        {
            if (indiceABorrar >= 0 && indiceABorrar < pokemon.MoveSet.Count)
                pokemon.MoveSet[indiceABorrar] = movimientoNuevo;
            else
                pokemon.MoveSet.Add(movimientoNuevo);   // fallback seguro

            _repo.UpdatePokemon(pokemon);
        }

        // ----------------------------------------------------------------
        private void RecalcularPokemon(string userId)
        {
            var pokes = _repo.GetPokemonsByUser(userId);
            var user  = _userRepo.GetUserById(userId);
            user.Pokemon = pokes.Count;
            _userRepo.UpdateUser(user);
        }

        public int ContarPorTipo(string userId, string tipo)
            => _repo.CountByType(userId, tipo);
    }
}
