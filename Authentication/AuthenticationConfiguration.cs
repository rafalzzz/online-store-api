using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Configuration
{
    public class AuthenticationConfiguration
    {
        public AuthenticationConfiguration(IServiceCollection services, IConfigurationSection jwtSettings)
        {
            var secretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.SecretKey);
            var issuer = jwtSettings[JwtSettingsKeys.Issuer];
            var audience = jwtSettings[JwtSettingsKeys.Audience];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.ContainsKey(CookieNames.AccessToken))
                        {
                            context.Token = context.Request.Cookies[CookieNames.AccessToken];
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.Configure<AuthenticationConfiguration>(jwtSettings);
        }
    }
}