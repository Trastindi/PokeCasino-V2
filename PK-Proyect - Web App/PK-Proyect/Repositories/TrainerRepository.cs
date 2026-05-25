using MongoDB.Driver;
using PK_Proyect.Models;

namespace PK_Proyect.Repositories
{
    public class TrainerRepository
    {
        private readonly IMongoCollection<User> _collection;

        public TrainerRepository()
        {
            _collection = MongoDbContext.GetCollection<User>("Users");
        }

        public void Insert(User user)
        {
            _collection.InsertOne(user);
        }
    }
}
