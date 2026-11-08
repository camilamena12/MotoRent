
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.Data;
using MotoRent.Api.Models;
using MotoRent.Api.Services;
using System.Security.Claims;
using System.Text.Json;

namespace MotoRent.Api.Controllers;

[ApiController]
[Route("api/ordenes-pago")]
[Authorize(Roles = "Cliente")]
public class OrdenesPagoController : ControllerBase
{
    private readonly MotoRentContext _context;
    private readonly MercadoPagoService _mercadoPagoService;
    private readonly ILogger<OrdenesPagoController> _logger;

    public OrdenesPagoController(
        MotoRentContext context,
        MercadoPagoService mercadoPagoService,
        ILogger<OrdenesPagoController> logger)
    {
        _context = context;
        _mercadoPagoService = mercadoPagoService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CrearOrden(
        [FromBody] SolicitudPago solicitud)
    {
        // 1. Validar cantidad de días
        if (solicitud.CantidadDias <= 0)
            return BadRequest("La cantidad de días debe ser mayor a cero.");

        // 2. Identificar al cliente autenticado
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(usuarioId))
            return Unauthorized();

        var clienteId = await _context.Users
            .Where(u => u.Id == usuarioId)
            .Select(u => u.ClienteId)
            .FirstOrDefaultAsync();

        if (clienteId == null)
            return Forbid();

        // 3. Verificar que el alquiler le pertenezca
        var alquiler = await _context.Alquileres
            .FirstOrDefaultAsync(a =>
                a.Id == solicitud.AlquilerId &&
                a.ClienteId == clienteId.Value);

        if (alquiler == null)
            return NotFound("No se encontró el alquiler del cliente.");

        if (alquiler.Estado != "Activo")
            return BadRequest("El alquiler no está activo.");

        if (alquiler.FechaInicio.Date > DateTime.Today)
            return BadRequest("El alquiler todavía no comenzó.");

        // Buscar si ya existe una orden pendiente
        var ordenEnCurso = await _context.OrdenesPago
            .Where(o =>
                o.AlquilerId == alquiler.Id &&
                (o.Estado == "Iniciada" ||
                 o.Estado == "EsperandoPago"))
            .OrderByDescending(o => o.FechaCreacion)
            .FirstOrDefaultAsync();
        // 5. Buscar días que no tengan pagos registrados
        var pagosRegistrados = await _context.Pagos
            .Where(p =>
                p.AlquilerId == alquiler.Id &&
                (p.Estado == "Pendiente" ||
                 p.Estado == "Confirmado"))
            .Select(p => p.DiaAlquilado)
            .ToListAsync();

        var diasOcupados = pagosRegistrados
            .Select(d => d.Date)
            .ToHashSet();

        var diasPendientes = new List<DateTime>();

        for (var dia = alquiler.FechaInicio.Date;
             dia <= DateTime.Today;
             dia = dia.AddDays(1))
        {
            if (!diasOcupados.Contains(dia))
                diasPendientes.Add(dia);
        }

        if (solicitud.CantidadDias > diasPendientes.Count)
            return BadRequest(
                $"Solo hay {diasPendientes.Count} días disponibles para pagar."
            );

        var diasSeleccionados = diasPendientes
            .Take(solicitud.CantidadDias)
            .ToList();

        // El importe siempre se calcula desde SQL Server
        var importeTotal =
            diasSeleccionados.Count * alquiler.PrecioPorDia;
        // Reutilizar una orden pendiente si corresponde al mismo pago
        if (ordenEnCurso != null)
        {
            var mismosDias =
                ordenEnCurso.DiasSeleccionadosJson ==
                JsonSerializer.Serialize(diasSeleccionados);

            if (ordenEnCurso.Estado == "EsperandoPago" &&
                !string.IsNullOrWhiteSpace(ordenEnCurso.CheckoutUrl) &&
                ordenEnCurso.CantidadDias == diasSeleccionados.Count &&
                ordenEnCurso.ImporteTotal == importeTotal &&
                mismosDias)
            {
                return Ok(new
                {
                    ordenId = ordenEnCurso.Id,
                    importeTotal = ordenEnCurso.ImporteTotal,
                    checkoutUrl = ordenEnCurso.CheckoutUrl,
                    mensaje = "Continuando una orden pendiente."
                });
            }

            return Conflict(new
            {
                mensaje = "Ya existe una orden pendiente para este alquiler. " +
                          "Debemos verificar su estado antes de generar otra."
            });
        }

        // 6. Registrar la operación en MotoRent
        var orden = new OrdenPago
        {
            AlquilerId = alquiler.Id,
            CantidadDias = diasSeleccionados.Count,
            ImporteTotal = importeTotal,
            DiasSeleccionadosJson =
                JsonSerializer.Serialize(diasSeleccionados),
            ReferenciaExterna =
                $"MOTORENT-{Guid.NewGuid():N}",
            Estado = "Iniciada"
        };

        _context.OrdenesPago.Add(orden);
        await _context.SaveChangesAsync();

        // 7. Solicitar el checkout a Mercado Pago
        try
        {
            var (ordenMpId, checkoutUrl) =
                await _mercadoPagoService.CrearOrdenAsync(
                    importeTotal,
                    orden.ReferenciaExterna
                );

            orden.MercadoPagoOrdenId = ordenMpId;
            orden.CheckoutUrl = checkoutUrl;
            orden.Estado = "EsperandoPago";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                ordenId = orden.Id,
                importeTotal,
                checkoutUrl,
                mensaje = "Orden creada correctamente."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al crear la orden {OrdenId} en Mercado Pago.",
                orden.Id
            );

            orden.Estado = "Error";
            await _context.SaveChangesAsync();

            return StatusCode(502, new
            {
                mensaje = "No se pudo crear la orden en Mercado Pago."
            });
        }
    }

    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> CancelarOrden(int id)
    {
        // Identificar al usuario que inició sesión
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(usuarioId))
            return Unauthorized();

        var clienteId = await _context.Users
            .Where(u => u.Id == usuarioId)
            .Select(u => u.ClienteId)
            .FirstOrDefaultAsync();

        if (clienteId == null)
            return Forbid();

        // Buscar la orden en MotoRent
        var orden = await _context.OrdenesPago
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null)
            return NotFound("La orden no existe.");

        // Verificar que pertenezca al cliente autenticado
        var esSuAlquiler = await _context.Alquileres
            .AnyAsync(a =>
                a.Id == orden.AlquilerId &&
                a.ClienteId == clienteId.Value);

        if (!esSuAlquiler)
            return NotFound("La orden no existe.");

        if (orden.Estado == "Cancelada")
            return Ok(new { mensaje = "La orden ya está cancelada." });

        if (orden.Estado != "EsperandoPago" ||
            string.IsNullOrWhiteSpace(orden.MercadoPagoOrdenId))
        {
            return Conflict(new
            {
                mensaje = "Esta orden no se puede cancelar desde aquí."
            });
        }

        try
        {
            // Primero cancelar la orden en Mercado Pago
            await _mercadoPagoService.CancelarOrdenAsync(
                orden.MercadoPagoOrdenId
            );

            // Solo después actualizar nuestra base de datos
            orden.Estado = "Cancelada";
            orden.CheckoutUrl = null;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Orden cancelada correctamente. Podés elegir otra cantidad de días."
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex,
                "Mercado Pago no permitió cancelar la orden {OrdenId}.",
                orden.Id
            );

            return Conflict(new
            {
                mensaje = "No se pudo cancelar la orden. Es posible que el pago ya esté procesándose. Debemos verificar su estado."
            });
        }
    }

    [HttpGet("pendiente/{alquilerId:int}")]
    public async Task<IActionResult> ObtenerOrdenPendiente(int alquilerId)
    {
        // Identificar al cliente que inició sesión
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(usuarioId))
            return Unauthorized();

        var clienteId = await _context.Users
            .Where(u => u.Id == usuarioId)
            .Select(u => u.ClienteId)
            .FirstOrDefaultAsync();

        if (clienteId == null)
            return Forbid();

        // Comprobar que el alquiler sea de este cliente
        var esSuAlquiler = await _context.Alquileres
            .AnyAsync(a =>
                a.Id == alquilerId &&
                a.ClienteId == clienteId.Value);

        if (!esSuAlquiler)
            return NotFound("El alquiler no existe.");

        // Buscar una orden pendiente de ese alquiler
        var orden = await _context.OrdenesPago
            .Where(o =>
                o.AlquilerId == alquilerId &&
                (o.Estado == "EsperandoPago" ||
                 o.Estado == "Iniciada"))
            .OrderByDescending(o => o.FechaCreacion)
            .FirstOrDefaultAsync();

        if (orden == null)
        {
            return Ok(new
            {
                hayOrdenPendiente = false
            });
        }

        return Ok(new
        {
            hayOrdenPendiente = true,
            ordenId = orden.Id,
            cantidadDias = orden.CantidadDias,
            importeTotal = orden.ImporteTotal,
            checkoutUrl = orden.CheckoutUrl,
            estado = orden.Estado
        });
    }


    [HttpGet("{id:int}/estado")]
    public async Task<IActionResult> ConsultarEstadoOrden(int id)
    {
        // 1. Identificar al cliente autenticado
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(usuarioId))
            return Unauthorized();

        var clienteId = await _context.Users
            .Where(u => u.Id == usuarioId)
            .Select(u => u.ClienteId)
            .FirstOrDefaultAsync();

        if (clienteId == null)
            return Forbid();

        // 2. Buscar la orden en MotoRent
        var orden = await _context.OrdenesPago
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null)
            return NotFound("La orden no existe.");

        // 3. Verificar que pertenezca al cliente
        var esSuAlquiler = await _context.Alquileres
            .AnyAsync(a =>
                a.Id == orden.AlquilerId &&
                a.ClienteId == clienteId.Value);

        if (!esSuAlquiler)
            return NotFound("La orden no existe.");

        if (string.IsNullOrWhiteSpace(orden.MercadoPagoOrdenId))
            return Conflict(new
            {
                mensaje = "La orden todavía no tiene un ID de Mercado Pago."
            });

        try
        {
            // 4. Consultar directamente a Mercado Pago
            var resultado = await _mercadoPagoService.ConsultarOrdenAsync(
                orden.MercadoPagoOrdenId
            );

            // 5. Comprobar que corresponda a nuestra orden
            var datosCoinciden =
                resultado.ReferenciaExterna == orden.ReferenciaExterna &&
                resultado.ImporteTotal == orden.ImporteTotal &&
                resultado.Moneda == "ARS";

            if (!datosCoinciden)
            {
                return Conflict(new
                {
                    mensaje = "Los datos de Mercado Pago no coinciden con la orden de MotoRent."
                });
            }

            // 6. Verificar si el pago fue acreditado
            var aprobado =
                resultado.Estado == "processed" &&
                resultado.Detalle == "accredited" &&
                resultado.TotalPagado == orden.ImporteTotal;

            return Ok(new
            {
                ordenId = orden.Id,
                estadoMercadoPago = resultado.Estado,
                detalle = resultado.Detalle,
                importeTotal = orden.ImporteTotal,
                aprobado
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new
            {
                mensaje = "No se pudo consultar el estado en Mercado Pago."
            });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(503, new
            {
                mensaje = "Mercado Pago tardó demasiado en responder."
            });
        }
    }

    [HttpPost("{id:int}/confirmar")]
    public async Task<IActionResult> ConfirmarOrden(int id)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(usuarioId))
            return Unauthorized();

        var clienteId = await _context.Users
            .Where(u => u.Id == usuarioId)
            .Select(u => u.ClienteId)
            .FirstOrDefaultAsync();

        if (clienteId == null)
            return Forbid();

        // Buscar solo órdenes que pertenezcan al cliente
        var orden = await _context.OrdenesPago
            .FirstOrDefaultAsync(o =>
                o.Id == id &&
                _context.Alquileres.Any(a =>
                    a.Id == o.AlquilerId &&
                    a.ClienteId == clienteId.Value));

        if (orden == null)
            return NotFound("La orden no existe.");

        // Evitar registrar dos veces el mismo pago
        if (orden.Estado == "Confirmada")
            return Ok(new { mensaje = "Este pago ya fue registrado." });

        if (orden.Estado != "EsperandoPago" ||
            string.IsNullOrWhiteSpace(orden.MercadoPagoOrdenId))
            return Conflict(new
            {
                mensaje = "La orden no está disponible para confirmar."
            });

        // Consultar Mercado Pago: nunca confiar en el frontend
        var resultado = await _mercadoPagoService.ConsultarOrdenAsync(
            orden.MercadoPagoOrdenId
        );

        var datosCorrectos =
            resultado.ReferenciaExterna == orden.ReferenciaExterna &&
            resultado.ImporteTotal == orden.ImporteTotal &&
            resultado.TotalPagado == orden.ImporteTotal &&
            resultado.Moneda == "ARS";

        var acreditado =
            resultado.Estado == "processed" &&
            resultado.Detalle == "accredited";

        if (!acreditado)
            return Ok(new
            {
                aprobado = false,
                mensaje = "El pago todavía no fue acreditado."
            });

        if (!datosCorrectos)
            return Conflict(new
            {
                mensaje = "Los datos del pago no coinciden con la orden."
            });

        // Recuperar las fechas que se eligieron al crear la orden
        var dias = JsonSerializer.Deserialize<List<DateTime>>(
            orden.DiasSeleccionadosJson
        );

        if (dias == null ||
            dias.Count == 0 ||
            dias.Count != orden.CantidadDias ||
            dias.Select(d => d.Date).Distinct().Count() != dias.Count)
            return Conflict(new
            {
                mensaje = "Los días de la orden son inválidos."
            });

        // Guardar los días y la confirmación en una transacción
        await using var transaccion =
            await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

        await _context.Entry(orden).ReloadAsync();

        if (orden.Estado == "Confirmada")
            return Ok(new { mensaje = "Este pago ya fue registrado." });

        if (orden.Estado != "EsperandoPago")
            return Conflict(new
            {
                mensaje = "La orden cambió de estado."
            });

        // Evitar duplicar días pagados o pendientes
        var pagosExistentes = await _context.Pagos
            .Where(p =>
                p.AlquilerId == orden.AlquilerId &&
                (p.Estado == "Pendiente" ||
                 p.Estado == "Confirmado"))
            .Select(p => p.DiaAlquilado)
            .ToListAsync();

        var diasOcupados = pagosExistentes
            .Select(d => d.Date)
            .ToHashSet();

        if (dias.Any(d => diasOcupados.Contains(d.Date)))
            return Conflict(new
            {
                mensaje = "Hay días que ya tienen un pago registrado. Requiere revisión."
            });

        var importePorDia = orden.ImporteTotal / dias.Count;

        foreach (var dia in dias)
        {
            _context.Pagos.Add(new Pago
            {
                AlquilerId = orden.AlquilerId,
                DiaAlquilado = dia.Date,
                FechaPago = DateTime.Now,
                Importe = importePorDia,
                MetodoPago = "Mercado Pago",
                Estado = "Confirmado",
                ReferenciaExterna = orden.MercadoPagoOrdenId
            });
        }

        orden.Estado = "Confirmada";
        orden.CheckoutUrl = null;

        await _context.SaveChangesAsync();
        await transaccion.CommitAsync();

        return Ok(new
        {
            aprobado = true,
            mensaje = "Pago confirmado correctamente.",
            cantidadDias = dias.Count,
            importeTotal = orden.ImporteTotal
        });
    }


}
