
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.Data;
using MotoRent.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace MotoRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]

public class ClientesController : ControllerBase
{
    private readonly MotoRentContext _context;

    public ClientesController(MotoRentContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerClientes()
    {
        var clientes = await _context.Clientes.ToListAsync();

        return Ok(clientes);
    }

    //modificar cliente
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActualizarCliente(int id, Cliente clienteActualizado)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente == null)
        {
            return NotFound();
        }

        cliente.Nombre = clienteActualizado.Nombre;
        cliente.Apellido = clienteActualizado.Apellido;
        cliente.DNI = clienteActualizado.DNI;
        cliente.Telefono = clienteActualizado.Telefono;
        cliente.Email = clienteActualizado.Email;

        await _context.SaveChangesAsync();

        return Ok(cliente);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerClientePorId(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente == null)
        {
            return NotFound();
        }

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> CrearCliente(Cliente cliente)
    {
        _context.Clientes.Add(cliente);

        await _context.SaveChangesAsync();

        return Ok(cliente);
    }
}
