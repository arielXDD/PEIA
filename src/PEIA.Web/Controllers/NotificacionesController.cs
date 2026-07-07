using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEIA.Shared.Infra.Data;

namespace PEIA.Web.Controllers;

[ApiController]
[Route("api/notificaciones")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly PeiaDbContext _context;

    public NotificacionesController(PeiaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotificaciones([FromQuery] Guid? centroId, [FromQuery] bool soloNoLeidas = false)
    {
        if (centroId == null || centroId == Guid.Empty)
        {
            return BadRequest(new { message = "El parámetro centroId es obligatorio." });
        }

        var query = _context.Notificaciones.Where(n => n.CentroId == centroId);
        if (soloNoLeidas)
        {
            query = query.Where(n => !n.Leida);
        }

        var notificaciones = await query
            .OrderByDescending(n => n.FechaCreacion)
            .Take(100)
            .Select(n => new
            {
                n.Id,
                n.Tipo,
                n.Titulo,
                n.Descripcion,
                n.Leida,
                n.FechaCreacion
            })
            .ToListAsync();

        return Ok(notificaciones);
    }

    [HttpPut("{id:guid}/leer")]
    public async Task<IActionResult> MarcarLeida(Guid id)
    {
        var notificacion = await _context.Notificaciones.FindAsync(id);
        if (notificacion is null)
        {
            return NotFound(new { message = "Notificación no encontrada." });
        }

        notificacion.Leida = true;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Notificación marcada como leída." });
    }

    [HttpPut("marcar-todas")]
    public async Task<IActionResult> MarcarTodasLeidas([FromQuery] Guid? centroId)
    {
        if (centroId == null || centroId == Guid.Empty)
        {
            return BadRequest(new { message = "El parámetro centroId es obligatorio." });
        }

        await _context.Notificaciones
            .Where(n => n.CentroId == centroId && !n.Leida)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));

        return Ok(new { message = "Todas las notificaciones fueron marcadas como leídas." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> EliminarNotificacion(Guid id)
    {
        var notificacion = await _context.Notificaciones.FindAsync(id);
        if (notificacion is null)
        {
            return NotFound(new { message = "Notificación no encontrada." });
        }

        _context.Notificaciones.Remove(notificacion);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
