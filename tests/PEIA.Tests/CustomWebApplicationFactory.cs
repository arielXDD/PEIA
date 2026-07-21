using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PEIA.Tests;

/// <summary>
/// Handler de autenticación falso para pruebas de integración.
/// Simula siempre un usuario autenticado válido sin necesidad de tokens reales.
/// </summary>
public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim(ClaimTypes.Email, "test@peia.com"),
            new Claim(ClaimTypes.Role, "Administrador")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var assemblyDir = Path.GetDirectoryName(typeof(CustomWebApplicationFactory).Assembly.Location)!;

        var webProjectDir = Path.GetFullPath(
            Path.Combine(assemblyDir, "..", "..", "..", "..", "..", "src", "PEIA.Web"));

        builder.UseContentRoot(webProjectDir);
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Registrar esquema de autenticación de prueba que siempre autentica al usuario
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }
}
