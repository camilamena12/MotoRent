using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotoRent.Api.Services;

namespace MotoRent.Api.Controllers;

[ApiController]
[Route("api/validacion-dni")]
public class ValidacionDniController : ControllerBase
{
    private readonly RenaperService _renaperService;

    public ValidacionDniController(RenaperService renaperService)
    {
        _renaperService = renaperService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ValidarDni(
        [FromBody] ValidarDniRequest request)
    {
        try
        {
            var validado = await _renaperService.ValidarDniAsync(
                request.Dni,
                request.Nombre.Trim(),
                request.Apellido.Trim()
            );

            if (!validado)
            {
                return Ok(new
                {
                    validado = false,
                    mensaje = "Los datos no coinciden o no cumple la edad mínima."
                });
            }

            return Ok(new
            {
                validado = true,
                mensaje = "DNI validado correctamente."
            });
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine(
     $"Error al consultar RENAPER: {ex.Message}"
 );

            return StatusCode(503, new
            {
                validado = false,
                mensaje = "No se pudo consultar RENAPER. Intentá más tarde."
            });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(503, new
            {
                validado = false,
                mensaje = "RENAPER tardó demasiado en responder."
            });
        }
        catch (System.Text.Json.JsonException)
        {
            return StatusCode(502, new
            {
                validado = false,
                mensaje = "RENAPER devolvió una respuesta inválida."
            });
        }
    }
}

public class ValidarDniRequest
{
    [Required]
    [RegularExpression(@"^\d{7,8}$")]
    public string Dni { get; set; } = string.Empty;

    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Apellido { get; set; } = string.Empty;
}