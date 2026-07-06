using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PEIA.Tests;

public class PrediccionControllerTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_ReturnsOk_With6Productos()
    {
        var response = await _client.GetAsync("/api/predicciones");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var productos = await response.Content.ReadFromJsonAsync<JsonElement[]>();
        Assert.NotNull(productos);
        Assert.Equal(6, productos.Length);
    }

    [Fact]
    public async Task Resumen_ReturnsOk_WithExpectedFields()
    {
        var response = await _client.GetAsync("/api/predicciones/resumen");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var resumen = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(resumen.GetProperty("demandaEstimada").GetInt32() > 0);
        Assert.True(resumen.GetProperty("precisionModelo").GetDouble() > 0);
        Assert.True(resumen.GetProperty("alertasActivas").GetInt32() > 0);
        Assert.True(resumen.GetProperty("categoriasAnalizadas").GetInt32() > 0);
        Assert.False(string.IsNullOrEmpty(resumen.GetProperty("periodo").GetString()));
    }

    [Fact]
    public async Task Historico_ReturnsOk_WithData()
    {
        var response = await _client.GetAsync("/api/predicciones/historico");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var historico = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(historico.TryGetProperty("labels", out _));
        Assert.True(historico.TryGetProperty("valores", out _));
    }

    [Fact]
    public async Task Prediccion_DefaultDays_Returns14Days()
    {
        var response = await _client.GetAsync("/api/predicciones/prediccion");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var prediccion = await response.Content.ReadFromJsonAsync<JsonElement>();
        var labels = prediccion.GetProperty("labels").EnumerateArray().ToArray();
        var valores = prediccion.GetProperty("valores").EnumerateArray().ToArray();

        Assert.Equal(14, labels.Length);
        Assert.Equal(14, valores.Length);
    }

    [Fact]
    public async Task Prediccion_With7Days_Returns7Days()
    {
        var response = await _client.GetAsync("/api/predicciones/prediccion?dias=7");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var prediccion = await response.Content.ReadFromJsonAsync<JsonElement>();
        var labels = prediccion.GetProperty("labels").EnumerateArray().ToArray();
        Assert.Equal(7, labels.Length);
    }

    [Fact]
    public async Task Prediccion_With30Days_Returns30Days()
    {
        var response = await _client.GetAsync("/api/predicciones/prediccion?dias=30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var prediccion = await response.Content.ReadFromJsonAsync<JsonElement>();
        var labels = prediccion.GetProperty("labels").EnumerateArray().ToArray();
        Assert.Equal(30, labels.Length);
    }

    [Fact]
    public async Task Prediccion_IncludesConfianzaArray()
    {
        var response = await _client.GetAsync("/api/predicciones/prediccion");

        var prediccion = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(prediccion.TryGetProperty("confianza", out _));
    }
}
