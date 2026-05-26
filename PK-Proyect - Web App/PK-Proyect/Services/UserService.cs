using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.Generic;

namespace PK_Proyect.Services
{
    public class UserService
    {
        private readonly UserRepository _repo = new();

        public User GetUserById(string id)       => _repo.GetUserById(id);
        public User GetUserByUsername(string u)  => _repo.GetUserByUsername(u);
        public User GetUserByEmail(string e)     => _repo.GetUserByEmail(e);
        public void CreateUser(User user)        => _repo.CreateUser(user);
        public void UpdateUser(User user)        => _repo.UpdateUser(user);
        public List<User> GetAllUsers()          => _repo.GetAllUsers();
    }
}
