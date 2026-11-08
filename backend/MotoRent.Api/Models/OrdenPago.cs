
namespace MotoRent.Api.Models;

public class OrdenPago
{
    public int Id { get; set; }

    public int AlquilerId { get; set; }

    public int CantidadDias { get; set; }

    public decimal ImporteTotal { get; set; }

    // Guardaremos las fechas seleccionadas en formato JSON
    public string DiasSeleccionadosJson { get; set; } = "[]";

    // Identifica la operación dentro de MotoRent
    public string ReferenciaExterna { get; set; } = "";

    // Identificador devuelto por Mercado Pago
    public string? MercadoPagoOrdenId { get; set; }
    // Enlace para continuar el pago en MP para eitar problemas
    public string? CheckoutUrl { get; set; }

    public string Estado { get; set; } = "Iniciada";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
