
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.Data;
using MotoRent.Api.Models;

namespace MotoRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagosController : ControllerBase
{
    private readonly MotoRentContext _context;

    public PagosController(MotoRentContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerPagos()
    {
        var pagos = await _context.Pagos
            .Include(p => p.Alquiler)
            .ToListAsync();

        return Ok(pagos);
    }

    [HttpGet("{id}")]
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

        if (pago.Importe <= 0)
        {
            return BadRequest("El importe debe ser mayor que cero.");
        }

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
            p.DiaAlquilado.Date == pago.DiaAlquilado.Date);

        if (pagoExistente)
        {
            return BadRequest("Ya existe un pago registrado para ese día de alquiler.");
        }

        pago.FechaPago = DateTime.Now;
        pago.DiaAlquilado = pago.DiaAlquilado.Date;

        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();

        return Ok(pago);
    }
}
