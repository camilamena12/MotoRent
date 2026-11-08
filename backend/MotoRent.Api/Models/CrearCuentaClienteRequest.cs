using System.ComponentModel.DataAnnotations;

namespace MotoRent.Api.Models;

public class CrearCuentaClienteRequest
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}