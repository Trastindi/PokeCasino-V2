using System;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PK_Proyect.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Nombre { get; set; }
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

        
        public int Pokemon { get; set; } = 0;

        public string Password { get; set; }
        public DateTime Birthdate { get; set; }
        public string Role { get; set; }
        public int Pokes { get; set; }
        public int FichasCasino { get; set; }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailRegex);
        }
    }
}
