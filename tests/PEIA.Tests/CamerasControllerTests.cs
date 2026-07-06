using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PEIA.Tests;

public class CamerasControllerTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_ReturnsOk_With4Cameras()
    {
        var response = await _client.GetAsync("/api/camaras");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cameras = await response.Content.ReadFromJsonAsync<JsonElement[]>();
        Assert.NotNull(cameras);
        Assert.Equal(4, cameras.Length);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/camaras/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var camara = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, camara.GetProperty("id").GetInt32());
        Assert.Equal("Cámara 1", camara.GetProperty("nombre").GetString());
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/camaras/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/camaras/abc");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Capturar_ExistingId_ReturnsOk()
    {
        var response = await _client.PostAsync("/api/camaras/1/capturar", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Captura realizada con éxito.", result.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Capturar_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.PostAsync("/api/camaras/999/capturar", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ReportarIncidencia_ExistingId_ReturnsOk()
    {
        var payload = new { descripcion = "Prueba de incidencia" };
        var response = await _client.PostAsJsonAsync("/api/camaras/1/reportar-incidencia", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Incidencia reportada correctamente.", result.GetProperty("message").GetString());
    }

    [Fact]
    public async Task ReportarIncidencia_NonExistingId_ReturnsNotFound()
    {
        var payload = new { descripcion = "Prueba" };
        var response = await _client.PostAsJsonAsync("/api/camaras/999/reportar-incidencia", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
