using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEIA.Shared.Infra.Data;

namespace PEIA.Web.Controllers;

[ApiController]
[Route("api/bitacora")]
[Authorize]
public class BitacoraController : ControllerBase
{
    private readonly PeiaDbContext _context;

    public BitacoraController(PeiaDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? modulo, [FromQuery] string? nivel,
        [FromQuery] string? query, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var events = await GetEventsAsync();
        var filtered = Filter(events, modulo, nivel, query, desde, hasta);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 5, 100);

        return Ok(new { items = filtered.Skip((page - 1) * pageSize).Take(pageSize), total = filtered.Count, page, pageSize });
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> Summary()
    {
        var events = await GetEventsAsync();
        var today = DateTime.UtcNow.Date;
        return Ok(new
        {
            eventosHoy = events.Count(item => item.fecha >= today),
            erroresHoy = events.Count(item => item.fecha >= today && item.nivel == "Error"),
            usuariosActivos = events.Where(item => item.fecha >= today).Select(item => item.usuario).Distinct().Count(),
            eventosMes = events.Count(item => item.fecha >= new DateTime(today.Year, today.Month, 1))
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? modulo, [FromQuery] string? nivel,
        [FromQuery] string? query, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var events = Filter(await GetEventsAsync(), modulo, nivel, query, desde, hasta);
        var csv = new StringBuilder("Fecha,Usuario,Módulo,Acción,Detalle,Centro,Nivel\n");
        foreach (var item in events)
            csv.AppendLine(string.Join(',', new[] { item.fecha.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), item.usuario, item.modulo, item.accion, item.detalle, item.centro, item.nivel }.Select(Escape)));

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv; charset=utf-8", $"bitacora-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private async Task<List<AuditEvent>> GetEventsAsync()
    {
        var movements = await _context.Movimientos.AsNoTracking().Include(item => item.Producto).Include(item => item.Centro).Include(item => item.Usuario)
            .OrderByDescending(item => item.FechaMovimiento).Take(250).Select(item => new AuditEvent(item.FechaMovimiento, item.Usuario != null ? item.Usuario.NombreCompleto : "Sistema", "Inventario", $"Movimiento de {item.Tipo}", $"{item.Producto!.Nombre}: {item.Motivo ?? "sin motivo"}", item.Centro!.Nombre, "Info")).ToListAsync();
        var orders = await _context.Pedidos.AsNoTracking().Include(item => item.Centro).OrderByDescending(item => item.FechaPedido).Take(250)
            .Select(item => new AuditEvent(item.FechaPedido, "Sistema", "Pedidos", "Pedido registrado", $"{item.Codigo} · {item.Cliente} · {item.Estado}", item.Centro!.Nombre, item.Estado == "Cancelado" ? "Warning" : "Info")).ToListAsync();
        var notifications = await _context.Notificaciones.AsNoTracking().OrderByDescending(item => item.FechaCreacion).Take(250)
            .Join(_context.Centros.AsNoTracking(), item => item.CentroId, centro => centro.Id, (item, centro) => new AuditEvent(item.FechaCreacion, "Sistema", "Notificaciones", item.Titulo, item.Descripcion ?? string.Empty, centro.Nombre, item.Tipo == "error" ? "Error" : item.Tipo == "warning" ? "Warning" : "Info")).ToListAsync();
        return movements.Concat(orders).Concat(notifications).OrderByDescending(item => item.fecha).ToList();
    }

    private static List<AuditEvent> Filter(IEnumerable<AuditEvent> source, string? modulo, string? nivel, string? query, DateTime? desde, DateTime? hasta) => source
        .Where(item => string.IsNullOrWhiteSpace(modulo) || item.modulo.Equals(modulo, StringComparison.OrdinalIgnoreCase))
        .Where(item => string.IsNullOrWhiteSpace(nivel) || item.nivel.Equals(nivel, StringComparison.OrdinalIgnoreCase))
        .Where(item => !desde.HasValue || item.fecha >= desde.Value.Date)
        .Where(item => !hasta.HasValue || item.fecha < hasta.Value.Date.AddDays(1))
        .Where(item => string.IsNullOrWhiteSpace(query) || $"{item.usuario} {item.modulo} {item.accion} {item.detalle} {item.centro}".Contains(query, StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(item => item.fecha).ToList();

    private static string Escape(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
    private record AuditEvent(DateTime fecha, string usuario, string modulo, string accion, string detalle, string centro, string nivel);
}
