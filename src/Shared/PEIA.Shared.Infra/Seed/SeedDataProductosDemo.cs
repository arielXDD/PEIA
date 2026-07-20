using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Inventory;

namespace PEIA.Shared.Infra.Seed;

public static class SeedDataProductosDemo
{
    public static async Task EnsureAsync(PeiaDbContext db, ILogger logger)
    {
        var centros = await db.Centros.OrderBy(c => c.Codigo).ToListAsync();
        var categorias = await db.Categorias.ToDictionaryAsync(c => c.Nombre);
        if (centros.Count == 0 || categorias.Count == 0) return;

        var productos = new[]
        {
            Product("ELEC-101", "Camara IP Interior 1080p", "Equipo de monitoreo para pasillos internos.", "Electrónica", "Pieza", 74m, 16),
            Product("ELEC-102", "NVR 16 Canales", "Grabador para circuito cerrado de bodegas.", "Electrónica", "Pieza", 390m, 4),
            Product("ELEC-103", "Switch PoE 24 Puertos", "Switch para alimentar camaras y puntos de red.", "Electrónica", "Pieza", 280m, 6),
            Product("INS-101", "Rollos Termicos 80mm", "Consumible para impresoras de ticket y recepcion.", "Insumos", "Caja", 42m, 65),
            Product("INS-102", "Hojas Picking Pack", "Formatos para surtido y validacion manual.", "Insumos", "Paquete", 18m, 90),
            Product("REF-101", "Cadena Transportadora", "Refaccion para linea de traslado de cajas.", "Refacciones", "Metro", 55m, 12),
            Product("REF-102", "Sensor Fotoelectrico", "Sensor para validacion de paso en banda.", "Refacciones", "Pieza", 34m, 20),
            Product("EMB-101", "Bolsa Burbuja 30x40", "Material protector para productos fragiles.", "Embalaje", "Paquete", 16m, 120),
            Product("EMB-102", "Esquinero Carton", "Proteccion para tarimas y cajas grandes.", "Embalaje", "Paquete", 22m, 80),
            Product("SEG-101", "Botiquin Industrial", "Kit de primeros auxilios para area operativa.", "Seguridad", "Pieza", 58m, 8),
            Product("SEG-102", "Cono Vial Reflejante", "Senalizacion para patio de maniobras.", "Seguridad", "Pieza", 11m, 35),
            Product("LIM-101", "Limpiador Antiestatico", "Limpieza de estaciones y equipos electronicos.", "Limpieza", "Litro", 13m, 24)
        };

        var existingSkus = await db.Productos.Select(p => p.Sku).ToListAsync();
        var nuevos = productos
            .Where(p => !existingSkus.Contains(p.Sku))
            .Select(p => new Producto
            {
                Id = Guid.NewGuid(),
                Sku = p.Sku,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                CategoriaId = categorias.TryGetValue(p.Categoria, out var categoria)
                    ? categoria.Id
                    : categorias.Values.First().Id,
                UnidadMedida = p.UnidadMedida,
                PrecioUnitario = p.PrecioUnitario,
                StockMinimo = p.StockMinimo
            })
            .ToList();

        if (nuevos.Count == 0) return;

        db.Productos.AddRange(nuevos);
        await db.SaveChangesAsync();

        var random = new Random(20260722);
        foreach (var producto in nuevos)
        {
            foreach (var centro in centros)
            {
                var cantidad = random.Next(producto.StockMinimo, Math.Max(producto.StockMinimo * 6, producto.StockMinimo + 40));
                db.Stocks.Add(new Stock
                {
                    Id = Guid.NewGuid(),
                    ProductoId = producto.Id,
                    CentroId = centro.Id,
                    Cantidad = cantidad,
                    Ubicacion = $"{(char)('A' + random.Next(0, 6))}-{random.Next(1, 20):00}",
                    Lote = $"{producto.Sku}-X{random.Next(100, 999)}",
                    FechaCaducidad = producto.Sku.StartsWith("INS") || producto.Sku.StartsWith("LIM")
                        ? DateTime.UtcNow.AddMonths(random.Next(5, 20))
                        : null
                });

                for (var i = 0; i < 12; i++)
                {
                    var tipo = i % 3 == 0 ? "Ajuste" : i % 2 == 0 ? "Entrada" : "Salida";
                    var movimiento = random.Next(2, 40);
                    var stockAnterior = random.Next(producto.StockMinimo, Math.Max(producto.StockMinimo * 4, producto.StockMinimo + 50));
                    db.Movimientos.Add(new Movimiento
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = producto.Id,
                        CentroId = centro.Id,
                        Tipo = tipo,
                        Cantidad = movimiento,
                        StockAnterior = stockAnterior,
                        StockNuevo = tipo == "Salida" ? Math.Max(0, stockAnterior - movimiento) : stockAnterior + movimiento,
                        Motivo = tipo == "Entrada" ? "Compra demo" : tipo == "Salida" ? "Pedido demo" : "Ajuste por auditoria demo",
                        Referencia = $"{tipo[..2].ToUpperInvariant()}-{random.Next(10000, 99999)}",
                        Lote = $"{producto.Sku}-X{random.Next(100, 999)}",
                        FechaMovimiento = DateTime.UtcNow.AddDays(-random.Next(0, 45)).AddHours(-random.Next(0, 23)),
                        FechaCaducidad = producto.Sku.StartsWith("INS") || producto.Sku.StartsWith("LIM")
                            ? DateTime.UtcNow.AddMonths(random.Next(5, 20))
                            : null
                    });
                }
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seed demo productos extendidos: {Cantidad} productos agregados.", nuevos.Count);
    }

    private static (string Sku, string Nombre, string Descripcion, string Categoria, string UnidadMedida, decimal PrecioUnitario, int StockMinimo)
        Product(string sku, string nombre, string descripcion, string categoria, string unidadMedida, decimal precioUnitario, int stockMinimo)
        => (sku, nombre, descripcion, categoria, unidadMedida, precioUnitario, stockMinimo);
}
