using Microsoft.EntityFrameworkCore;
using TKC_Cloud.Data;
using TKC_Cloud.Services;
using TKC_Cloud.Services.Storage;
using TKC_Cloud.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(optins =>
{
    optins.Limits.MaxRequestBodySize = null; // unbegrenzt
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("dev", policy =>
    {
        policy
            .WithOrigins("http://localhost:5223")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Content-Disposition");
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Controller aktivieren
builder.Services.AddControllers();

// Authorization
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// Datenbank (erstmal SQLite fur local)
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=cloud.db"));

// Service Configuration
builder.Services.Configure<StorageSettings>(
    builder.Configuration.GetSection("Storage"));
builder.Services.Configure<UploadCleanupSettings>(
    builder.Configuration.GetSection("UploadCleanup"));

// Services registrieren
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<FileAccessTokenService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddSingleton<IStorageService, LocalStorageService>();

// BackgroundServices
builder.Services.AddHostedService<UploadCleanupService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!context.Users.Any())
    {
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("adminPass01!"),
            Role = "Admin"
        });

        await context.SaveChangesAsync();
    }
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("dev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();