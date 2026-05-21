using MongoDB.Driver;
using PK_Proyect.Models;
using PK_Proyect.Repository;

namespace PK_Proyect.Services
{
    public class UserService
    {
        private readonly UserRepository _repo = new UserRepository();

        public User GetUserById(string id) => _repo.GetUserById(id);
        public User GetUserByUsername(string username) => _repo.GetUserByUsername(username);
        public void CreateUser(User user) => _repo.CreateUser(user);
        public void UpdateUser(User user) => _repo.UpdateUser(user);
        public List<User> GetAllUsers() => _repo.GetAllUsers();

        public User GetUserByEmail(string email) => _repo.GetUserByEmail(email);




    }

}
