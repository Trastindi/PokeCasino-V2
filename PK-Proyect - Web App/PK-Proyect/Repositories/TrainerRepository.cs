using MongoDB.Driver;
using PK_Proyect.Models;

namespace PK_Proyect.Repositories
{
    public class TrainerRepository
    {
        private readonly IMongoCollection<User> _collection;

        public TrainerRepository()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("PokemonDB");
            _collection = db.GetCollection<User>("Users");
        }

        public void Insert(User user)
        {
            _collection.InsertOne(user);
        }
    }
}
