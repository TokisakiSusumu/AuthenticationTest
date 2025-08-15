using System.Collections.Concurrent;

namespace WebUI.Components;

public interface ITokenService
{
    void StoreToken(string sessionId, string token);
    string? GetToken(string sessionId);
    void RemoveToken(string sessionId);
}

public class TokenService : ITokenService
{
    private readonly ConcurrentDictionary<string, (string token, DateTime expiry)> _tokens = new();

    public void StoreToken(string sessionId, string token)
    {
        // Auto-expire after 8 hours
        _tokens[sessionId] = (token, DateTime.UtcNow.AddHours(8));

        // Clean up old tokens
        CleanupExpiredTokens();
    }

    public string? GetToken(string sessionId)
    {
        if (_tokens.TryGetValue(sessionId, out var tokenData))
        {
            if (tokenData.expiry > DateTime.UtcNow)
                return tokenData.token;

            _tokens.TryRemove(sessionId, out _);
        }
        return null;
    }

    public void RemoveToken(string sessionId)
    {
        _tokens.TryRemove(sessionId, out _);
    }

    private void CleanupExpiredTokens()
    {
        var expired = _tokens.Where(x => x.Value.expiry < DateTime.UtcNow).Select(x => x.Key);
        foreach (var key in expired)
            _tokens.TryRemove(key, out _);
    }
}