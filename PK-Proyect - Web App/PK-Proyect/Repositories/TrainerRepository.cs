using MongoDB.Driver;
using PK_Proyect.Models;

namespace PK_Proyect.Repositories
{
    public class TrainerRepository
    {
        private readonly IMongoCollection<User> _collection;

        public TrainerRepository()
        {
            var client = new MongoClient("mongodb+srv://marcosemiliorodriguezmartin_db_user:gDfjWHYHIqMJ346V@pokecasino.asaeily.mongodb.net");
            var db = client.GetDatabase("PokemonDB");
            _collection = db.GetCollection<User>("Users");
        }

        public void Insert(User user)
        {
            _collection.InsertOne(user);
        }
    }
}
