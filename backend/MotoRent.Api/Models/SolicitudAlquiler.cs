namespace MotoRent.Api.Models;

public class SolicitudAlquiler
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int MotoId { get; set; }
    public Moto? Moto { get; set; }

    public DateTime FechaInicioSolicitada { get; set; }

    public bool OpcionCompra { get; set; } = false;

    public string Estado { get; set; } = "Pendiente";

    public DateTime FechaSolicitud { get; set; } = DateTime.Now;
}