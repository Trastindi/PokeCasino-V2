using MongoDB.Driver;
using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace PK_Proyect.Repositories
{
    public class HistoricoTiradasRepository
    {
        private readonly IMongoCollection<HistoricoTirada> _collection;

        public HistoricoTiradasRepository()
        {
            var client = new MongoClient("mongodb+srv://marcosemiliorodriguezmartin_db_user:gDfjWHYHIqMJ346V@pokecasino.asaeily.mongodb.net");
            var db = client.GetDatabase("PokemonDB");
            _collection = db.GetCollection<HistoricoTirada>("HistoricoTiradas");
        }

        // ---------------------------------------------------------
        // INSERTAR UNA TIRADA
        // ---------------------------------------------------------
        public void RegistrarTirada(HistoricoTirada tirada)
        {
            _collection.InsertOne(tirada);
        }

        // ---------------------------------------------------------
        // OBTENER TODO EL HISTORIAL DE UN USUARIO
        // ---------------------------------------------------------
        public List<HistoricoTirada> ObtenerPorUsuario(string userId)
        {
            return _collection
                .Find(t => t.UserId == userId)
                .SortByDescending(t => t.Fecha)
                .ToList();
        }

        // ---------------------------------------------------------
        // OBTENER HISTORIAL POR USUARIO Y ZONA
        // ---------------------------------------------------------
        public List<HistoricoTirada> ObtenerPorZona(string userId, string zona)
        {
            return _collection
                .Find(t => t.UserId == userId && t.Zona == zona)
                .SortByDescending(t => t.Fecha)
                .ToList();
        }

        // ---------------------------------------------------------
        // OBTENER HISTORIAL POR TIPO DE TIRADA (single/multi)
        // ---------------------------------------------------------
        public List<HistoricoTirada> ObtenerPorTipo(string userId, string tipo)
        {
            return _collection
                .Find(t => t.UserId == userId && t.TipoTirada == tipo)
                .SortByDescending(t => t.Fecha)
                .ToList();
        }

        // ---------------------------------------------------------
        // OBTENER LAS ÚLTIMAS N TIRADAS
        // ---------------------------------------------------------
        public List<HistoricoTirada> ObtenerUltimas(string userId, int cantidad)
        {
            return _collection
                .Find(t => t.UserId == userId)
                .SortByDescending(t => t.Fecha)
                .Limit(cantidad)
                .ToList();
        }

        // ---------------------------------------------------------
        // BORRAR HISTORIAL COMPLETO DE UN USUARIO
        // ---------------------------------------------------------
        public void BorrarHistorial(string userId)
        {
            _collection.DeleteMany(t => t.UserId == userId);
        }
    }
}
