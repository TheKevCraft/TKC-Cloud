using Microsoft.EntityFrameworkCore;
using TKC_Cloud.Data;
using TKC_Cloud.Services;
using TKC_Cloud.Services.FileService;
using TKC_Cloud.Services.Cleanup;
using TKC_Cloud.Services.Storage;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Kestrel Configuration

// Remove the request body size limit to allow very large file uploads.
// WARNING: In production, consider setting a reasonble limit to prevent abuse.
builder.WebHost.ConfigureKestrel(optins =>
{
    optins.Limits.MaxRequestBodySize = null; // unbegrenzt
    optins.ListenAnyIP(7296, listen => listen.UseHttps());
});

#endregion

#region CORS Configuration

// Development CORS policy for local frontend.
// Allows the client to access the API and read the Content-Disposition
// header, which is required for file downloads.
builder.Services.AddCors(options =>
{
    options.AddPolicy("dev", policy =>
    {
        policy
            .WithOrigins("https://localhost:7151")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Content-Disposition")
            .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

#endregion

#region Swagger / OpenAPI

// Register Swagger services to generate API documentation.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#endregion

#region Controllers

// Enable MVC controllers.
builder.Services.AddControllers();

#endregion

#region Authtication & Authorzation

// Configure JWT Bearer authentication.
// Tokens are signed with the secret defined in appsettings.json:
//
// "Jwt": {
//    "Key": "your-very-secret-key"
// }
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"]
                     ?? throw new InvalidOperationException(
                        "JWT signing key is missing. Configure 'Jwt:Key' in appsettings.json.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Disable issuer and audience for local development.
            // Enable these checks in production for better security.
            ValidateIssuer = false,
            ValidateAudience = false,

            // Ensure the token has not expired.
            ValidateLifetime = true,

            // Validate the token signature.
            ValidateIssuerSigningKey = true,

            // Secret key used to sign and validate tokens.
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Enable authorization services.
builder.Services.AddAuthorization();

#endregion

#region Database


// Read connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Safety check: fail fast if missing
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing an appsettings.json");
}

// Register Entity Framework Core with SQLite using configuration-based connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

#endregion

#region Configuration Binding

// Bind the "Storage" section to StorageSettings.
//
// Example:
// "Storage": {
//   "Provider": "Local"
// }
builder.Services.Configure<StorageSettings>(
    builder.Configuration.GetSection("Storage"));

// Bind upload cleanup settings.
//
// Exapmle:
// "UploadCleanup": {
//   "ExpirationHoures": 24
// }
builder.Services.Configure<UploadCleanupSettings>(
    builder.Configuration.GetSection("UploadCleanup"));

#endregion

#region Application Services

// Authentication and token services.
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<FileAccessTokenService>();

// Main file handling service.
builder.Services.AddScoped<IFileService, FileService>();

#endregion

#region Storage Providers

// Register all available storage providers.
// The active provider is selected dynamically by StorageServiceFactory.
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<MinioStorageService>();

// Register the factory that select the correct provider based on configuration.
builder.Services.AddScoped<IStorageServiceFactory, StorageServiceFactory>();

// Register IStorageService as the selected provider.
// Example:
// Provider = "Local" -> LocalStorageService
// Provider = "MinIO" -> MinioStorageService
builder.Services.AddScoped<IStorageService>(serviceProvider =>
{
    var factory = serviceProvider.GetRequiredService<IStorageServiceFactory>();
    return factory.Create();
});

#endregion

#region Background Services

// Periodically removes expired or incomplete uploads.
builder.Services.AddHostedService<UploadCleanupService>();

// sync data between to storage providers on application start
builder.Services.AddHostedService<StorageMigrationService>();

#endregion

var app = builder.Build();

#region Middleware Pipeline

// Enable Swagger UI only in development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirect Http request to Https.
app.UseHttpsRedirection();

// Enable endpoint routing.
app.UseRouting();

// Apply development CORS policy.
app.UseCors("dev");

// Enable authentication and authorization.
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints.
app.MapControllers();

#endregion

#region Database Initialization

// Create a default admin user if the database is empty.
// This is useful for local development and testing.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Apply pending migration automatically.
    // Recommended for development; in production, migrations should
    // typically be executed as part of deployment.
    await dbContext.Database.MigrateAsync();

    // Create a default administrator if no users exist.
    if (!await dbContext.Users.AnyAsync())
    {
        dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("adminPass01!"),
            Role = "Admin"
        });

        await dbContext.SaveChangesAsync();
    }
}

#endregion

// Start the web application.
app.Run();