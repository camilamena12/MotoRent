//indicamos que use SQL server como proveedor de base de datos
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MotoRentContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MotoRentConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();