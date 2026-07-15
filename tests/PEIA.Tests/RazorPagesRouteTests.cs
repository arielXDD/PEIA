using System.Net;

namespace PEIA.Tests;

public class RazorPagesRouteTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    public static IEnumerable<object[]> Routes()
    {
        yield return ["/Login"];
        yield return ["/Inicio"];
        yield return ["/Inventario"];
        yield return ["/Pedidos"];
        yield return ["/Reportes"];
        yield return ["/Roles"];
        yield return ["/Usuarios"];
        yield return ["/Notificaciones"];
        yield return ["/Configuracion"];
        yield return ["/Bitacora"];
        yield return ["/Prediccion"];
        yield return ["/Camaras"];
        yield return ["/Guia"];
        yield return ["/Guia/intro"];
    }

    [Theory]
    [MemberData(nameof(Routes))]
    public async Task RazorPageRoute_ReturnsHtml(string route)
    {
        var response = await _client.GetAsync(route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }
}
