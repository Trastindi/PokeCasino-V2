using MongoDB.Driver;
using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    public class MensajeRepository
    {
        private readonly IMongoCollection<Mensaje> _collection;

        public MensajeRepository()
        {
            var context = new MongoDbContext();
            _collection = context.GetCollection<Mensaje>("mensajes");
        }

        public List<Mensaje> GetMensajesByUser(string userId)
        {
            return _collection
                .Find(m => m.UserId == userId)
                .ToList();
        }
    }
}
