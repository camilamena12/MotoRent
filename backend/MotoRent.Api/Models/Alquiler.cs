
namespace MotoRent.Api.Models;

public class Alquiler
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    public int MotoId { get; set; }

    public Moto? Moto { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaDevolucion { get; set; }

    public decimal PrecioPorDia { get; set; }

    public bool OpcionCompra { get; set; } = false;

    public string Estado { get; set; } = "Activo";
}
