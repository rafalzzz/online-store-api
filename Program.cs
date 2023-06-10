using Microsoft.EntityFrameworkCore;
using NLog.Web;
using FluentValidation;
using OnlineStoreAPI.Authentication;
using OnlineStoreAPI.Authorization;
using OnlineStoreAPI.Configuration;
using OnlineStoreAPI.Entities;
using OnlineStoreAPI.Helpers;
using OnlineStoreAPI.Middleware;
using OnlineStoreAPI.Models;
using OnlineStoreAPI.Requests;
using OnlineStoreAPI.Services;
using OnlineStoreAPI.Validations;
using OnlineStoreAPI.Variables;

var builder = WebApplication.CreateBuilder(args);

// DB context
EnvironmentHelper.EnsureConnectionStringVariableExists(EnvironmentVariables.ConnectionString);
var connectionString = Environment.GetEnvironmentVariable(EnvironmentVariables.ConnectionString);

builder.Services.AddDbContext<OnlineStoreDbContext>(options => options.UseNpgsql(connectionString));

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Memory cache
// Only for testing purposes
builder.Services.AddDistributedMemoryCache();

// Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

var resetPasswordSettings = builder.Configuration.GetSection("ResetPasswordSettings");
builder.Services.Configure<ResetPasswordSettings>(resetPasswordSettings);

builder.Host.UseNLog();
new CorsConfiguration(builder.Services);
new SessionConfiguration(builder.Services, jwtSettings);
new AuthenticationConfiguration(builder.Services, jwtSettings);
new AuthorizationConfiguration(builder.Services);
builder.Services.AddSwaggerGen(SwaggerConfiguration.ConfigureSwagger);

// Additional Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddTransient<IJwtService, JwtService>();
builder.Services.AddTransient<IAccessTokenService, AccessTokenService>();
builder.Services.AddTransient<IResetPasswordTokenService, ResetPasswordTokenService>();
builder.Services.AddTransient<IEmailService, EmailService>();

// Validators
builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordRequestValidator>();
builder.Services.AddScoped<IValidator<ChangePasswordRequest>, ChangePasswordRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();

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
app.UseSession();

app.MapControllers();

app.Run();
