using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Inventory;

namespace PEIA.Shared.Infra.Seed;

public static class SeedDataProductosVariadosDemo
{
    public static async Task EnsureAsync(PeiaDbContext db, ILogger logger)
    {
        var centros = await db.Centros.OrderBy(c => c.Codigo).ToListAsync();
        if (centros.Count == 0) return;

        var categorias = await EnsureCategoriasAsync(db);
        var productos = new[]
        {
            Product("VAR-ELEC-001", "Handheld Android Zebra", "Terminal movil para conteo, surtido y escaneo en pasillo.", "Electronica", "Pieza", 680m, 6),
            Product("VAR-ELEC-002", "Antena WiFi Industrial", "Punto de cobertura para zonas de baja senal.", "Electronica", "Pieza", 145m, 8),
            Product("VAR-ELEC-003", "Base de Carga Multiple", "Estacion de carga para dispositivos de operacion.", "Electronica", "Pieza", 210m, 5),
            Product("VAR-INS-001", "Etiquetas Frio 4x6", "Etiqueta adhesiva para producto refrigerado.", "Insumos", "Rollo", 24m, 90),
            Product("VAR-INS-002", "Marcador Permanente Caja", "Marcador para identificacion manual de tarimas.", "Insumos", "Caja", 19m, 55),
            Product("VAR-REF-001", "Balero de Rodillo", "Pieza para mantenimiento preventivo de conveyors.", "Refacciones", "Pieza", 17m, 24),
            Product("VAR-REF-002", "Kit Tornilleria Banda", "Paquete para correccion rapida de bandas.", "Refacciones", "Kit", 31m, 18),
            Product("VAR-EMB-001", "Fleje Plastico 12mm", "Fleje para asegurar cajas y tarimas.", "Embalaje", "Rollo", 29m, 75),
            Product("VAR-EMB-002", "Pelicula Stretch Negra", "Material de embalaje para producto sensible.", "Embalaje", "Rollo", 14m, 110),
            Product("VAR-SEG-001", "Arnes de Seguridad", "Equipo de proteccion para trabajo en altura.", "Seguridad", "Pieza", 86m, 9),
            Product("VAR-SEG-002", "Lampara de Emergencia", "Equipo de seguridad para rutas de evacuacion.", "Seguridad", "Pieza", 36m, 14),
            Product("VAR-LIM-001", "Absorbente Granulado", "Consumible para derrames en anden y patio.", "Limpieza", "Saco", 27m, 22)
        };

        var existingSkus = (await db.Productos.Select(p => p.Sku).ToListAsync()).ToHashSet();
        var nuevos = productos
            .Where(p => !existingSkus.Contains(p.Sku))
            .Select(p => new Producto
            {
                Id = Guid.NewGuid(),
                Sku = p.Sku,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                CategoriaId = categorias[p.Categoria],
                UnidadMedida = p.UnidadMedida,
                PrecioUnitario = p.PrecioUnitario,
                StockMinimo = p.StockMinimo
            })
            .ToList();

        if (nuevos.Count == 0) return;

        db.Productos.AddRange(nuevos);
        await db.SaveChangesAsync();

        var random = new Random(2026072401);
        foreach (var producto in nuevos)
        {
            foreach (var centro in centros)
            {
                var lowStock = random.Next(0, 5) == 0;
                var cantidad = lowStock
                    ? Math.Max(1, producto.StockMinimo - random.Next(1, Math.Max(2, producto.StockMinimo)))
                    : random.Next(producto.StockMinimo, Math.Max(producto.StockMinimo * 8, producto.StockMinimo + 80));

                db.Stocks.Add(new Stock
                {
                    Id = Guid.NewGuid(),
                    ProductoId = producto.Id,
                    CentroId = centro.Id,
                    Cantidad = cantidad,
                    Ubicacion = $"{(char)('A' + random.Next(0, 7))}-{random.Next(1, 25):00}",
                    Lote = $"{producto.Sku}-V{random.Next(100, 999)}",
                    FechaCaducidad = producto.Sku.Contains("-INS-") || producto.Sku.Contains("-LIM-")
                        ? DateTime.UtcNow.AddMonths(random.Next(3, 18))
                        : null
                });

                for (var i = 0; i < 16; i++)
                {
                    var tipo = i % 4 == 0 ? "Ajuste" : i % 2 == 0 ? "Entrada" : "Salida";
                    var movimiento = random.Next(2, 65);
                    var anterior = random.Next(producto.StockMinimo, Math.Max(producto.StockMinimo * 6, producto.StockMinimo + 70));
                    db.Movimientos.Add(new Movimiento
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = producto.Id,
                        CentroId = centro.Id,
                        Tipo = tipo,
                        Cantidad = movimiento,
                        StockAnterior = anterior,
                        StockNuevo = tipo == "Salida" ? Math.Max(0, anterior - movimiento) : anterior + movimiento,
                        Motivo = tipo == "Entrada" ? "Recepcion demo variada" : tipo == "Salida" ? "Surtido demo variado" : "Conteo ciclico demo",
                        Referencia = $"VAR-{random.Next(10000, 99999)}",
                        Lote = $"{producto.Sku}-V{random.Next(100, 999)}",
                        FechaMovimiento = DateTime.UtcNow.AddDays(-random.Next(0, 75)).AddHours(-random.Next(0, 23))
                    });
                }
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seed productos variados demo agregado: {Cantidad} productos.", nuevos.Count);
    }

    private static async Task<Dictionary<string, Guid>> EnsureCategoriasAsync(PeiaDbContext db)
    {
        var nombres = new[] { "Electronica", "Insumos", "Refacciones", "Embalaje", "Seguridad", "Limpieza" };
        var existentes = await db.Categorias.ToDictionaryAsync(c => c.Nombre, c => c.Id);

        foreach (var nombre in nombres)
        {
            if (existentes.ContainsKey(nombre)) continue;

            var categoria = new Categoria
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = $"Categoria demo para {nombre.ToLowerInvariant()}."
            };
            db.Categorias.Add(categoria);
            existentes[nombre] = categoria.Id;
        }

        await db.SaveChangesAsync();
        return existentes;
    }

    private static (string Sku, string Nombre, string Descripcion, string Categoria, string UnidadMedida, decimal PrecioUnitario, int StockMinimo)
        Product(string sku, string nombre, string descripcion, string categoria, string unidadMedida, decimal precioUnitario, int stockMinimo)
        => (sku, nombre, descripcion, categoria, unidadMedida, precioUnitario, stockMinimo);
}
