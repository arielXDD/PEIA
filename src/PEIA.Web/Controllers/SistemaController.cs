using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEIA.Shared.Infra.Data;

namespace PEIA.Web.Controllers;

[ApiController]
[Route("api/sistema")]
[Authorize(Roles = "Administrador")]
public class SistemaController : ControllerBase
{
    private readonly PeiaDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public SistemaController(PeiaDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet("salud")]
    public async Task<IActionResult> GetSalud()
    {
        var canConnect = await _context.Database.CanConnectAsync();
        var pendingMigrations = canConnect
            ? await _context.Database.GetPendingMigrationsAsync()
            : Array.Empty<string>();
        var appliedMigrations = canConnect
            ? await _context.Database.GetAppliedMigrationsAsync()
            : Array.Empty<string>();

        return Ok(new
        {
            estado = canConnect && !pendingMigrations.Any() ? "Operativo" : "Atencion",
            ambiente = _environment.EnvironmentName,
            baseDatos = new
            {
                conectada = canConnect,
                migracionesAplicadas = appliedMigrations,
                migracionesPendientes = pendingMigrations
            },
            signalR = new
            {
                activo = true,
                hub = "/hubs/peia"
            },
            fecha = DateTime.UtcNow
        });
    }
}
