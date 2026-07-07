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
    public async Task<IActionResult> GetInventarioReport([FromQuery] Guid? centroId)
    {
        if (centroId == null || centroId == Guid.Empty)
        {
            return BadRequest(new { message = "El parámetro centroId es obligatorio." });
        }

        var productos = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Stocks)
            .Where(p => p.Activo)
            .Select(p => new
            {
                p.Sku,
                p.Nombre,
                p.StockMinimo,
                Categoria = p.Categoria != null ? p.Categoria.Nombre : "Sin categoría",
                Stock = p.Stocks.Where(s => s.CentroId == centroId).Select(s => s.Cantidad).FirstOrDefault()
            })
            .ToListAsync();

        var categorias = productos
            .GroupBy(p => p.Categoria)
            .Select(g => new { Nombre = g.Key, Cantidad = g.Sum(p => p.Stock) })
            .OrderByDescending(c => c.Cantidad)
            .ToList();

        var alertas = productos
            .Where(p => p.Stock <= p.StockMinimo)
            .OrderBy(p => p.Stock)
            .Select(p => new { Codigo = p.Sku, Nombre = p.Nombre, StockActual = p.Stock, StockMinimo = p.StockMinimo })
            .ToList();

        return Ok(new
        {
            TotalProductos = productos.Count,
            StockTotal = productos.Sum(p => p.Stock),
            Categorias = categorias,
            AlertasStockMinimo = alertas
        });
    }

    [HttpGet("movimientos")]
    public async Task<IActionResult> GetMovimientosReport([FromQuery] Guid? centroId, [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        if (centroId == null || centroId == Guid.Empty)
        {
            return BadRequest(new { message = "El parámetro centroId es obligatorio." });
        }

        var query = _context.Movimientos
            .Include(m => m.Producto)
            .Where(m => m.CentroId == centroId);

        if (fechaInicio.HasValue)
        {
            query = query.Where(m => m.FechaMovimiento >= fechaInicio.Value.ToUniversalTime());
        }

        if (fechaFin.HasValue)
        {
            query = query.Where(m => m.FechaMovimiento <= fechaFin.Value.ToUniversalTime());
        }

        var movimientos = await query
            .OrderByDescending(m => m.FechaMovimiento)
            .Take(500)
            .Select(m => new
            {
                Fecha = m.FechaMovimiento,
                m.Tipo,
                Producto = m.Producto != null ? m.Producto.Nombre : string.Empty,
                m.Cantidad,
                m.Referencia
            })
            .ToListAsync();

        return Ok(movimientos);
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
