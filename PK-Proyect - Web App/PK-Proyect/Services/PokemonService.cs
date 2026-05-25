//using MongoDB.Driver;
//using PK_Proyect.Models;
//using System.Collections.Generic;
//using PK_Proyect.Services;
//using MongoDB.Bson;
//using PK_Proyect.Repositories;


//namespace PK_Proyect.Services
//{
//    public static class PokemonService
//    {
//        private static readonly IMongoCollection<Pokemon> _pokemon;

//        static PokemonService()
//        {
//            var client = new MongoClient("mongodb+srv://marcosemiliorodriguezmartin_db_user:gDfjWHYHIqMJ346V@pokecasino.asaeily.mongodb.net");
//            var db = client.GetDatabase("PokemonDB");
//            _pokemon = db.GetCollection<Pokemon>("Pokedex");
//        }

//        public static List<Pokemon> GetAll()
//        {
//            return _pokemon.Find(_ => true).ToList();
//        }

//        public static Pokemon GetById(int id)
//        {
//            return _pokemon.Find(p => p.Numero_pokedex == id).FirstOrDefault();
//        }

//        public static void InsertMany(List<Pokemon> lista)
//        {
//            _pokemon.InsertMany(lista);
//        }
//    }
//}

//// NO SE USA ACTUALMENTE PARA NADA