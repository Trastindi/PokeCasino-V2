using MongoDB.Driver;
using MongoDB.Bson;
using PK_Proyect.Models;

namespace PK_Proyect.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository()
        {
            var client = new MongoClient("mongodb+srv://marcosemiliorodriguezmartin_db_user:gDfjWHYHIqMJ346V@pokecasino.asaeily.mongodb.net");
            var database = client.GetDatabase("PokemonDB");
            _users = database.GetCollection<User>("Users");
        }

        public User GetUserById(string id)
        {
            return _users.Find(u => u.Id == id).FirstOrDefault();
        }

        public User GetUserByUsername(string username)
        {
            username = username.Trim();

            var filter = Builders<User>.Filter.Regex(
                u => u.Username,
                new BsonRegularExpression($"^{username}$", "i")
            );

            return _users.Find(filter).FirstOrDefault();
        }

        public User GetUserByEmail(string email)
        {
            email = email.Trim();

            var filter = Builders<User>.Filter.Regex(
                u => u.Correo,
                new BsonRegularExpression($"^{email}$", "i")
            );

            return _users.Find(filter).FirstOrDefault();
        }

        public bool Exists(string username)
        {
            username = username.Trim();

            var filter = Builders<User>.Filter.Regex(
                u => u.Username,
                new BsonRegularExpression($"^{username}$", "i")
            );

            return _users.Find(filter).Any();
        }

        public void CreateUser(User user)
        {
            _users.InsertOne(user);
        }

        public void UpdateUser(User user)
        {
            _users.ReplaceOne(u => u.Id == user.Id, user);
        }

        public List<User> GetAllUsers()
        {
            return _users.Find(_ => true).ToList();
        }
    }
}
