using PK_Proyect.Models;

namespace PK_Proyect.Services
{
    public class CasinoService
    {
        private readonly UserService _userService;

        public CasinoService(UserService userService)
        {
            _userService = userService;
        }

        public void ActualizarFichas(User user, int nuevasFichas)
        {
            user.FichasCasino = nuevasFichas;
            _userService.UpdateUser(user);
        }
    }
}
