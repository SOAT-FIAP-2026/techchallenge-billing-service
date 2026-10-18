using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Fiap.TechChallenge.Billing.Api.Configurations
{
    public static class JWTConfig
    {
        public static void AddJWTConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var key = Encoding.UTF8.GetBytes(JwtSecretResolver.Resolve(configuration));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Permite HTTP em ambiente interno de cluster
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(configuration["Jwt:Issuer"]),
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "fiap-tech-challenge",
                    ValidateAudience = !string.IsNullOrWhiteSpace(configuration["Jwt:Audience"]),
                    ValidAudience = configuration["Jwt:Audience"] ?? "fiap-api",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization();
        }
    }
}
