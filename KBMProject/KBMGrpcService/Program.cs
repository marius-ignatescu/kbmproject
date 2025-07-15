using KBMGrpcService.Data;
using KBMGrpcService.Interceptors;
using KBMGrpcService.Profiles;
using KBMGrpcService.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.WebHost.UseUrls("http://0.0.0.0:5001");

// Database Context
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbUserPassword = Environment.GetEnvironmentVariable("DB_USER_PASSWORD");

if (string.IsNullOrWhiteSpace(dbHost) || string.IsNullOrWhiteSpace(dbUserPassword))
    throw new Exception("Missing database environment variables");

if (string.IsNullOrEmpty(dbUserPassword))
    throw new Exception("Database user password not found");

var connectionString = $"Server={dbHost}; Database={dbName}; User ID={dbUser}; Password={dbUserPassword}; TrustServerCertificate=True;";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));

// Add repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Add exception interceptor
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ExceptionInterceptor>();
    options.EnableDetailedErrors = true;
});

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<UserService>();
app.MapGrpcService<OrganizationService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
