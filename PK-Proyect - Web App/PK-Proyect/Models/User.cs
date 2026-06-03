using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PK_Proyect.Repositories;

namespace PK_Proyect.Models
{
    public class UserMessage
    {
        public int    MessageId  { get; set; }
        public int    ForeignId  { get; set; }
        public string Text       { get; set; }
        public DateTime Date     { get; set; }
        public bool   Read       { get; set; }
    }

    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Nombre   { get; set; }
        public string Apellido { get; set; }
        public string Username { get; set; }

        private string _email;

        public string Correo
        {
            get => _email;
            set
            {
                if (!IsValidEmail(value))
                    throw new ArgumentException("Email inválido");
                _email = value;
            }
        }

        /// <summary>Cantidad total de Pokémon del usuario (suma de duplicados).</summary>
        public int Pokemon { get; set; } = 0;

        public string   Password     { get; set; }
        public DateTime Birthdate    { get; set; }
        public string   Role         { get; set; }

        /// <summary>PokeDólares: moneda del juego.</summary>
        public int Pokes        { get; set; } = 0;
        public int FichasCasino { get; set; } = 0;

        public List<string>      Medallas { get; set; } = new List<string>();
        public List<UserMessage> Messages { get; set; } = new List<UserMessage>();

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailRegex);
        }
    }
}
