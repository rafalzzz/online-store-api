using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OnlineStoreAPI.Configuration
{
    public class SwaggerConfiguration
    {
        public static void ConfigureSwagger(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Online store API", Version = "v1" });

            options.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Cookie,
                Name = "access_token",
                Scheme = "Bearer",
                Description = "JWT Authorization using the Bearer scheme. Enter your token in the text input below.",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "cookieAuth"
                        }
                    },
                    new string[] {}
                }
            });
        }
    }
}