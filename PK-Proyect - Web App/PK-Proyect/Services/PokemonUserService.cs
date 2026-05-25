using PK_Proyect.Models;
using PK_Proyect.Repositories;
using PK_Proyect.Services;

namespace PK_Proyect.Services
{
    public class PokemonUserService
    {
        private readonly PokemonUserRepository _repo;
        private readonly UserRepository _userRepo;

        public PokemonUserService()
        {
            _repo = new PokemonUserRepository();
            _userRepo = new UserRepository();
        }

        public PokemonUser ObtenerPokemon(string userId, int pokemonId, string nombre, string tipo1, string tipo2, int currentHp)
        {
            var existente = _repo.GetPokemon(userId, pokemonId);
            var user = _userRepo.GetUserById(userId);

            if (existente == null)
            {
                var nuevo = new PokemonUser
                {
                    UserId        = userId,
                    Username      = user.Username,
                    PokemonId     = pokemonId,
                    Nombre        = nombre,
                    TipoPrincipal = tipo1,
                    TipoSecundario = tipo2,
                    Nivel         = 1,
                    Cantidad      = 1,
                    FechaObtenido = DateTime.Now,
                    HiddenPowerSeed  = Random.Shared.Next(0, 16),
                    HiddenPowerPower = (Random.Shared.Next(31, 71) + Random.Shared.Next(31, 71)) / 2,
                    CurrentHp     = currentHp,
                };

                _repo.InsertPokemon(nuevo);
                RecalcularPokemon(userId);
                return nuevo;
            }

            // Documento existente: subir cantidad y nivel
            existente.Cantidad++;
            existente.Nivel++;
            existente.Username = user.Username;

            // Backfill: rellenar campos que faltan en documentos antiguos
            if (existente.HiddenPowerSeed == 0 && existente.HiddenPowerPower == 0)
            {
                existente.HiddenPowerSeed  = Random.Shared.Next(0, 16);
                existente.HiddenPowerPower = (Random.Shared.Next(31, 71) + Random.Shared.Next(31, 71)) / 2;
            }

            if (existente.CurrentHp == 0)
                existente.CurrentHp = currentHp;

            if (existente.MoveSet == null || existente.MoveSet.Count == 0)
                existente.MoveSet = new List<string>();

            _repo.UpdatePokemon(existente);
            RecalcularPokemon(userId);
            return existente;
        }

        private void RecalcularPokemon(string userId)
        {
            var pokes = _repo.GetPokemonsByUser(userId);
            int totalUnicos = pokes.Count;

            var user = _userRepo.GetUserById(userId);
            user.Pokemon = totalUnicos;
            _userRepo.UpdateUser(user);
        }

        public int ContarPorTipo(string userId, string tipo)
        {
            return _repo.CountByType(userId, tipo);
        }
    }
}
