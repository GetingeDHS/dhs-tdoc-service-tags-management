using TagManagement.Api.Services;
using TagManagement.Core.Interfaces;
using TagManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add data layer services (includes DbContext and repositories)
builder.Services.AddDataServices(builder.Configuration);

// Add application services
builder.Services.AddScoped<ITagService, TagService>();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Swagger/OpenAPI for better API documentation
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
