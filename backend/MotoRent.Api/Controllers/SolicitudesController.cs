
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.Data;
using MotoRent.Api.Models;
using MotoRent.Api.Services;

namespace MotoRent.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador")]
public class SolicitudesController : ControllerBase
{
    private readonly MotoRentContext _context;
    private readonly RenaperService _renaperService;

    public SolicitudesController(
        MotoRentContext context,
        RenaperService renaperService)
    {
        _context = context;
        _renaperService = renaperService;
    }


    // Consultar todas las solicitudes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolicitudAlquiler>>> GetSolicitudes()
    {
        return await _context.SolicitudesAlquiler
            .Include(s => s.Cliente)
            .Include(s => s.Moto)
            .ToListAsync();
    }

    
    // Consultar una solicitud por ID
    [HttpGet("{id}")]
    public async Task<ActionResult<SolicitudAlquiler>> GetSolicitud(int id)
    {
        var solicitud = await _context.SolicitudesAlquiler
            .Include(s => s.Cliente)
            .Include(s => s.Moto)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud == null)
            return NotFound("No se encontró la solicitud.");

        return solicitud;
    }


    // Registrar una solicitud desde la página pública
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CrearSolicitud(
        [FromBody] CrearSolicitudRequest request)
    {
        // 1. Comprobar la fecha

        var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Argentina/Buenos_Aires"
        );

        var hoyArgentina = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            zonaArgentina
        ).Date;

        if (!request.FechaInicioSolicitada.HasValue ||
            request.FechaInicioSolicitada.Value.Date < hoyArgentina)

        {
            return BadRequest("La fecha de inicio no puede ser anterior a hoy.");
        }

        // 2. Comprobar que la moto esté disponible
        var moto = await _context.Motos.FindAsync(request.MotoId);

        if (moto == null)
            return BadRequest("La moto no existe.");

        if (!moto.Disponible)
            return BadRequest("La moto no está disponible.");

        // 3. Validar el DNI con la API RENAPER
        bool dniValido;

        try
        {
            dniValido = await _renaperService.ValidarDniAsync(
                request.DNI,
                request.Nombre.Trim(),
                request.Apellido.Trim()
            );
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, new
            {
                mensaje = "El servicio de validación de DNI no está disponible."
            });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(503, new
            {
                mensaje = "El servicio de validación de DNI tardó demasiado."
            });
        }
        catch (System.Text.Json.JsonException)
        {
            return StatusCode(502, new
            {
                mensaje = "El servicio de DNI devolvió una respuesta inválida."
            });
        }

        if (!dniValido)
        {
            return BadRequest(new
            {
                mensaje = "El DNI no pudo validarse con los datos ingresados."
            });
        }

        // 4. Buscar al cliente por DNI
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.DNI == request.DNI);

        if (cliente == null)
        {
            cliente = new Cliente
            {
                Nombre = request.Nombre.Trim(),
                Apellido = request.Apellido.Trim(),
                DNI = request.DNI,
                Telefono = request.Telefono,
                Email = request.Email
            };
        }
        else if (
            !string.Equals(cliente.Nombre.Trim(), request.Nombre.Trim(),
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(cliente.Apellido.Trim(), request.Apellido.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new
            {
                mensaje = "No se pudo registrar la solicitud. Consultá con el administrador."
            });
        }
        // 4.1. Verificar si el cliente ya tiene una moto alquilada
        if (cliente.Id != 0)
        {
            var tieneAlquilerActivo = await _context.Alquileres
                .AnyAsync(a =>
                    a.ClienteId == cliente.Id &&
                    a.Estado == "Activo");

            if (tieneAlquilerActivo)
            {
                return BadRequest(new
                {
                    mensaje = "No podés solicitar otra moto porque ya tenés un alquiler activo."
                });
            }
        }
        // 5. Crear la solicitud pendiente
        var solicitud = new SolicitudAlquiler
        {
            Cliente = cliente,
            MotoId = request.MotoId,
            FechaInicioSolicitada = request.FechaInicioSolicitada.Value,
            OpcionCompra = request.OpcionCompra,
            Estado = "Pendiente",
            FechaSolicitud = DateTime.Now
        };

        _context.SolicitudesAlquiler.Add(solicitud);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetSolicitud),
            new { id = solicitud.Id },
            new
            {
                solicitud.Id,
                solicitud.Estado,
                mensaje = "Solicitud registrada correctamente."
            }
        );
    }
    // Aprobar una solicitud y generar el alquiler
    [HttpPut("{id}/aprobar")]
    public async Task<IActionResult> AprobarSolicitud(int id)
    {
        var solicitud = await _context.SolicitudesAlquiler
            .Include(s => s.Moto)
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud == null)
        {
            return NotFound("La solicitud no existe.");
        }

        if (solicitud.Estado != "Pendiente")
        {
            return BadRequest("La solicitud ya fue procesada.");
        }

        if (solicitud.Moto == null || !solicitud.Moto.Disponible)
        {
            return BadRequest("La moto ya no está disponible.");
        }
        // Verificar que el cliente no tenga otra moto alquilada
        var tieneAlquilerActivo = await _context.Alquileres
            .AnyAsync(a =>
                a.ClienteId == solicitud.ClienteId &&
                a.Estado == "Activo");

        if (tieneAlquilerActivo)
        {
            return BadRequest(
                "El cliente ya tiene una moto alquilada. " +
                "Debe finalizar ese alquiler antes de obtener otra."
            );
        }
        var alquiler = new Alquiler
        {
            ClienteId = solicitud.ClienteId,
            MotoId = solicitud.MotoId,
            FechaInicio = solicitud.FechaInicioSolicitada,
            FechaDevolucion = null,
            PrecioPorDia = solicitud.Moto.PrecioPorDia,
            OpcionCompra = solicitud.OpcionCompra,
            Estado = "Activo",
            FechaHabilitacionCompra = solicitud.OpcionCompra
                ? solicitud.FechaInicioSolicitada.AddMonths(6)
                : null
        };

        solicitud.Estado = "Aprobada";
        solicitud.Moto.Disponible = false;

        _context.Alquileres.Add(alquiler);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Mensaje = "Solicitud aprobada y alquiler creado correctamente.",
            Solicitud = solicitud,
            Alquiler = alquiler
        });
    }


    
    // Rechazar una solicitud de alquiler
    [HttpPut("{id}/rechazar")]
    public async Task<IActionResult> RechazarSolicitud(int id)
    {
        var solicitud = await _context.SolicitudesAlquiler
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud == null)
        {
            return NotFound("La solicitud no existe.");
        }

        if (solicitud.Estado != "Pendiente")
        {
            return BadRequest("La solicitud ya fue procesada.");
        }

        solicitud.Estado = "Rechazada";

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Mensaje = "La solicitud fue rechazada correctamente.",
            Solicitud = solicitud
        });
    }


}
