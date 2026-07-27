using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TKC_Cloud.Extensions;

internal static class CloudAuthentication
{
    internal static IServiceCollection AddCloudAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") //builder.Configuration["Jwt:Key"]
                             ?? throw new InvalidOperationException(
                                "JWT signing key is missing. Configure 'Jwt_Key' in .env.");

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

        services.AddAuthorization();

        return services;
    }
}