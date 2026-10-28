
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.Data;

namespace MotoRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MotosController : ControllerBase
{
    private readonly MotoRentContext _context;

    public MotosController(MotoRentContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerMotos()
    {
        var motos = await _context.Motos.ToListAsync();

        return Ok(motos);
    }
    //este metodo recibe los datos de una moto y las guarda en sql y devuelve la moto registrada
    [HttpPost]
    public async Task<IActionResult> CrearMoto(MotoRent.Api.Models.Moto moto)
    {
        _context.Motos.Add(moto);

        await _context.SaveChangesAsync();

        return Ok(moto);
    }
   
    //este metodo sirve para consultar motos, recibe el id de la moto desde la URL y busca esa moto en la DB
    
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerMotoPorId(int id)
    {
        var moto = await _context.Motos.FindAsync(id);

        if (moto == null)
        {
            return NotFound();
        }

        return Ok(moto);
    }

    //este metodo sirve para modificar motos 

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarMoto(int id, MotoRent.Api.Models.Moto motoActualizada)
    {
        var moto = await _context.Motos.FindAsync(id);

        if (moto == null)
        {
            return NotFound();
        }

        moto.Marca = motoActualizada.Marca;
        moto.Modelo = motoActualizada.Modelo;
        moto.Cilindrada = motoActualizada.Cilindrada;
        moto.PrecioPorDia = motoActualizada.PrecioPorDia;
        moto.Disponible = motoActualizada.Disponible;

        await _context.SaveChangesAsync();

        return Ok(moto);
    }
    //este metodo se utiliza para eliminar motos 
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarMoto(int id)
    {
        var moto = await _context.Motos.FindAsync(id);

        if (moto == null)
        {
            return NotFound();
        }

        _context.Motos.Remove(moto);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
