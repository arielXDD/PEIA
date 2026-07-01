using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEIA.Shared.Infra.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PEIA.Modules.Reports.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly PeiaDbContext _context;

    public ReportsController(PeiaDbContext context)
    {
        _context = context;
    }

    [HttpGet("pedidos")]
    public async Task<IActionResult> GetPedidosReport([FromQuery] Guid? centroId, [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        if (centroId == null || centroId == Guid.Empty)
        {
            return BadRequest(new { message = "El parámetro centroId es obligatorio." });
        }

        var query = _context.Pedidos
            .Include(p => p.SLA)
            .Where(p => p.CentroId == centroId);

        if (fechaInicio.HasValue)
        {
            query = query.Where(p => p.FechaPedido >= fechaInicio.Value.ToUniversalTime());
        }

        if (fechaFin.HasValue)
        {
            query = query.Where(p => p.FechaPedido <= fechaFin.Value.ToUniversalTime());
        }

        var totalPedidos = await query.CountAsync();
        var entregados = await query.CountAsync(p => p.Estado == "Entregado");
        var enRuta = await query.CountAsync(p => p.Estado == "EnRuta");
        var asignados = await query.CountAsync(p => p.Estado == "Asignado");
        var pendientes = await query.CountAsync(p => p.Estado == "Creado");
        var cancelados = await query.CountAsync(p => p.Estado == "Cancelado");

        var slas = await query
            .Where(p => p.SLA != null)
            .Select(p => p.SLA!)
            .ToListAsync();

        var totalSlas = slas.Count;
        var cumplidos = slas.Count(s => s.EstadoSLA.Equals("Cumplido", StringComparison.OrdinalIgnoreCase));
        var incumplidos = slas.Count(s => s.EstadoSLA.Equals("Incumplido", StringComparison.OrdinalIgnoreCase));
        
        var tasaCumplimiento = totalSlas > 0 ? (double)cumplidos / totalSlas * 100 : 100.0;

        return Ok(new
        {
            TotalPedidos = totalPedidos,
            Estados = new
            {
                Creado = pendientes,
                Asignado = asignados,
                EnRuta = enRuta,
                Entregado = entregados,
                Cancelado = cancelados
            },
            Slas = new
            {
                Total = totalSlas,
                Cumplidos = cumplidos,
                Incumplidos = incumplidos,
                TasaCumplimiento = Math.Round(tasaCumplimiento, 2)
            }
        });
    }

    [HttpGet("inventario")]
    public IActionResult GetInventarioReport([FromQuery] Guid? centroId)
    {
        // Mock de datos de inventario mientras el módulo de inventario no está implementado
        return Ok(new
        {
            TotalProductos = 150,
            StockTotal = 8247,
            Categorias = new[]
            {
                new { Nombre = "Maderas", Cantidad = 1200 },
                new { Nombre = "Plásticos", Cantidad = 2010 },
                new { Nombre = "Embalajes", Cantidad = 1160 },
                new { Nombre = "Cintas", Cantidad = 640 },
                new { Nombre = "Otros", Cantidad = 3237 }
            },
            AlertasStockMinimo = new[]
            {
                new { Codigo = "PROD-0012", Nombre = "Cinta Adhesiva 48mm", StockActual = 25, StockMinimo = 50 },
                new { Codigo = "PROD-0045", Nombre = "Guantes de Nitrilo", StockActual = 0, StockMinimo = 100 }
            }
        });
    }

    [HttpGet("movimientos")]
    public IActionResult GetMovimientosReport([FromQuery] Guid? centroId, [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        // Mock de movimientos
        return Ok(new[]
        {
            new { Fecha = DateTime.UtcNow.AddHours(-1), Tipo = "Entrada", Producto = "Tarima de Madera", Cantidad = 120, Ubicacion = "A-01-01" },
            new { Fecha = DateTime.UtcNow.AddHours(-2), Tipo = "Salida", Producto = "Caja Plástica 60L", Cantidad = 45, Ubicacion = "B-03-03" },
            new { Fecha = DateTime.UtcNow.AddHours(-4), Tipo = "Entrada", Producto = "Film Stretch", Cantidad = 80, Ubicacion = "C-07-02" },
            new { Fecha = DateTime.UtcNow.AddHours(-5), Tipo = "Salida", Producto = "Pallet Plástico", Cantidad = 20, Ubicacion = "A-02-03" }
        });
    }

    [HttpGet("exportar")]
    public IActionResult ExportarReporte([FromQuery] string tipoReporte)
    {
        // Simular exportación a CSV/Excel devolviendo un string formateado o archivo ficticio
        var csv = "Reporte;Generado;Fecha\n";
        csv += $"{tipoReporte};Exitoso;{DateTime.UtcNow}\n";

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"Reporte_{tipoReporte}_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
