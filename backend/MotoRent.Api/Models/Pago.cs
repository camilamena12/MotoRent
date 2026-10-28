
namespace MotoRent.Api.Models;

public class Pago
{
    public int Id { get; set; }

    public int AlquilerId { get; set; }

    public Alquiler? Alquiler { get; set; }

    public DateTime FechaPago { get; set; } = DateTime.Now;

    public DateTime DiaAlquilado { get; set; }

    public decimal Importe { get; set; }

    public string MetodoPago { get; set; } = "Efectivo";
}
