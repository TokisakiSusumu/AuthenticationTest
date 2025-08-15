using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using WebUI.Components;

namespace WebUI.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient http, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> Login(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { email, password });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Token != null)
                {
                    await ((CustomAuthStateProvider)_authStateProvider).Login(result.Token);
                    return true;
                }
            }
        }
        catch { }

        return false;
    }

    public async Task Logout()
    {
        await ((CustomAuthStateProvider)_authStateProvider).Logout();
    }

    private class LoginResponse
    {
        public string Token { get; set; } = "";
    }
}