
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.Data;
using MotoRent.Api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


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
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ObtenerAlquileres()
    {
        var alquileres = await _context.Alquileres
            .Include(a => a.Cliente)
            .Include(a => a.Moto)
            .ToListAsync();

        return Ok(alquileres);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Administrador")]
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
    // Consultar el estado de cuenta del cliente autenticado
    [HttpGet("mi-estado-cuenta")]
    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> MiEstadoCuenta()
    {
        // Obtener el ID del usuario desde su token JWT
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(usuarioId))
            return Unauthorized();

        // Buscar qué cliente está relacionado con ese usuario
        var clienteId = await _context.Users
            .Where(u => u.Id == usuarioId)
            .Select(u => u.ClienteId)
            .FirstOrDefaultAsync();

        if (clienteId == null)
            return Forbid();

        // Buscar su alquiler activo más reciente
        var alquilerId = await _context.Alquileres
            .Where(a => a.ClienteId == clienteId.Value &&
                        a.Estado == "Activo")
            .OrderByDescending(a => a.FechaInicio)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();

        if (alquilerId == 0)
            return NotFound("No tenés un alquiler activo.");

        // Reutilizar el cálculo de deuda que ya tenemos
        return await ObtenerEstadoCuenta(alquilerId);
    }
   

    //aca agregamos el registro de alquileres, validando que exista el cliente y la moto y que la moto este disponible 

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CrearAlquiler(Alquiler alquiler)
    {
        var cliente = await _context.Clientes
            .FindAsync(alquiler.ClienteId);

        if (cliente == null)
        {
            return BadRequest("El cliente indicado no existe.");
        }

        // Un cliente solo puede tener una moto alquilada a la vez
        var tieneAlquilerActivo = await _context.Alquileres
            .AnyAsync(a =>
                a.ClienteId == alquiler.ClienteId &&
                a.Estado == "Activo");

        if (tieneAlquilerActivo)
        {
            return BadRequest(
                "Este cliente ya tiene una moto alquilada. " +
                "Debe devolverla antes de alquilar otra."
            );
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
    [Authorize(Roles = "Administrador")]
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
    [Authorize(Roles = "Administrador")]
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

    // Consultar los días pagados y la deuda de un alquiler
    [HttpGet("{id}/estado-cuenta")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ObtenerEstadoCuenta(int id)
    {
        var alquiler = await _context.Alquileres
            .Include(a => a.Cliente)
            .Include(a => a.Moto)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (alquiler == null)
        {
            return NotFound("El alquiler no existe.");
        }

        var inicio = alquiler.FechaInicio.Date;
        var fin = alquiler.FechaDevolucion?.Date ?? DateTime.Today;

        // Si el alquiler todavía no comenzó, no hay días adeudados.
        var pagosConfirmados = await _context.Pagos
            .Where(p => p.AlquilerId == id && p.Estado == "Confirmado")
            .Select(p => p.DiaAlquilado)
            .ToListAsync();

        var diasPagados = pagosConfirmados
            .Select(fecha => fecha.Date)
            .ToHashSet();

        var detalle = new List<object>();

        decimal totalAcumulado = 0;
        decimal totalPagado = 0;
        int cantidadDiasPagados = 0;

        // Se cobra cada día desde el inicio hasta hoy o la devolución.
        for (var dia = inicio; dia <= fin; dia = dia.AddDays(1))
        {
            bool pagado = diasPagados.Contains(dia);

            totalAcumulado += alquiler.PrecioPorDia;

            if (pagado)
            {
                totalPagado += alquiler.PrecioPorDia;
                cantidadDiasPagados++;
            }

            detalle.Add(new
            {
                fecha = dia,
                importe = alquiler.PrecioPorDia,
                estado = pagado ? "Pagado" : "Pendiente"
            });
        }

        return Ok(new
        {
            alquilerId = alquiler.Id,
            cliente = alquiler.Cliente == null
                ? null
                : $"{alquiler.Cliente.Nombre} {alquiler.Cliente.Apellido}",
            moto = alquiler.Moto == null
                ? null
                : $"{alquiler.Moto.Marca} {alquiler.Moto.Modelo}",
            precioPorDia = alquiler.PrecioPorDia,
            diasTranscurridos = detalle.Count,
            diasPagados = cantidadDiasPagados,
            diasPendientes = detalle.Count - cantidadDiasPagados,
            totalAcumulado,
            totalPagado,
            deudaTotal = totalAcumulado - totalPagado,
            detalle
        });
    }

}
