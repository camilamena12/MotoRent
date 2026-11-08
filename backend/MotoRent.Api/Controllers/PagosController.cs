
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.Data;
using MotoRent.Api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MotoRent.Api.Services;

namespace MotoRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PagosController : ControllerBase
{
    private readonly MotoRentContext _context;
    private readonly MercadoPagoService _mercadoPagoService;

    public PagosController(
        MotoRentContext context,
        MercadoPagoService mercadoPagoService)
    {
        _context = context;
        _mercadoPagoService = mercadoPagoService;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ObtenerPagos()
    {
        var pagos = await _context.Pagos
     .Include(p => p.Alquiler)
         .ThenInclude(a => a!.Cliente)
     .Include(p => p.Alquiler)
         .ThenInclude(a => a!.Moto)
     .ToListAsync();

        return Ok(pagos);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ObtenerPagoPorId(int id)
    {
        var pago = await _context.Pagos
            .Include(p => p.Alquiler)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pago == null)
        {
            return NotFound();
        }

        return Ok(pago);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> RegistrarPago(Pago pago)
    {
        var alquiler = await _context.Alquileres
            .FindAsync(pago.AlquilerId);

        if (alquiler == null)
        {
            return BadRequest("El alquiler indicado no existe.");
        }

        if (alquiler.Estado != "Activo")
        {
            return BadRequest("No se pueden registrar pagos para un alquiler inactivo.");
        }

        // No permitir pagar días futuros
        if (pago.DiaAlquilado.Date > DateTime.Today)
        {
            return BadRequest(
                "No se pueden pagar días futuros."
            );
        }

        // El importe lo determina MotoRent, no el cliente
        pago.Importe = alquiler.PrecioPorDia;

        if (pago.DiaAlquilado.Date < alquiler.FechaInicio.Date)
        {
            return BadRequest("El día pagado no puede ser anterior al inicio del alquiler.");
        }

        if (alquiler.FechaDevolucion.HasValue &&
            pago.DiaAlquilado.Date > alquiler.FechaDevolucion.Value.Date)
        {
            return BadRequest("El día pagado no puede ser posterior a la devolución.");
        }

        var pagoExistente = await _context.Pagos.AnyAsync(p =>
    p.AlquilerId == pago.AlquilerId &&
    p.DiaAlquilado.Date == pago.DiaAlquilado.Date &&
    (p.Estado == "Pendiente" || p.Estado == "Confirmado"));
        if (pagoExistente)
        {
            return BadRequest("Ya existe un pago registrado para ese día de alquiler.");
        }

        pago.FechaPago = DateTime.Now;
        pago.DiaAlquilado = pago.DiaAlquilado.Date;
        pago.Estado = "Pendiente";
        pago.ReferenciaExterna = null;

        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();

        return Ok(pago);
    }

    // Calcular el importe de varios días pendientes
    [HttpPost("calcular")]
    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> CalcularPago(SolicitudPago solicitud)
    {
        if (solicitud.CantidadDias <= 0)
        {
            return BadRequest("La cantidad de días debe ser mayor que cero.");
        }

        var alquiler = await _context.Alquileres
            .FirstOrDefaultAsync(a => a.Id == solicitud.AlquilerId);

        if (alquiler == null)
        {
            return NotFound("El alquiler no existe.");
        }

        // Identificar al usuario que inició sesión
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(usuarioId))
        {
            return Unauthorized();
        }

        // Obtener el cliente asociado a ese usuario
        var clienteId = await _context.Users
            .Where(u => u.Id == usuarioId)
            .Select(u => u.ClienteId)
            .FirstOrDefaultAsync();

        if (clienteId == null)
        {
            return Forbid();
        }

        // Verificar que el alquiler pertenezca al cliente
        if (alquiler.ClienteId != clienteId.Value)
        {
            return Forbid();
        }

        if (alquiler.Estado != "Activo")
        {
            return BadRequest("El alquiler no está activo.");
        }

        var pagosRegistrados = await _context.Pagos
            .Where(p => p.AlquilerId == solicitud.AlquilerId &&
                        (p.Estado == "Pendiente" ||
                         p.Estado == "Confirmado"))
            .Select(p => p.DiaAlquilado)
            .ToListAsync();

        var diasOcupados = pagosRegistrados
            .Select(fecha => fecha.Date)
            .ToHashSet();

        var diasPendientes = new List<DateTime>();

        for (var dia = alquiler.FechaInicio.Date;
             dia <= DateTime.Today;
             dia = dia.AddDays(1))
        {
            if (!diasOcupados.Contains(dia))
            {
                diasPendientes.Add(dia);
            }
        }

        if (solicitud.CantidadDias > diasPendientes.Count)
        {
            return BadRequest(
                $"Solo hay {diasPendientes.Count} días disponibles para pagar."
            );
        }

        var diasSeleccionados = diasPendientes
            .Take(solicitud.CantidadDias)
            .ToList();

        var importeTotal = diasSeleccionados.Count * alquiler.PrecioPorDia;

        return Ok(new
        {
            alquilerId = alquiler.Id,
            cantidadDias = diasSeleccionados.Count,
            precioPorDia = alquiler.PrecioPorDia,
            importeTotal,
            diasSeleccionados
        });
    }

}
