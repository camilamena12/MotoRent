
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.Data;
using MotoRent.Api.Models;

namespace MotoRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlquileresController : ControllerBase
{
    private readonly MotoRentContext _context;

    public AlquileresController(MotoRentContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerAlquileres()
    {
        var alquileres = await _context.Alquileres
            .Include(a => a.Cliente)
            .Include(a => a.Moto)
            .ToListAsync();

        return Ok(alquileres);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerAlquilerPorId(int id)
    {
        var alquiler = await _context.Alquileres
            .Include(a => a.Cliente)
            .Include(a => a.Moto)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (alquiler == null)
        {
            return NotFound();
        }

        return Ok(alquiler);
    }
    //aca agregamos el registro de alquileres, validando que exista el cliente y la moto y que la moto este disponible 

    [HttpPost]
    public async Task<IActionResult> CrearAlquiler(Alquiler alquiler)
    {
        var cliente = await _context.Clientes
            .FindAsync(alquiler.ClienteId);

        if (cliente == null)
        {
            return BadRequest("El cliente indicado no existe.");
        }

        var moto = await _context.Motos
            .FindAsync(alquiler.MotoId);

        if (moto == null)
        {
            return BadRequest("La moto indicada no existe.");
        }

        if (!moto.Disponible)
        {
            return BadRequest("La moto no está disponible para alquilar.");
        }

        if (alquiler.FechaInicio == default)
        {
            return BadRequest("La fecha de inicio es obligatoria.");
        }

        alquiler.FechaDevolucion = null;
        alquiler.PrecioPorDia = moto.PrecioPorDia;
        alquiler.Estado = "Activo";
        //calcular fecha de compra
        if (alquiler.OpcionCompra)
        {
            alquiler.FechaHabilitacionCompra =
                alquiler.FechaInicio.AddMonths(6);
        }
        else
        {
            alquiler.FechaHabilitacionCompra = null;
        }

        _context.Alquileres.Add(alquiler);
        moto.Disponible = false;

        await _context.SaveChangesAsync();

        return Ok(alquiler);
    }
    // endpoint para devolucion de moto 

    [HttpPut("{id}/devolver")]
    public async Task<IActionResult> DevolverMoto(int id)
    {
        var alquiler = await _context.Alquileres
            .Include(a => a.Moto)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (alquiler == null)
        {
            return NotFound("El alquiler no existe.");
        }

        if (alquiler.Estado != "Activo")
        {
            return BadRequest("El alquiler no está activo.");
        }

        alquiler.FechaDevolucion = DateTime.Now;
        alquiler.Estado = "Finalizado";

        if (alquiler.Moto != null)
        {
            alquiler.Moto.Disponible = true;
        }

        await _context.SaveChangesAsync();

        return Ok(alquiler);
    }

    // consulta si un alquiler ya esta habilitado para ejercer a opcion de compra 

    [HttpGet("{id}/opcion-compra")]
    public async Task<IActionResult> ConsultarOpcionCompra(int id)
    {
        var alquiler = await _context.Alquileres
            .FirstOrDefaultAsync(a => a.Id == id);

        if (alquiler == null)
        {
            return NotFound("El alquiler no existe.");
        }

        if (!alquiler.OpcionCompra)
        {
            return Ok(new
            {
                TieneOpcionCompra = false,
                Habilitada = false,
                Mensaje = "Este alquiler no tiene opción de compra."
            });
        }

        var habilitada =
            alquiler.FechaHabilitacionCompra.HasValue &&
            DateTime.Today >= alquiler.FechaHabilitacionCompra.Value.Date;

        return Ok(new
        {
            TieneOpcionCompra = true,
            Habilitada = habilitada,
            FechaHabilitacionCompra = alquiler.FechaHabilitacionCompra,
            Mensaje = habilitada
                ? "La opción de compra ya está habilitada."
                : "La opción de compra todavía no está habilitada."
        });
    }



}
