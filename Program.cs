using Microsoft.EntityFrameworkCore;
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

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

new CorsConfiguration(builder.Services);
new AuthenticationConfiguration(jwtSettings, builder.Services);

// DB context
EnvironmentHelper.EnsureConnectionStringVariableExists(EnvironmentVariables.ConnectionString);
var connectionString = Environment.GetEnvironmentVariable(EnvironmentVariables.ConnectionString);

builder.Services.AddDbContext<OnlineStoreDbContext>(options => options.UseNpgsql(connectionString));

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Additional Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Validators
builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors(Cors.CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<JwtCookieMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
