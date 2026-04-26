using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using TKC_Cloud.Data;

namespace TKC_Cloud.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // Create Register-Token (to register a new User)
    public async Task<string> CreateRegisterTokenAsync()
    {
        var token = new RegisterToken
        {
            Id = Guid.NewGuid(),
            Token = Guid.NewGuid().ToString(),
            IsUsed = false,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RegisterTokens.Add(token);
        await _context.SaveChangesAsync();

        return token.Token;
    }

    // Register (the User set EMail/Pass with Register-Token)
    public async Task<User> RegisterAsync(string email, string password, string registerToken)
    {
        if (await _context.Users.AnyAsync(u => u.Email == email))
            throw new Exception("User already exists");

        var token = await _context.RegisterTokens
            .FirstOrDefaultAsync(t => t.Token == registerToken);

        if (token == null)
            throw new Exception("Invalid register token");

        if (token.IsUsed)
            throw new Exception("Token already used");

        if (token.ExpiresAt.HasValue && token.ExpiresAt < DateTime.UtcNow)
            throw new Exception("Token expired");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "User"
        };

        token.IsUsed = true;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    // Login
    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return GenerateJwt(user);
    }

    public async Task<(string accessToken, string refreshToken)?> LoginWithRefreshAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        // Access Token
        var accessToken = GenerateJwt(user);

        // Refresh Token
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = Guid.NewGuid().ToString(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(30) // 30 Days accessable
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return (accessToken, refreshToken.Token);
    }
    
    public async Task<string?> RefreshAccessTokenAsync(string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken && !r.IsRevoked);

        if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow)
            return null;

        // New Access Token generated
        return GenerateJwt(tokenEntity.User!);
    }

    // Generate Tokens
    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}