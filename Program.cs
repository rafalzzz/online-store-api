using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineStoreAPI.Entities;
using OnlineStoreAPI.Services;
using OnlineStoreAPI.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserService, UserService>();

string connectionStringEnv = "ONLINE_STORE_CONNECTION_STRING";
EnvironmentHelper.EnsureConnectionStringVariableExists(connectionStringEnv);
var connectionString = Environment.GetEnvironmentVariable(connectionStringEnv);

builder.Services.AddDbContext<OnlineStoreDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
