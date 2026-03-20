using Blazored.LocalStorage;
using MyBlazorApp.Shared.Models;
using System.Text.Json;

namespace MyBlazorApp.Web.Services;

public class AuthStateService
{
    private readonly ILocalStorageService _localStorage;
    private readonly ApiService _api;
    private AuthUserData? _currentUser;

    public event Action? AuthStateChanged;
    public event Action? OnAuthStateChanged
    {
        add => AuthStateChanged += value;
        remove => AuthStateChanged -= value;
    }

    public AuthUserData? CurrentUser => _currentUser;
    public bool IsAuthenticated { get; private set; }

    public AuthStateService(ILocalStorageService localStorage, ApiService api)
    {
        _localStorage = localStorage;
        _api = api;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("authToken");
        IsAuthenticated = !string.IsNullOrEmpty(token);
        return IsAuthenticated;
    }

    public async Task<AuthUserData?> GetCurrentUserAsync()
    {
        if (_currentUser != null)
            return _currentUser;

        var userJson = await _localStorage.GetItemAsStringAsync("currentUser");
        if (string.IsNullOrEmpty(userJson))
            return null;

        _currentUser = JsonSerializer.Deserialize<AuthUserData>(userJson);
        return _currentUser;
    }

    public async Task<AuthUserData?> GetUserAsync() => await GetCurrentUserAsync();

    public AuthUserData? GetUser() => _currentUser;

    public async Task LoginAsync(AuthUserData user, string token)
    {
        _currentUser = user;
        IsAuthenticated = true;
        await _localStorage.SetItemAsStringAsync("authToken", token);
        await _localStorage.SetItemAsStringAsync("currentUser", JsonSerializer.Serialize(user));
        _api.SetAuthToken(token);
        AuthStateChanged?.Invoke();
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        IsAuthenticated = false;
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("currentUser");
        _api.ClearAuthToken();
        AuthStateChanged?.Invoke();
    }

    public async Task InitializeAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            _api.SetAuthToken(token);
            IsAuthenticated = true;
            await GetCurrentUserAsync();
        }
    }

    public bool IsAdmin()
    {
        return _currentUser?.RoleName?.ToLower() == "admin";
    }
}