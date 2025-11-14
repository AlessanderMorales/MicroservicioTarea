using MicroservicioTarea.Application.Services;
using MicroservicioTarea.Domain.Entities;
using MicroservicioTarea.Domain.Interfaces;
using MicroservicioTarea.Infrastructure.Persistence;
using MicroservicioTarea.Infrastructure.Repository;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB
builder.Services.AddSingleton<MySqlConnectionSingleton>();

// Repos
builder.Services.AddScoped<IRepository<Tarea>, TareaRepository>();
builder.Services.AddScoped<TareaUsuarioRepository>();

// Services
builder.Services.AddScoped<TareaService>();
builder.Services.AddScoped<TareaUsuarioService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
