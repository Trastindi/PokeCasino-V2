using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    public interface IUserRepository
    {
        User GetUserById(string id);
        User GetUserByUsername(string username);
        User GetUserByEmail(string email);
        bool Exists(string username);
        void CreateUser(User user);
        void UpdateUser(User user);
        List<User> GetAllUsers();
    }
}
