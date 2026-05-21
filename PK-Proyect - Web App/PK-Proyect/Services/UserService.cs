using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.Generic;

namespace PK_Proyect.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User GetUserById(string id) => _userRepository.GetUserById(id);

        public User GetUserByUsername(string username) => _userRepository.GetUserByUsername(username);

        public User GetUserByEmail(string email) => _userRepository.GetUserByEmail(email);

        public void CreateUser(User user) => _userRepository.CreateUser(user);

        public void UpdateUser(User user) => _userRepository.UpdateUser(user);

        public List<User> GetAllUsers() => _userRepository.GetAllUsers();
    }
}
