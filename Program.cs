using OnlineStoreAPI.Entities;
using OnlineStoreAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserService, UserService>();

var connectionString = builder.Configuration.GetConnectionString("OnlineStoreDbContext");
builder.Services.AddDbContext<OnlineStoreDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
