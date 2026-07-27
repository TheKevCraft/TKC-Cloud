using Microsoft.EntityFrameworkCore;
using TKC_Cloud.Data;

namespace TKC_Cloud.Services.Initialization;

public class DatabaseInitializer : IApplicationInitializer
{
    private readonly AppDbContext _db;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(AppDbContext db, ILogger<DatabaseInitializer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing database ...");

        await _db.Database.MigrateAsync();

        if (!await _db.Users.AnyAsync())
        {
            _logger.LogInformation("Creating default administrator ...");

            _db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("adminPass01!"),
                Role = "Admin"
            });

            await _db.SaveChangesAsync();
        }

        _logger.LogInformation("Database initialized.");
    }
}