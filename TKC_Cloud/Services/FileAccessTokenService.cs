using TKC_Cloud.Models;

namespace TKC_Cloud.Services;

public class FileAccessTokenService
{
    private readonly Dictionary<string, FileAccessToken> _tokens = new();

    public string CreateToken(Guid fileId, Guid userId, int seconds = 60)
    {
        var token = new FileAccessToken
        {
            FileId = fileId,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddSeconds(seconds)
        };

        _tokens[token.Token] = token;
        return token.Token;
    }

    public FileAccessToken? Validate(string token)
    {
        if (!_tokens.TryGetValue(token, out var entry))
            return null;

        if (entry.ExpiresAt < DateTime.UtcNow)
        {
            _tokens.Remove(token);
            return null;
        }

        return entry;
    }
}