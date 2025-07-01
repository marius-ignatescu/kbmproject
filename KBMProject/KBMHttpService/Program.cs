using KBMGrpcService.Protos;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KBM API", Version = "v1" });
});

// Add gPRC clients
builder.Services.AddGrpcClient<UserProtoService.UserProtoServiceClient>(g =>
{
    g.Address = new Uri("http://kbmgrpcservice:5001");
});

builder.Services.AddGrpcClient<OrganizationProtoService.OrganizationProtoServiceClient>(g =>
{
    g.Address = new Uri("http://kbmgrpcservice:5001");
});

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "KBM API V1");
        c.RoutePrefix = string.Empty;
    });
//}

app.UseAuthorization();
app.MapControllers();
app.Run();