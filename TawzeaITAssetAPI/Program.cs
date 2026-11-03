using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TawzeaITAssetAPI.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// To name enum values as string rather than int in json response
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));


builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Allow the static web frontend (VS Code Live Server) to call the API
builder.Services.AddCors(options =>
    options.AddPolicy("WebFrontend", policy =>
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("WebFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
