using Microsoft.AspNetCore.Http.HttpResults;
using WalterApi.Core;
using WalterApi.Infrastucture;
using WalterApi.Infrastucture.Initializers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Create connection sting
string connStr = builder.Configuration.GetConnectionString("DefaultConnection");
// Database context
builder.Services.AddDbContext(connStr);

// Add Core services
builder.Services.AddCoreServices();

// Add Infrastructure Service
builder.Services.AddInfrastructureService();

// Add Mapping
builder.Services.AddMapping();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


await UsersAndRolesInitializer.SeedUsersAndRoles(app);
app.Run();
