using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using System.Text.Json;
using WebUI.Components;

namespace WebUI.Components;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _http;
    private readonly ProtectedSessionStorage _sessionStorage;

    public CustomAuthStateProvider(
        ITokenService tokenService,
        IHttpContextAccessor httpContextAccessor,
        HttpClient http,
        ProtectedSessionStorage sessionStorage)
    {
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
        _http = http;
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var sessionId = GetOrCreateSessionId();
        if (sessionId == null)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var token = _tokenService.GetToken(sessionId);
        if (string.IsNullOrEmpty(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return new AuthenticationState(new ClaimsPrincipal(
            new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
    }

    public async Task Login(string token)
    {
        var sessionId = GetOrCreateSessionId();
        if (sessionId != null)
        {
            _tokenService.StoreToken(sessionId, token);
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }

    public async Task Logout()
    {
        var sessionId = GetOrCreateSessionId();
        if (sessionId != null)
        {
            _tokenService.RemoveToken(sessionId);
            _http.DefaultRequestHeaders.Authorization = null;
        }

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    private string? GetOrCreateSessionId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        var sessionId = context.Request.Cookies["AuthSession"];
        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
            context.Response.Cookies.Append("AuthSession", sessionId, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                IsEssential = true
                // No Expires = session cookie (deleted when browser closes)
            });
        }
        return sessionId;
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            return keyValuePairs?.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!))
                ?? Enumerable.Empty<Claim>();
        }
        catch
        {
            return Enumerable.Empty<Claim>();
        }
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}