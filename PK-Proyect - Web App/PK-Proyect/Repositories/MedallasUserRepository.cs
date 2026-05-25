using MongoDB.Driver;
using PK_Proyect.Models;

namespace PK_Proyect.Repositories
{
    public class MedallasUserRepository
    {
        private readonly IMongoCollection<MedallasUser> _collection;

        public MedallasUserRepository()
        {
            var client = new MongoClient("mongodb+srv://marcosemiliorodriguezmartin_db_user:gDfjWHYHIqMJ346V@pokecasino.asaeily.mongodb.net");
            var db = client.GetDatabase("PokemonDB");
            _collection = db.GetCollection<MedallasUser>("MedallasUser");
        }

        public List<MedallasUser> GetByUser(string userId)
        {
            return _collection.Find(m => m.UserId == userId).ToList();
        }

        public MedallasUser GetMedalla(string userId, string tipo)
        {
            return _collection.Find(m => m.UserId == userId && m.Tipo == tipo).FirstOrDefault();
        }

        public void InsertMedalla(MedallasUser medalla)
        {
            _collection.InsertOne(medalla);
        }
    }
}
// NO SE USA ACTUALMENTE