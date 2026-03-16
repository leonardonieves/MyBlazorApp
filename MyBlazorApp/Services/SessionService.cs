using MyBlazorApp.Models;
using System.Collections.Concurrent;

namespace MyBlazorApp.Services;

/// <summary>
/// Servicio Singleton para mantener el estado de autenticación del usuario por circuito
/// </summary>
public class SessionService
{
    // Diccionario thread-safe para mantener usuarios por ID de circuito
    private readonly ConcurrentDictionary<string, User> _activeUsers = new();
    private string? _currentCircuitId;

    public void SetCircuitId(string circuitId)
    {
        _currentCircuitId = circuitId;
    }

    public User? GetCurrentUser()
    {
        if (string.IsNullOrEmpty(_currentCircuitId))
            return null;

        _activeUsers.TryGetValue(_currentCircuitId, out var user);
        return user;
    }

    public void SetCurrentUser(User? user)
    {
        if (string.IsNullOrEmpty(_currentCircuitId))
            return;

        if (user == null)
        {
            _activeUsers.TryRemove(_currentCircuitId, out _);
        }
        else
        {
            _activeUsers[_currentCircuitId] = user;
        }
    }

    public bool IsAuthenticated()
    {
        return GetCurrentUser() != null;
    }

    public void Logout()
    {
        if (!string.IsNullOrEmpty(_currentCircuitId))
        {
            _activeUsers.TryRemove(_currentCircuitId, out _);
        }
    }
}
