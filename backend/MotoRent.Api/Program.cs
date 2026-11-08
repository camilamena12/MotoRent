//indicamos que use SQL server como proveedor de base de datos

using Microsoft.EntityFrameworkCore;
using MotoRent.Api.Data;

using Microsoft.AspNetCore.Identity;
using MotoRent.Api.Models;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using MotoRent.Api.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresá tu token JWT."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
//conectamos con sql server 
builder.Services.AddDbContext<MotoRentContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MotoRentConnection")));
// Configurar usuarios y roles con Identity
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<MotoRentContext>()
.AddDefaultTokenProviders();

// Configurar autenticación con JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new Exception("No se configuró la clave JWT.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = "MotoRent.Api",
        ValidAudience = "MotoRent.Frontend",

        IssuerSigningKey = new SymmetricSecurityKey(
            Convert.FromBase64String(jwtKey)
        )
    };
});



//configurar CORS, es para que se comunique react con .NET 
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173",
        "http://localhost:5174"
      )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// Conexión con la API RENAPER de nuestros compañeros
builder.Services.AddHttpClient("Renaper", client =>
{
    client.BaseAddress = new Uri("https://146.181.31.154.nip.io/");
    client.Timeout = TimeSpan.FromSeconds(15);
});

// Cliente HTTP para Mercado Pago
builder.Services.AddHttpClient("MercadoPago", client =>
{
    client.BaseAddress = new Uri("https://api.mercadopago.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});



// Registrar el servicio de validación de DNI
builder.Services.AddScoped<RenaperService>();

// Servicio para integrar pagos con MP
builder.Services.AddScoped<MercadoPagoService>();

var app = builder.Build();

// Crear los roles del sistema si todavía no existen
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Administrador", "Cliente" };

    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            var resultado = await roleManager.CreateAsync(
                new IdentityRole(rol)
            );

            if (!resultado.Succeeded)
            {
                throw new Exception(
                    $"No se pudo crear el rol {rol}"
                );
            }
        }
    }
}

// Crear el administrador de prueba solo en desarrollo
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<Usuario>>();

    var email = builder.Configuration["Admin:Email"];
    var password = builder.Configuration["Admin:Password"];

    if (string.IsNullOrWhiteSpace(email) ||
        string.IsNullOrWhiteSpace(password))
    {
        throw new Exception("Faltan las credenciales del administrador.");
    }

    var admin = await userManager.FindByEmailAsync(email);

    if (admin == null)
    {
        admin = new Usuario
        {
            UserName = email,
            Email = email
        };

        var resultado = await userManager.CreateAsync(admin, password);

        if (!resultado.Succeeded)
        {
            throw new Exception(
                string.Join(", ", resultado.Errors.Select(e => e.Description))
            );
        }

        var resultadoRol = await userManager.AddToRoleAsync(
            admin, "Administrador"
        );

        if (!resultadoRol.Succeeded)
        {
            throw new Exception("No se pudo asignar el rol Administrador.");
        }

        Console.WriteLine("Administrador creado correctamente.");
    }
    
      else
        {
            if (!await userManager.IsInRoleAsync(admin, "Administrador"))
            {
                var resultadoRol = await userManager.AddToRoleAsync(
                    admin, "Administrador"
                );

                if (!resultadoRol.Succeeded)
                {
                    throw new Exception("No se pudo asignar el rol Administrador.");
                }

                Console.WriteLine("Rol Administrador asignado correctamente.");
            }
            else
            {
                Console.WriteLine("El administrador ya existe y tiene su rol.");
            }
        }
    }

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("PermitirFrontend");

app.UseAuthentication(); //identifica al usuario 
app.UseAuthorization(); // comprueba que permisos tiene ese usuario

app.MapControllers();

app.Run();


//El archivo Program.cs configura e inicia el backend de MotoRent.
//En él registramos los controladores, Swagger y el contexto de Entity Framework
//Core para conectarnos con SQL Server. También configuramos CORS para permitir que
//nuestro frontend desarrollado en React se comunique con la API, ya que ambos
//se ejecutan en distintos orígenes.
//Finalmente, configuramos el procesamiento de las solicitudes HTTP e iniciamos la aplicación.
