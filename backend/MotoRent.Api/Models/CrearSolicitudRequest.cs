using System.ComponentModel.DataAnnotations;

namespace MotoRent.Api.Models;

public class CrearSolicitudRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{7,8}$")]
    public string DNI { get; set; } = string.Empty;

    [Required]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int MotoId { get; set; }

    [Required]
    public DateTime? FechaInicioSolicitada { get; set; }

    public bool OpcionCompra { get; set; }
}