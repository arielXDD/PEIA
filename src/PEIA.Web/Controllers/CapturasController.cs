using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEIA.Shared.Infra.Cameras;
using PEIA.Shared.Infra.Data;
using System.Security.Claims;

namespace PEIA.Web.Controllers;

[ApiController]
[Route("api/capturas")]
[Authorize]
public class CapturasController : ControllerBase
{
    private readonly PeiaDbContext _db;

    public CapturasController(PeiaDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var capturas = await _db.Capturas
            .OrderByDescending(c => c.FechaCaptura)
            .Take(50)
            .ToListAsync();
        return Ok(capturas);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCapturaRequest request)
    {
        var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : (Guid?)null;

        var captura = new Captura
        {
            Id = Guid.NewGuid(),
            CameraId = request.CameraId,
            NombreCamara = request.NombreCamara,
            Zona = request.Zona,
            ImagenUrl = request.ImagenUrl,
            Descripcion = request.Descripcion,
            UsuarioId = userId,
            FechaCaptura = DateTime.UtcNow
        };

        _db.Capturas.Add(captura);
        await _db.SaveChangesAsync();

        return Ok(captura);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var captura = await _db.Capturas.FindAsync(id);
        if (captura is null) return NotFound();

        _db.Capturas.Remove(captura);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Captura eliminada." });
    }
}

public class CreateCapturaRequest
{
    public int CameraId { get; set; }
    public string NombreCamara { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public string? Descripcion { get; set; }
}
