using MyBlazorApp.Models;

namespace MyBlazorApp.Services;

/// <summary>
/// Servicio para mantener el estado de autenticación del usuario
/// </summary>
public class SessionService
{
    private User? _currentUser;

    public User? GetCurrentUser()
    {
        return _currentUser;
    }

    public void SetCurrentUser(User? user)
    {
        _currentUser = user;
    }

    public bool IsAuthenticated()
    {
        return _currentUser != null;
    }

    public void Logout()
    {
        _currentUser = null;
    }
}
