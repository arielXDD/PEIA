using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEIA.Shared.Infra.Automation;
using PEIA.Shared.Infra.Data;

namespace PEIA.Modules.Automation.Controllers;

[ApiController]
[Route("api/reglas-automatizacion")]
[Authorize(Roles = "Administrador")]
public class ReglasController : ControllerBase
{
    private readonly PeiaDbContext _context;

    public ReglasController(PeiaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetReglas()
    {
        var reglas = await _context.ReglasAutomatizacion
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();
        return Ok(reglas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRegla(Guid id)
    {
        var regla = await _context.ReglasAutomatizacion.FindAsync(id);
        if (regla == null)
        {
            return NotFound(new { message = "La regla no existe." });
        }
        return Ok(regla);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRegla([FromBody] ReglaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new { message = "El nombre es obligatorio." });
        }
        if (string.IsNullOrWhiteSpace(request.EventoOrigen))
        {
            return BadRequest(new { message = "El evento de origen es obligatorio." });
        }
        if (string.IsNullOrWhiteSpace(request.Condicion))
        {
            return BadRequest(new { message = "La condición es obligatoria." });
        }
        if (string.IsNullOrWhiteSpace(request.Accion))
        {
            return BadRequest(new { message = "La acción es obligatoria." });
        }
        if (string.IsNullOrWhiteSpace(request.Responsable))
        {
            return BadRequest(new { message = "El responsable es obligatorio." });
        }

        var regla = new ReglaAutomatizacion
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            EventoOrigen = request.EventoOrigen.Trim(),
            Condicion = request.Condicion.Trim(),
            Accion = request.Accion.Trim(),
            Responsable = request.Responsable.Trim(),
            Activa = request.Activa,
            FechaCreacion = DateTime.UtcNow
        };

        _context.ReglasAutomatizacion.Add(regla);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRegla), new { id = regla.Id }, regla);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRegla(Guid id, [FromBody] ReglaRequest request)
    {
        var regla = await _context.ReglasAutomatizacion.FindAsync(id);
        if (regla == null)
        {
            return NotFound(new { message = "La regla no existe." });
        }

        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new { message = "El nombre es obligatorio." });
        }
        if (string.IsNullOrWhiteSpace(request.EventoOrigen))
        {
            return BadRequest(new { message = "El evento de origen es obligatorio." });
        }
        if (string.IsNullOrWhiteSpace(request.Condicion))
        {
            return BadRequest(new { message = "La condición es obligatoria." });
        }
        if (string.IsNullOrWhiteSpace(request.Accion))
        {
            return BadRequest(new { message = "La acción es obligatoria." });
        }
        if (string.IsNullOrWhiteSpace(request.Responsable))
        {
            return BadRequest(new { message = "El responsable es obligatorio." });
        }

        regla.Nombre = request.Nombre.Trim();
        regla.EventoOrigen = request.EventoOrigen.Trim();
        regla.Condicion = request.Condicion.Trim();
        regla.Accion = request.Accion.Trim();
        regla.Responsable = request.Responsable.Trim();
        regla.Activa = request.Activa;

        await _context.SaveChangesAsync();

        return Ok(regla);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRegla(Guid id)
    {
        var regla = await _context.ReglasAutomatizacion.FindAsync(id);
        if (regla == null)
        {
            return NotFound(new { message = "La regla no existe." });
        }

        _context.ReglasAutomatizacion.Remove(regla);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Regla eliminada correctamente." });
    }
}

public record ReglaRequest(
    string Nombre,
    string EventoOrigen,
    string Condicion,
    string Accion,
    string Responsable,
    bool Activa);
