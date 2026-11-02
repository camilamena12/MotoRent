//indicamos que use SQL server como proveedor de base de datos

using Microsoft.EntityFrameworkCore;
using MotoRent.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//conectamos con sql server 
builder.Services.AddDbContext<MotoRentContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MotoRentConnection")));
//configurar CORS, es para que se comunique react con .NET 
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("PermitirFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();


//El archivo Program.cs configura e inicia el backend de MotoRent.
//En él registramos los controladores, Swagger y el contexto de Entity Framework
//Core para conectarnos con SQL Server. También configuramos CORS para permitir que
//nuestro frontend desarrollado en React se comunique con la API, ya que ambos
//se ejecutan en distintos orígenes.
//Finalmente, configuramos el procesamiento de las solicitudes HTTP e iniciamos la aplicación.
