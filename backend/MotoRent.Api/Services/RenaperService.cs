using System.Net.Http.Json;

namespace MotoRent.Api.Services;

public class RenaperService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public RenaperService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<bool> ValidarDniAsync(
        string dni,
        string nombre,
        string apellido)
    {
        // Obtener la API Key desde User Secrets
        var apiKey = _configuration["Renaper:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception("No se configuró la API Key de RENAPER.");
        }

        if (!int.TryParse(dni, out int dniNumero))
        {
            return false;
        }

        // Utilizar el cliente HTTP que configuramos en Program.cs
        var cliente = _httpClientFactory.CreateClient("Renaper");

        // Preparar los datos para RENAPER
        var datos = new
        {
            dni = dniNumero,
            nombre = nombre,
            apellido = apellido,
            edadMinima = 18
        };

        // Preparar la solicitud
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/v1/personas/validar"
        );

        request.Headers.Add("X-API-Key", apiKey);
        request.Content = JsonContent.Create(datos);

        // Consultar la API de nuestros compañeros
        using var response = await cliente.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"RENAPER respondió con código {(int)response.StatusCode}."
            );
        }

        // Leer la respuesta JSON
        var resultado = await response.Content
            .ReadFromJsonAsync<RespuestaRenaper>();

        // Ambas condiciones deben ser verdaderas
        return resultado?.DatosCoinciden == true &&
               resultado?.CumpleEdadMinima == true;
    }

    private class RespuestaRenaper
    {
        public bool? DatosCoinciden { get; set; }
        public bool? CumpleEdadMinima { get; set; }
    }
}