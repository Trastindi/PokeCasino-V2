using MongoDB.Driver;
using PK_Proyect.Models;

namespace PK_Proyect.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly IMongoCollection<User> _collection;

        public AdminRepository()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("PokemonDB");
            _collection = db.GetCollection<User>("Users");
        }

        public List<User> GetAllUsers()
        {
            return _collection.Find(_ => true).ToList();
        }

        public void DeleteUser(string id)
        {
            _collection.DeleteOne(u => u.Id == id);
        }

        public void UpdateUser(User user)
        {
            _collection.ReplaceOne(u => u.Id == user.Id, user);
        }

        public void ChangeRole(string id, string newRole)
        {
            var update = Builders<User>.Update.Set(u => u.Role, newRole);
            _collection.UpdateOne(u => u.Id == id, update);
        }

        public void ResetPassword(string id, string newHashedPassword)
        {
            var update = Builders<User>.Update.Set(u => u.Password, newHashedPassword);
            _collection.UpdateOne(u => u.Id == id, update);
        }
    }
}
