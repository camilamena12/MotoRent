
namespace MotoRent.Api.Models;

public class Moto
{
    public int Id { get; set; }

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public int Cilindrada { get; set; }

    public decimal PrecioPorDia { get; set; }

    public bool Disponible { get; set; } = true;
}
