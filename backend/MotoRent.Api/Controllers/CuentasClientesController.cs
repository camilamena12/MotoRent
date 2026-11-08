using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.Data;
using MotoRent.Api.Models;

namespace MotoRent.Api.Controllers;

[ApiController]
[Route("api/cuentas-clientes")]
[Authorize(Roles = "Administrador")]
public class CuentasClientesController : ControllerBase
{
    private readonly MotoRentContext _context;
    private readonly UserManager<Usuario> _userManager;

    public CuentasClientesController(
        MotoRentContext context,
        UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> CrearCuenta(
        [FromBody] CrearCuentaClienteRequest request)
    {
        // 1. Buscar al cliente
        var cliente = await _context.Clientes
            .FindAsync(request.ClienteId);

        if (cliente == null)
            return NotFound("El cliente no existe.");

        // 2. Verificar que tenga un alquiler activo
        var tieneAlquiler = await _context.Alquileres
            .AnyAsync(a =>
                a.ClienteId == cliente.Id &&
                a.Estado == "Activo");

        if (!tieneAlquiler)
            return BadRequest(
                "El cliente no tiene un alquiler activo."
            );

        // 3. Verificar que no tenga una cuenta creada
        var cuentaExistente = await _userManager.Users
            .AnyAsync(u => u.ClienteId == cliente.Id);

        if (cuentaExistente)
            return BadRequest(
                "Este cliente ya tiene una cuenta."
            );

        // 4. Verificar que el correo no esté registrado
        var email = cliente.Email.Trim();

        var usuarioExistente =
            await _userManager.FindByEmailAsync(email);

        if (usuarioExistente != null)
            return BadRequest(
                "Este correo ya está registrado."
            );

        // 5. Crear la cuenta con Identity
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            ClienteId = cliente.Id
        };

        var resultado = await _userManager.CreateAsync(
            usuario,
            request.Password
        );

        if (!resultado.Succeeded)
        {
            return BadRequest(new
            {
                mensaje = "No se pudo crear la cuenta.",
                errores = resultado.Errors.Select(e => e.Description)
            });
        }

        // 6. Asignar el rol Cliente
        var resultadoRol = await _userManager.AddToRoleAsync(
            usuario,
            "Cliente"
        );

        if (!resultadoRol.Succeeded)
        {
            await _userManager.DeleteAsync(usuario);

            return StatusCode(
                500,
                "No se pudo asignar el rol Cliente."
            );
        }

        return Ok(new
        {
            mensaje = "Cuenta de cliente creada correctamente.",
            clienteId = cliente.Id,
            email = usuario.Email,
            rol = "Cliente"
        });
    }
    // Consultar qué clientes tienen cuenta de acceso
    [HttpGet("estados")]
    public async Task<IActionResult> ObtenerEstadosCuentas()
    {
        var estados = await _context.Clientes
            .AsNoTracking()
            .Select(c => new
            {
                clienteId = c.Id,

                tieneCuenta = _context.Users
                    .Any(u => u.ClienteId == c.Id),

                tieneAlquilerActivo = _context.Alquileres
                    .Any(a =>
                        a.ClienteId == c.Id &&
                        a.Estado == "Activo")
            })
            .ToListAsync();

        return Ok(estados);
    }


}