using Microsoft.EntityFrameworkCore;
using NLog.Web;
using FluentValidation;
using OnlineStoreAPI.Entities;
using OnlineStoreAPI.Services;
using OnlineStoreAPI.Helpers;
using OnlineStoreAPI.Models;
using OnlineStoreAPI.Requests;
using OnlineStoreAPI.Validations;
using OnlineStoreAPI.Configuration;
using OnlineStoreAPI.Middleware;
using OnlineStoreAPI.Variables;

var builder = WebApplication.CreateBuilder(args);

// DB context
EnvironmentHelper.EnsureConnectionStringVariableExists(EnvironmentVariables.ConnectionString);
var connectionString = Environment.GetEnvironmentVariable(EnvironmentVariables.ConnectionString);

builder.Services.AddDbContext<OnlineStoreDbContext>(options => options.UseNpgsql(connectionString));

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

builder.Host.UseNLog();
new CorsConfiguration(builder.Services);
new AuthenticationConfiguration(jwtSettings, builder.Services);
builder.Services.AddSwaggerGen(SwaggerConfiguration.ConfigureSwagger);

// Additional Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Validators
builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Online store API");
    });
}

app.UseCors(Cors.CorsPolicy);

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<CookieAuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
