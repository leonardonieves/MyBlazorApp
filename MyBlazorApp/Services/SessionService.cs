using MyBlazorApp.Models;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace MyBlazorApp.Services;

/// <summary>
/// Servicio Scoped para mantener el estado de autenticación del usuario
/// Cada circuito de Blazor Server tiene su propia instancia
/// </summary>
public class SessionService
{
    private User? _currentUser;
    private string? _circuitId;

    public string? CircuitId => _circuitId;

    public void SetCircuitId(string circuitId)
    {
        _circuitId = circuitId;
    }

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

/// <summary>
/// Servicio Singleton que mantiene un mapa de sesiones por circuito
/// Persiste las sesiones cuando el circuito se desconecta temporalmente
/// </summary>
public class SessionStore
{
    private readonly ConcurrentDictionary<string, User> _sessions = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastAccess = new();
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(60);

    public void SetUser(string circuitId, User user)
    {
        _sessions[circuitId] = user;
        _lastAccess[circuitId] = DateTime.UtcNow;
    }

    public User? GetUser(string circuitId)
    {
        if (_sessions.TryGetValue(circuitId, out var user))
        {
            _lastAccess[circuitId] = DateTime.UtcNow;
            return user;
        }
        return null;
    }

    public void RemoveUser(string circuitId)
    {
        _sessions.TryRemove(circuitId, out _);
        _lastAccess.TryRemove(circuitId, out _);
    }

    public void CleanupExpiredSessions()
    {
        var cutoff = DateTime.UtcNow - _sessionTimeout;
        foreach (var kvp in _lastAccess)
        {
            if (kvp.Value < cutoff)
            {
                _sessions.TryRemove(kvp.Key, out _);
                _lastAccess.TryRemove(kvp.Key, out _);
            }
        }
    }
}
