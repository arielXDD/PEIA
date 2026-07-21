using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Identity;
using PEIA.Shared.Infra.Logistics;

namespace PEIA.Shared.Infra.Seed;

public static class SeedDataPedidosVariadosDemo
{
    public static async Task EnsureAsync(PeiaDbContext db, UserManager<Usuario> userManager, ILogger logger)
    {
        var centros = await db.Centros.OrderBy(c => c.Codigo).ToListAsync();
        if (centros.Count == 0) return;

        var transportista = await userManager.FindByEmailAsync("logistica@peia.com")
            ?? await userManager.FindByEmailAsync("carlos.log@peia.com");

        var rutas = await EnsureRutasAsync(db, centros);
        var existentes = (await db.Pedidos.Select(p => p.Codigo).ToListAsync()).ToHashSet();
        var clientes = new[]
        {
            "Cliente Demo Norte", "Distribuidora Sol", "Ferreteria Industrial", "Hospital Regional",
            "Autopartes Express", "Supermercado Central", "Manufactura Orion", "Comercial Santa Fe",
            "Refaccionaria Valle", "Laboratorio Nova", "Farmacia Uno", "Servicios Delta"
        };
        var estados = new[] { "Creado", "Asignado", "EnRuta", "Entregado", "Cancelado" };
        var random = new Random(2026072402);
        var nuevosPedidos = new List<Pedido>();

        for (var i = 1; i <= 150; i++)
        {
            var codigo = $"VAR-PED-{i:000}";
            if (existentes.Contains(codigo)) continue;

            var estado = estados[random.Next(estados.Length)];
            var fechaPedido = DateTime.UtcNow.AddDays(-random.Next(0, 45)).AddHours(-random.Next(0, 18));
            var fechaEntrega = fechaPedido.AddHours(random.Next(8, 96));
            if (i % 7 == 0 && estado != "Entregado")
            {
                fechaEntrega = DateTime.UtcNow.AddHours(-random.Next(2, 36));
            }

            var pedido = new Pedido
            {
                Id = Guid.NewGuid(),
                Codigo = codigo,
                Cliente = clientes[i % clientes.Length],
                DireccionEntrega = $"Av. Logistica Demo {500 + i}, Zona {i % 9 + 1}",
                CentroId = centros[i % centros.Count].Id,
                RutaId = rutas[i % rutas.Count].Id,
                TransportistaId = estado == "Creado" ? null : transportista?.Id,
                Estado = estado,
                FechaPedido = fechaPedido,
                FechaEstimadaEntrega = fechaEntrega
            };

            nuevosPedidos.Add(pedido);
            db.Pedidos.Add(pedido);
        }

        foreach (var pedido in nuevosPedidos)
        {
            var vencido = pedido.Estado != "Entregado" && pedido.FechaEstimadaEntrega < DateTime.UtcNow;
            db.SLAs.Add(new SLA
            {
                Id = Guid.NewGuid(),
                PedidoId = pedido.Id,
                TiempoLimite = pedido.FechaEstimadaEntrega,
                EstadoSLA = pedido.Estado == "Entregado" ? "Cumplido" : vencido ? "Incumplido" : "EnRiesgo",
                FechaResolucion = pedido.Estado == "Entregado" ? pedido.FechaEstimadaEntrega.AddHours(-1) : null
            });

            db.EntregaEstados.Add(new EntregaEstado
            {
                Id = Guid.NewGuid(),
                PedidoId = pedido.Id,
                Estado = "Creado",
                Descripcion = "Pedido demo variado creado para pruebas de logistica.",
                FechaActualizacion = pedido.FechaPedido,
                ActualizadoPorId = transportista?.Id
            });

            if (pedido.Estado is "Asignado" or "EnRuta" or "Entregado")
            {
                db.EntregaEstados.Add(new EntregaEstado
                {
                    Id = Guid.NewGuid(),
                    PedidoId = pedido.Id,
                    Estado = pedido.Estado,
                    Descripcion = $"Pedido demo actualizado a {pedido.Estado}.",
                    Latitud = 19.410000m + random.Next(1, 500) / 10000m,
                    Longitud = -99.180000m - random.Next(1, 500) / 10000m,
                    FechaActualizacion = pedido.FechaPedido.AddHours(random.Next(1, 12)),
                    ActualizadoPorId = transportista?.Id
                });
            }
        }

        await db.SaveChangesAsync();

        // Forzar colores: actualizar todos los pedidos existentes a un estado aleatorio y fecha reciente
        // Esto asegura que la gráfica del dashboard siempre muestre todos los colores incluso si la DB ya estaba creada
        var todosLosPedidos = await db.Pedidos.ToListAsync();
        foreach (var p in todosLosPedidos)
        {
            p.Estado = estados[random.Next(estados.Length)];
            p.FechaPedido = DateTime.UtcNow.AddDays(-random.Next(0, 7)).AddHours(-random.Next(0, 24));
        }
        await db.SaveChangesAsync();

        logger.LogInformation("Seed pedidos variados demo agregado: {Cantidad} pedidos.", nuevosPedidos.Count);
    }

    private static async Task<List<Ruta>> EnsureRutasAsync(PeiaDbContext db, List<Centro> centros)
    {
        var rutas = new[]
        {
            Route("VAR Ruta Nocturna Norte", "Bodega Norte", "Parque Industrial Norte", 28m),
            Route("VAR Ruta Hospitales", "CEDIS Central", "Corredor Medico", 34m),
            Route("VAR Ruta Retail Este", "Bodega Este", "Centros Comerciales Oriente", 22m),
            Route("VAR Ruta Mayorista Oeste", "Bodega Oeste", "Mercado Mayorista", 41m),
            Route("VAR Ruta Express Sur", "Bodega Sur", "Zona Sur Metropolitana", 18m)
        };

        var existentes = (await db.Rutas.Select(r => r.Nombre).ToListAsync()).ToHashSet();
        foreach (var ruta in rutas.Where(r => !existentes.Contains(r.Nombre)))
        {
            db.Rutas.Add(new Ruta
            {
                Id = Guid.NewGuid(),
                Nombre = ruta.Nombre,
                Origen = centros.FirstOrDefault(c => c.Nombre == ruta.Origen)?.Nombre ?? centros[0].Nombre,
                Destino = ruta.Destino,
                DistanciaKm = ruta.DistanciaKm,
                Activa = true
            });
        }

        await db.SaveChangesAsync();
        return await db.Rutas.Where(r => r.Nombre.StartsWith("VAR Ruta")).OrderBy(r => r.Nombre).ToListAsync();
    }

    private static (string Nombre, string Origen, string Destino, decimal DistanciaKm)
        Route(string nombre, string origen, string destino, decimal distanciaKm)
        => (nombre, origen, destino, distanciaKm);
}
