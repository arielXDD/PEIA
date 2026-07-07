using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Identity;

namespace PEIA.Modules.ERP.Controllers;

[ApiController]
[Route("api/centros")]
[Authorize]
public class CentrosController : ControllerBase
{
    private readonly PeiaDbContext _context;

    public CentrosController(PeiaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCentros([FromQuery] bool incluirInactivos = false)
    {
        var centros = await _context.Centros
            .Where(c => incluirInactivos || c.Activo)
            .OrderBy(c => c.Codigo)
            .Select(c => new CentroResponse(c.Id, c.Nombre, c.Codigo, c.Direccion, c.Activo))
            .ToListAsync();

        return Ok(centros);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCentro(Guid id)
    {
        var centro = await _context.Centros
            .Where(c => c.Id == id)
            .Select(c => new CentroResponse(c.Id, c.Nombre, c.Codigo, c.Direccion, c.Activo))
            .FirstOrDefaultAsync();

        return centro is null ? NotFound(new { message = "Centro no encontrado." }) : Ok(centro);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CreateCentro([FromBody] CentroRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Codigo))
        {
            return BadRequest(new { message = "Nombre y código son obligatorios." });
        }

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await _context.Centros.AnyAsync(c => c.Codigo == codigo))
        {
            return Conflict(new { message = $"Ya existe un centro con código '{codigo}'." });
        }

        var centro = new Centro
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            Codigo = codigo,
            Direccion = request.Direccion?.Trim() ?? string.Empty,
            Activo = request.Activo
        };

        _context.Centros.Add(centro);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCentro), new { id = centro.Id },
            new CentroResponse(centro.Id, centro.Nombre, centro.Codigo, centro.Direccion, centro.Activo));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateCentro(Guid id, [FromBody] CentroRequest request)
    {
        var centro = await _context.Centros.FindAsync(id);
        if (centro is null)
        {
            return NotFound(new { message = "Centro no encontrado." });
        }

        if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Codigo))
        {
            return BadRequest(new { message = "Nombre y código son obligatorios." });
        }

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await _context.Centros.AnyAsync(c => c.Id != id && c.Codigo == codigo))
        {
            return Conflict(new { message = $"Ya existe un centro con código '{codigo}'." });
        }

        centro.Nombre = request.Nombre.Trim();
        centro.Codigo = codigo;
        centro.Direccion = request.Direccion?.Trim() ?? string.Empty;
        centro.Activo = request.Activo;

        await _context.SaveChangesAsync();
        return Ok(new CentroResponse(centro.Id, centro.Nombre, centro.Codigo, centro.Direccion, centro.Activo));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteCentro(Guid id)
    {
        var centro = await _context.Centros.FindAsync(id);
        if (centro is null)
        {
            return NotFound(new { message = "Centro no encontrado." });
        }

        centro.Activo = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public record CentroRequest(string Nombre, string Codigo, string? Direccion, bool Activo = true);
public record CentroResponse(Guid Id, string Nombre, string Codigo, string Direccion, bool Activo);
