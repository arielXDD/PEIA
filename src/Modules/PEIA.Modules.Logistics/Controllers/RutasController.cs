using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Logistics;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PEIA.Modules.Logistics.Controllers;

[ApiController]
[Route("api/rutas")]
[Authorize]
public class RutasController : ControllerBase
{
    private readonly PeiaDbContext _context;

    public RutasController(PeiaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetRutas()
    {
        var rutas = await _context.Rutas
            .Where(r => r.Activa)
            .OrderBy(r => r.Nombre)
            .ToListAsync();
        return Ok(rutas);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRuta([FromBody] CreateRutaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Origen) ||
            string.IsNullOrWhiteSpace(request.Destino) ||
            request.DistanciaKm <= 0)
        {
            return BadRequest(new { message = "Todos los campos de la ruta son obligatorios y la distancia debe ser mayor a 0." });
        }

        var ruta = new Ruta
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Origen = request.Origen,
            Destino = request.Destino,
            DistanciaKm = request.DistanciaKm,
            Activa = true,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Rutas.Add(ruta);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRutas), new { id = ruta.Id }, ruta);
    }
}

public class CreateRutaRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public decimal DistanciaKm { get; set; }
}
