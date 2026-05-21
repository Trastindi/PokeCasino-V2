using PK_Proyect.Services;
using PK_Proyect.Models;

public class CasinoService
{
    private readonly UserService _userService = new UserService();

    public void ActualizarFichas(User user, int nuevasFichas)
    {
        user.FichasCasino = nuevasFichas;
        _userService.UpdateUser(user);
    }
}
