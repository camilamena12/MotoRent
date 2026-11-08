
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MotoRent.Api.Services;

public class MercadoPagoService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public MercadoPagoService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<(string OrdenId, string CheckoutUrl)> CrearOrdenAsync(
        decimal importeTotal,
        string referenciaExterna)
    {
        if (importeTotal <= 0)
            throw new ArgumentException("El importe debe ser mayor a cero.");

        // Leer el Access Token guardado en User Secrets
        var token = _configuration["MercadoPago:AccessToken"];

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException(
                "No está configurado el Access Token de Mercado Pago.");

        // Mercado Pago necesita el importe con punto decimal
        var monto = importeTotal.ToString(
            "0.00",
            CultureInfo.InvariantCulture
        );

        var cliente = _httpClientFactory.CreateClient("MercadoPago");

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "v1/orders"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Headers.Add(
            "X-Idempotency-Key",
            Guid.NewGuid().ToString()
        );

        request.Content = JsonContent.Create(new
        {
            type = "online",
            processing_mode = "manual",
            total_amount = monto,
            external_reference = referenciaExterna,
            items = new[]
            {
                new
                {
                    title = "Pago de alquiler MotoRent",
                    unit_price = monto,
                    quantity = 1
                }
            }
        });

        using var response = await cliente.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Mercado Pago devolvió el error HTTP {(int)response.StatusCode}."
            );

        using var resultado = await response.Content
            .ReadFromJsonAsync<JsonDocument>();

        if (resultado == null)
            throw new InvalidOperationException(
                "Mercado Pago no devolvió una respuesta válida."
            );

        var ordenId = resultado.RootElement
            .GetProperty("id").GetString();

        var checkoutUrl = resultado.RootElement
            .GetProperty("checkout_url").GetString();

        if (string.IsNullOrWhiteSpace(ordenId) ||
            string.IsNullOrWhiteSpace(checkoutUrl))
            throw new InvalidOperationException(
                "Mercado Pago no devolvió los datos de la orden."
            );

        return (ordenId, checkoutUrl);
    }

    public async Task CancelarOrdenAsync(string ordenId)
    {
        if (string.IsNullOrWhiteSpace(ordenId))
            throw new ArgumentException("El ID de la orden es obligatorio.");

        var token = _configuration["MercadoPago:AccessToken"];

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException(
                "No está configurado el Access Token de Mercado Pago.");

        var cliente = _httpClientFactory.CreateClient("MercadoPago");

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"v1/orders/{Uri.EscapeDataString(ordenId)}/cancel"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Headers.Add(
            "X-Idempotency-Key",
            Guid.NewGuid().ToString()
        );

        using var response = await cliente.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"No se pudo cancelar la orden en Mercado Pago. " +
                $"Código HTTP: {(int)response.StatusCode}"
            );
        }
    }

    //consulta la orden 
    public async Task<(
        string Estado,
        string? Detalle,
        string? ReferenciaExterna,
        decimal ImporteTotal,
        decimal TotalPagado,
        string? Moneda)> ConsultarOrdenAsync(string ordenId)
    {
        var token = _configuration["MercadoPago:AccessToken"];

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException(
                "No está configurado el Access Token de Mercado Pago.");

        var cliente = _httpClientFactory.CreateClient("MercadoPago");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"v1/orders/{Uri.EscapeDataString(ordenId)}"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await cliente.SendAsync(request);

        response.EnsureSuccessStatusCode();

        using var resultado = await response.Content
            .ReadFromJsonAsync<JsonDocument>();

        if (resultado == null)
            throw new InvalidOperationException(
                "Mercado Pago no devolvió una respuesta válida.");

        var datos = resultado.RootElement;

        var estado = datos.GetProperty("status").GetString() ?? "";

        var detalle = datos.TryGetProperty("status_detail", out var d)
            ? d.GetString()
            : null;

        var referencia = datos.TryGetProperty("external_reference", out var r)
            ? r.GetString()
            : null;

        var moneda = datos.TryGetProperty("currency", out var m)
            ? m.GetString()
            : null;

        var importeTotal = decimal.Parse(
            datos.GetProperty("total_amount").ToString(),
            CultureInfo.InvariantCulture
        );

        decimal totalPagado = 0;

        if (datos.TryGetProperty("total_paid_amount", out var pagado))
        {
            totalPagado = decimal.Parse(
                pagado.ToString(),
                CultureInfo.InvariantCulture
            );
        }

        return (
            estado,
            detalle,
            referencia,
            importeTotal,
            totalPagado,
            moneda
        );
    }


}
