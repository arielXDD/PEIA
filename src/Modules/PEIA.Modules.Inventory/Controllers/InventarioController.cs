using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEIA.Modules.Inventory.Notifications;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Inventory;

namespace PEIA.Modules.Inventory.Controllers;

[ApiController]
[Route("api/inventario")]
[Authorize]
public class InventarioController : ControllerBase
{
    private readonly PeiaDbContext _context;
    private readonly IMediator _mediator;

    public InventarioController(PeiaDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias()
    {
        var categorias = await _context.Categorias
            .Where(c => c.Activa)
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaResponse(c.Id, c.Nombre, c.Descripcion, c.Activa))
            .ToListAsync();

        return Ok(categorias);
    }

    [HttpPost("categorias")]
    [Authorize(Roles = "Administrador,OperadorInventario")]
    public async Task<IActionResult> CreateCategoria([FromBody] CategoriaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new { message = "El nombre de la categoría es obligatorio." });
        }

        var nombre = request.Nombre.Trim();
        if (await _context.Categorias.AnyAsync(c => c.Nombre == nombre))
        {
            return Conflict(new { message = $"Ya existe la categoría '{nombre}'." });
        }

        var categoria = new Categoria
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Descripcion = request.Descripcion?.Trim(),
            Activa = true,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategorias), new { id = categoria.Id },
            new CategoriaResponse(categoria.Id, categoria.Nombre, categoria.Descripcion, categoria.Activa));
    }

    [HttpGet("productos")]
    public async Task<IActionResult> GetProductos([FromQuery] Guid centroId, [FromQuery] string? q, [FromQuery] bool incluirInactivos = false)
    {
        if (centroId == Guid.Empty)
        {
            return BadRequest(new { message = "El parámetro centroId es obligatorio." });
        }

        var query = _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Stocks)
            .Where(p => incluirInactivos || p.Activo);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var text = q.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(text) || p.Sku.ToLower().Contains(text));
        }

        var productos = await query
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoResponse(
                p.Id,
                p.Sku,
                p.Nombre,
                p.Descripcion,
                p.UnidadMedida,
                p.PrecioUnitario,
                p.StockMinimo,
                p.Activo,
                p.CategoriaId,
                p.Categoria != null ? p.Categoria.Nombre : string.Empty,
                p.Stocks.Where(s => s.CentroId == centroId).Select(s => s.Cantidad).FirstOrDefault(),
                p.Stocks.Where(s => s.CentroId == centroId).Select(s => s.Ubicacion).FirstOrDefault()))
            .ToListAsync();

        return Ok(productos);
    }

    [HttpGet("productos/{id:guid}")]
    public async Task<IActionResult> GetProducto(Guid id, [FromQuery] Guid centroId)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Stocks)
            .Where(p => p.Id == id)
            .Select(p => new ProductoResponse(
                p.Id,
                p.Sku,
                p.Nombre,
                p.Descripcion,
                p.UnidadMedida,
                p.PrecioUnitario,
                p.StockMinimo,
                p.Activo,
                p.CategoriaId,
                p.Categoria != null ? p.Categoria.Nombre : string.Empty,
                p.Stocks.Where(s => s.CentroId == centroId).Select(s => s.Cantidad).FirstOrDefault(),
                p.Stocks.Where(s => s.CentroId == centroId).Select(s => s.Ubicacion).FirstOrDefault()))
            .FirstOrDefaultAsync();

        return producto is null ? NotFound(new { message = "Producto no encontrado." }) : Ok(producto);
    }

    [HttpPost("productos")]
    [Authorize(Roles = "Administrador,OperadorInventario")]
    public async Task<IActionResult> CreateProducto([FromBody] ProductoRequest request)
    {
        var validation = await ValidateProductoAsync(request);
        if (validation is not null)
        {
            return validation;
        }

        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            Sku = request.Sku.Trim().ToUpperInvariant(),
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            UnidadMedida = request.UnidadMedida.Trim(),
            PrecioUnitario = request.PrecioUnitario,
            StockMinimo = request.StockMinimo,
            CategoriaId = request.CategoriaId,
            Activo = request.Activo,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProducto), new { id = producto.Id, centroId = request.CentroId }, new { producto.Id });
    }

    [HttpPut("productos/{id:guid}")]
    [Authorize(Roles = "Administrador,OperadorInventario")]
    public async Task<IActionResult> UpdateProducto(Guid id, [FromBody] ProductoRequest request)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null)
        {
            return NotFound(new { message = "Producto no encontrado." });
        }

        var validation = await ValidateProductoAsync(request, id);
        if (validation is not null)
        {
            return validation;
        }

        producto.Sku = request.Sku.Trim().ToUpperInvariant();
        producto.Nombre = request.Nombre.Trim();
        producto.Descripcion = request.Descripcion?.Trim();
        producto.UnidadMedida = request.UnidadMedida.Trim();
        producto.PrecioUnitario = request.PrecioUnitario;
        producto.StockMinimo = request.StockMinimo;
        producto.CategoriaId = request.CategoriaId;
        producto.Activo = request.Activo;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Producto actualizado correctamente." });
    }

    [HttpDelete("productos/{id:guid}")]
    [Authorize(Roles = "Administrador,OperadorInventario")]
    public async Task<IActionResult> DeleteProducto(Guid id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null)
        {
            return NotFound(new { message = "Producto no encontrado." });
        }

        producto.Activo = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("movimientos")]
    [Authorize(Roles = "Administrador,OperadorInventario")]
    public async Task<IActionResult> RegistrarMovimiento([FromBody] MovimientoRequest request)
    {
        if (request.ProductoId == Guid.Empty || request.CentroId == Guid.Empty || request.Cantidad <= 0)
        {
            return BadRequest(new { message = "Producto, centro y cantidad positiva son obligatorios." });
        }

        var tipo = request.Tipo.Trim();
        if (!new[] { "Entrada", "Salida", "Ajuste" }.Contains(tipo, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Tipo inválido. Usa Entrada, Salida o Ajuste." });
        }

        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == request.ProductoId && p.Activo);
        if (producto is null)
        {
            return BadRequest(new { message = "Producto no encontrado o inactivo." });
        }

        if (!await _context.Centros.AnyAsync(c => c.Id == request.CentroId && c.Activo))
        {
            return BadRequest(new { message = "Centro no encontrado o inactivo." });
        }

        var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProductoId == request.ProductoId && s.CentroId == request.CentroId);
        if (stock is null)
        {
            stock = new Stock
            {
                Id = Guid.NewGuid(),
                ProductoId = request.ProductoId,
                CentroId = request.CentroId,
                Cantidad = 0,
                Ubicacion = request.Ubicacion?.Trim() ?? string.Empty,
                FechaActualizacion = DateTime.UtcNow
            };
            _context.Stocks.Add(stock);
        }

        var anterior = stock.Cantidad;
        var nuevo = tipo.Equals("Entrada", StringComparison.OrdinalIgnoreCase)
            ? anterior + request.Cantidad
            : tipo.Equals("Salida", StringComparison.OrdinalIgnoreCase)
                ? anterior - request.Cantidad
                : request.Cantidad;

        if (nuevo < 0)
        {
            return BadRequest(new { message = "La salida deja el stock en negativo." });
        }

        stock.Cantidad = nuevo;
        stock.Ubicacion = request.Ubicacion?.Trim() ?? stock.Ubicacion;
        stock.FechaActualizacion = DateTime.UtcNow;

        var movimiento = new Movimiento
        {
            Id = Guid.NewGuid(),
            Tipo = tipo,
            Cantidad = request.Cantidad,
            StockAnterior = anterior,
            StockNuevo = nuevo,
            Motivo = request.Motivo?.Trim(),
            Referencia = request.Referencia?.Trim(),
            FechaMovimiento = DateTime.UtcNow,
            ProductoId = request.ProductoId,
            CentroId = request.CentroId
        };

        _context.Movimientos.Add(movimiento);
        await _context.SaveChangesAsync();

        if (nuevo <= producto.StockMinimo)
        {
            await _mediator.Publish(new StockMinimoAlcanzadoNotification(
                producto.Id,
                producto.Sku,
                producto.Nombre,
                request.CentroId,
                nuevo,
                producto.StockMinimo));
        }

        return Ok(new MovimientoResponse(movimiento.Id, movimiento.Tipo, movimiento.Cantidad, anterior, nuevo, movimiento.FechaMovimiento));
    }

    [HttpGet("movimientos")]
    public async Task<IActionResult> GetMovimientos([FromQuery] Guid centroId, [FromQuery] Guid? productoId = null)
    {
        if (centroId == Guid.Empty)
        {
            return BadRequest(new { message = "El parámetro centroId es obligatorio." });
        }

        var query = _context.Movimientos
            .Include(m => m.Producto)
            .Where(m => m.CentroId == centroId);

        if (productoId is not null)
        {
            query = query.Where(m => m.ProductoId == productoId);
        }

        var movimientos = await query
            .OrderByDescending(m => m.FechaMovimiento)
            .Take(200)
            .Select(m => new
            {
                m.Id,
                m.Tipo,
                m.Cantidad,
                m.StockAnterior,
                m.StockNuevo,
                m.Motivo,
                m.Referencia,
                m.FechaMovimiento,
                Producto = m.Producto != null ? new { m.Producto.Id, m.Producto.Sku, m.Producto.Nombre } : null
            })
            .ToListAsync();

        return Ok(movimientos);
    }

    private async Task<IActionResult?> ValidateProductoAsync(ProductoRequest request, Guid? productoId = null)
    {
        if (string.IsNullOrWhiteSpace(request.Sku) ||
            string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.UnidadMedida) ||
            request.CategoriaId == Guid.Empty ||
            request.StockMinimo < 0 ||
            request.PrecioUnitario < 0)
        {
            return BadRequest(new { message = "Datos del producto inválidos o incompletos." });
        }

        var sku = request.Sku.Trim().ToUpperInvariant();
        if (await _context.Productos.AnyAsync(p => p.Id != productoId && p.Sku == sku))
        {
            return Conflict(new { message = $"Ya existe un producto con SKU '{sku}'." });
        }

        if (!await _context.Categorias.AnyAsync(c => c.Id == request.CategoriaId && c.Activa))
        {
            return BadRequest(new { message = "La categoría seleccionada no existe o está inactiva." });
        }

        return null;
    }
}

public record CategoriaRequest(string Nombre, string? Descripcion);
public record CategoriaResponse(Guid Id, string Nombre, string? Descripcion, bool Activa);

public record ProductoRequest(
    string Sku,
    string Nombre,
    string? Descripcion,
    string UnidadMedida,
    decimal PrecioUnitario,
    int StockMinimo,
    bool Activo,
    Guid CategoriaId,
    Guid CentroId);

public record ProductoResponse(
    Guid Id,
    string Sku,
    string Nombre,
    string? Descripcion,
    string UnidadMedida,
    decimal PrecioUnitario,
    int StockMinimo,
    bool Activo,
    Guid CategoriaId,
    string Categoria,
    int Stock,
    string? Ubicacion);

public record MovimientoRequest(
    Guid ProductoId,
    Guid CentroId,
    string Tipo,
    int Cantidad,
    string? Ubicacion,
    string? Motivo,
    string? Referencia);

public record MovimientoResponse(Guid Id, string Tipo, int Cantidad, int StockAnterior, int StockNuevo, DateTime FechaMovimiento);
