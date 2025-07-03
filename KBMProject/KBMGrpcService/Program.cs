using KBMGrpcService.Data;
using KBMGrpcService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.WebHost.UseUrls("http://0.0.0.0:5001");

// Database Context
//var dbHost = Environment.GetEnvironmentVariable("DB_HOST");// ?? "172.24.192.1";
//var dbName = Environment.GetEnvironmentVariable("DB_NAME");
//var dbUser = Environment.GetEnvironmentVariable("DB_USER");
//var dbPassword = Environment.GetEnvironmentVariable("DB_SA_PASSWORD");
//var connectionString = $"Server={dbHost}; Database={dbName}; User ID={dbUser};Password={dbPassword}; TrustServerCertificate=True;";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));

var app = builder.Build();

//Console.WriteLine($"[DEBUG] DB_HOST = {dbHost}");
//Console.WriteLine($"[DEBUG] DB_NAME = {dbName}");
//Console.WriteLine($"[DEBUG] DB_SA_PASSWORD = {dbPassword}");

// Configure the HTTP request pipeline.
app.MapGrpcService<UserService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
