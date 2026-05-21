using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    public interface IAdminRepository
    {
        List<User> GetAllUsers();
        void DeleteUser(string id);
        void UpdateUser(User user);
    }
}
