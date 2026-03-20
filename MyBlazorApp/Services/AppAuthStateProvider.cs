using MyBlazorApp.Models;

namespace MyBlazorApp.Services;

/// <summary>
/// Singleton service that maintains authentication state across all components
/// Uses events to notify components when auth state changes
/// </summary>
public class AppAuthStateProvider
{
    private User? _currentUser;
    
    public User? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;
    
    // Event to notify subscribers when auth state changes
    public event Action? OnAuthStateChanged;

    public void SetUser(User user)
    {
        _currentUser = user;
        NotifyStateChanged();
    }

    public void ClearUser()
    {
        _currentUser = null;
        NotifyStateChanged();
    }

    public User? GetUser()
    {
        return _currentUser;
    }

    private void NotifyStateChanged()
    {
        OnAuthStateChanged?.Invoke();
    }
}
