using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PEIA.Shared.Infra.Identity;

namespace PEIA.Shared.Infra.Seed;

public static class SeedData
{
    // Roles disponibles en el sistema
    public static readonly string[] Roles =
    [
        "Administrador",
        "OperadorInventario",
        "Logistica",
        "Reportes",
        "Supervisor"
    ];

    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        var userManager = services.GetRequiredService<UserManager<Usuario>>();
        var roleManager = services.GetRequiredService<RoleManager<Rol>>();
        var db          = services.GetRequiredService<Data.PeiaDbContext>();
        var isDemoDatabase = string.Equals(
            db.Database.GetDbConnection().Database,
            "peiadb_demo",
            StringComparison.OrdinalIgnoreCase);

        // ── Aplicar migraciones pendientes automáticamente
        await db.Database.MigrateAsync();

        // ── 1. Crear roles
        foreach (var roleName in Roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new Rol { Name = roleName });
                if (result.Succeeded)
                    logger.LogInformation("Rol creado: {Rol}", roleName);
            }
        }

        // ── 2. Crear centros base
        if (!await db.Centros.AnyAsync())
        {
            var centrosBase = new List<Centro>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Bodega Norte", Codigo = "BN-01", Direccion = "Av. Industrial Norte 100", Activo = true },
                new() { Id = Guid.NewGuid(), Nombre = "Bodega Sur",   Codigo = "BS-01", Direccion = "Blvd. Sur 450",           Activo = true }
            };

            if (isDemoDatabase)
            {
                centrosBase.AddRange(new[]
                {
                    new Centro { Id = Guid.NewGuid(), Nombre = "CEDIS Central", Codigo = "CC-01", Direccion = "Parque Logistico 200", Activo = true },
                    new Centro { Id = Guid.NewGuid(), Nombre = "Bodega Este", Codigo = "BE-01", Direccion = "Av. Oriente 740", Activo = true },
                    new Centro { Id = Guid.NewGuid(), Nombre = "Bodega Oeste", Codigo = "BO-01", Direccion = "Circuito Poniente 88", Activo = true }
                });
            }

            db.Centros.AddRange(centrosBase);
            await db.SaveChangesAsync();
            logger.LogInformation("Centros base creados.");
        }

        // ── 3. Crear un usuario por rol
        var usuarios = new[]
        {
            new SeedUsuario("Ariel Guevara",       "admin",       "admin@peia.com",       "Administrador"),
            new SeedUsuario("Julian Sierra",        "inventario",  "inventario@peia.com",  "OperadorInventario"),
            new SeedUsuario("Jose Villa",           "logistica",   "logistica@peia.com",   "Logistica"),
            new SeedUsuario("Jennifer Munoz",       "reportes",    "reportes@peia.com",    "Reportes"),
            new SeedUsuario("Mariano Sanchez",      "supervisor",  "supervisor@peia.com",  "Supervisor"),
        };

        if (isDemoDatabase)
        {
            usuarios = usuarios.Concat(new[]
            {
                new SeedUsuario("Julian David Sierra", "julian", "julian@peia.com", "OperadorInventario"),
                new SeedUsuario("Laura Hernandez", "laura.inv", "laura.inv@peia.com", "OperadorInventario"),
                new SeedUsuario("Carlos Ramirez", "carlos.log", "carlos.log@peia.com", "Logistica"),
                new SeedUsuario("Fernanda Lopez", "fernanda.rep", "fernanda.rep@peia.com", "Reportes"),
                new SeedUsuario("Miguel Torres", "miguel.sup", "miguel.sup@peia.com", "Supervisor"),
                new SeedUsuario("Ana Martinez", "ana.admin", "ana.admin@peia.com", "Administrador"),
                new SeedUsuario("Roberto Diaz", "roberto.inv", "roberto.inv@peia.com", "OperadorInventario"),
                new SeedUsuario("Sofia Navarro", "sofia.log", "sofia.log@peia.com", "Logistica")
            }).ToArray();
        }

        const string defaultPassword = "Peia2025!";

        foreach (var u in usuarios)
        {
            var existingUser = await userManager.FindByEmailAsync(u.Email);
            Usuario targetUser;
            
            if (existingUser is null)
            {
                var newUser = new Usuario
                {
                    Id              = Guid.NewGuid(),
                    UserName        = u.Username,
                    Email           = u.Email,
                    NombreCompleto  = u.NombreCompleto,
                    EmailConfirmed  = true,
                    Activo          = true,
                    FechaCreacion   = DateTime.UtcNow,
                };

                var result = await userManager.CreateAsync(newUser, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, u.Rol);
                    logger.LogInformation("Usuario creado: {User} [{Rol}]", u.Username, u.Rol);
                    targetUser = newUser;
                }
                else
                {
                    logger.LogError("Error al crear {User}: {Errors}", u.Username,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    continue;
                }
            }
            else
            {
                targetUser = existingUser;
            }

            // Reparar usuarios creados previamente sin el rol esperado.
            if (!await userManager.IsInRoleAsync(targetUser, u.Rol))
            {
                var roleResult = await userManager.AddToRoleAsync(targetUser, u.Rol);
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Rol {Rol} asignado al usuario existente: {User}", u.Rol, u.Username);
                }
                else
                {
                    logger.LogError("Error al asignar el rol {Rol} a {User}: {Errors}",
                        u.Rol,
                        u.Username,
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }

            // Ensure user has centers assigned
            var hasCenters = await db.UsuarioCentros.AnyAsync(uc => uc.UsuarioId == targetUser.Id);
            if (!hasCenters)
            {
                var centros = await db.Centros
                    .OrderBy(c => c.Codigo)
                    .Take(2)
                    .ToListAsync();
                foreach (var centro in centros)
                {
                    db.UsuarioCentros.Add(new UsuarioCentro
                    {
                        UsuarioId = targetUser.Id,
                        CentroId = centro.Id,
                        Activo = true
                    });
                }
                await db.SaveChangesAsync();
                logger.LogInformation("Centros asignados a usuario existente: {User}", u.Username);
            }
        }

        await SeedBusinessDataAsync(db, logger, userManager, isDemoDatabase);
    }

    private record SeedUsuario(
        string NombreCompleto,
        string Username,
        string Email,
        string Rol);

    private static async Task SeedBusinessDataAsync(Data.PeiaDbContext db, ILogger logger, UserManager<Usuario> userManager, bool isDemoDatabase)
    {
        if (await db.Categorias.AnyAsync())
        {
            if (isDemoDatabase)
            {
                await SeedDemoShowcaseDataAsync(db, logger, userManager);
            }
            return;
        }

        logger.LogInformation("Iniciando creacion de datos de negocio (Categorias, Productos, Stocks, Pedidos)...");

        // 1. Categorías
        var catElectronica = new PEIA.Shared.Infra.Inventory.Categoria { Id = Guid.NewGuid(), Nombre = "Electrónica", Descripcion = "Equipos y periféricos de operación." };
        var catInsumos = new PEIA.Shared.Infra.Inventory.Categoria { Id = Guid.NewGuid(), Nombre = "Insumos", Descripcion = "Material consumible para almacén y oficina." };
        var catRefacciones = new PEIA.Shared.Infra.Inventory.Categoria { Id = Guid.NewGuid(), Nombre = "Refacciones", Descripcion = "Piezas para mantenimiento y operación." };
        var catEmbalaje = new PEIA.Shared.Infra.Inventory.Categoria { Id = Guid.NewGuid(), Nombre = "Embalaje", Descripcion = "Material para empaque, surtido y envío." };
        var catSeguridad = new PEIA.Shared.Infra.Inventory.Categoria { Id = Guid.NewGuid(), Nombre = "Seguridad", Descripcion = "Equipo de protección y señalización." };
        var catLimpieza = new PEIA.Shared.Infra.Inventory.Categoria { Id = Guid.NewGuid(), Nombre = "Limpieza", Descripcion = "Consumibles de higiene y mantenimiento." };
        db.Categorias.AddRange(catElectronica, catInsumos, catRefacciones, catEmbalaje, catSeguridad, catLimpieza);

        // 2. Productos
        var productos = new List<PEIA.Shared.Infra.Inventory.Producto>
        {
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "ELEC-001", Nombre = "Laptop Pro 14", Descripcion = "Equipo portátil para supervisión de almacén.", CategoriaId = catElectronica.Id, PrecioUnitario = 1500m, StockMinimo = 10 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "ELEC-002", Nombre = "Monitor 24", Descripcion = "Monitor para estación de captura.", CategoriaId = catElectronica.Id, PrecioUnitario = 200m, StockMinimo = 20 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "ELEC-003", Nombre = "Scanner EAN Inalámbrico", Descripcion = "Lector de código de barras para entradas y salidas.", CategoriaId = catElectronica.Id, PrecioUnitario = 125m, StockMinimo = 12 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "ELEC-004", Nombre = "Tablet Industrial", Descripcion = "Dispositivo para conteos cíclicos.", CategoriaId = catElectronica.Id, PrecioUnitario = 420m, StockMinimo = 8 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "INS-001", Nombre = "Papel Carta Caja", CategoriaId = catInsumos.Id, PrecioUnitario = 50m, StockMinimo = 50 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "INS-002", Nombre = "Tinta Impresora", CategoriaId = catInsumos.Id, PrecioUnitario = 30m, StockMinimo = 30 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "INS-003", Nombre = "Etiquetas Adhesivas 4x6", CategoriaId = catInsumos.Id, PrecioUnitario = 18m, StockMinimo = 80 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "INS-004", Nombre = "Ribbon Térmico", CategoriaId = catInsumos.Id, PrecioUnitario = 22m, StockMinimo = 40 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "REF-001", Nombre = "Filtro Aceite", CategoriaId = catRefacciones.Id, PrecioUnitario = 15m, StockMinimo = 100 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "REF-002", Nombre = "Banda Transportadora 2m", CategoriaId = catRefacciones.Id, PrecioUnitario = 95m, StockMinimo = 6 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "REF-003", Nombre = "Rodillo de Carga", CategoriaId = catRefacciones.Id, PrecioUnitario = 38m, StockMinimo = 15 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "EMB-001", Nombre = "Caja Estándar L", CategoriaId = catEmbalaje.Id, PrecioUnitario = 3.5m, StockMinimo = 300 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "EMB-002", Nombre = "Caja Estándar M", CategoriaId = catEmbalaje.Id, PrecioUnitario = 2.8m, StockMinimo = 400 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "EMB-003", Nombre = "Rollo de Emplaye", CategoriaId = catEmbalaje.Id, PrecioUnitario = 11m, StockMinimo = 75 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "SEG-001", Nombre = "Casco de Seguridad", CategoriaId = catSeguridad.Id, PrecioUnitario = 16m, StockMinimo = 25 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "SEG-002", Nombre = "Chaleco Reflectante", CategoriaId = catSeguridad.Id, PrecioUnitario = 9m, StockMinimo = 40 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "SEG-003", Nombre = "Guantes Anticorte", CategoriaId = catSeguridad.Id, PrecioUnitario = 7m, StockMinimo = 60 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "LIM-001", Nombre = "Desengrasante Industrial", CategoriaId = catLimpieza.Id, PrecioUnitario = 12m, StockMinimo = 35 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "LIM-002", Nombre = "Paños Microfibra", CategoriaId = catLimpieza.Id, PrecioUnitario = 6m, StockMinimo = 50 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "LIM-003", Nombre = "Kit Derrames", CategoriaId = catLimpieza.Id, PrecioUnitario = 48m, StockMinimo = 10 },
        };
        db.Productos.AddRange(productos);

        // 3. Centros
        var centros = await db.Centros.OrderBy(c => c.Codigo).ToListAsync();
        var centroNorte = centros.FirstOrDefault(c => c.Nombre == "Bodega Norte");
        var centroSur = centros.FirstOrDefault(c => c.Nombre == "Bodega Sur");
        var usuarioInventario = await userManager.FindByEmailAsync("inventario@peia.com");

        if (centros.Count > 0)
        {
            // 4. Stocks
            var r = new Random(20260720);
            foreach (var p in productos)
            {
                foreach (var centro in centros)
                {
                    var stockBase = r.Next(p.StockMinimo / 2, Math.Max(p.StockMinimo * 4, p.StockMinimo + 25));
                    var shouldBeLow = p.Sku is "ELEC-004" or "REF-002" or "SEG-003" or "LIM-003";
                    var cantidad = shouldBeLow && (centro.Codigo == "BN-01" || centro.Codigo == "BS-01")
                        ? Math.Max(1, p.StockMinimo - r.Next(1, Math.Max(2, p.StockMinimo / 2)))
                        : stockBase;

                    db.Stocks.Add(new PEIA.Shared.Infra.Inventory.Stock
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = p.Id,
                        CentroId = centro.Id,
                        Cantidad = cantidad,
                        Ubicacion = $"{(char)('A' + r.Next(0, 5))}-{r.Next(1, 12):00}",
                        Lote = $"{p.Sku}-L{r.Next(100, 999)}",
                        FechaCaducidad = p.Sku.StartsWith("LIM") || p.Sku.StartsWith("INS")
                            ? DateTime.UtcNow.AddMonths(r.Next(6, 24))
                            : null
                    });
                }

                // 5. Movimientos (Histórico para gráficas)
                for (int i = 0; i < 30; i++)
                {
                    var diasAtras = r.Next(1, 30);
                    var esEntrada = r.Next(0, 2) == 0;
                    var qty = r.Next(5, 50);
                    var stockAnterior = r.Next(p.StockMinimo, Math.Max(p.StockMinimo + 50, p.StockMinimo * 5));
                    db.Movimientos.Add(new PEIA.Shared.Infra.Inventory.Movimiento
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = p.Id,
                        CentroId = centros[r.Next(0, centros.Count)].Id,
                        Tipo = esEntrada ? "Entrada" : "Salida",
                        Cantidad = qty,
                        StockAnterior = stockAnterior,
                        StockNuevo = esEntrada ? stockAnterior + qty : Math.Max(0, stockAnterior - qty),
                        FechaMovimiento = DateTime.UtcNow.AddDays(-diasAtras),
                        Motivo = esEntrada ? "Compra a proveedor" : "Salida por pedido",
                        Referencia = $"REF-{r.Next(1000, 9999)}",
                        Lote = $"{p.Sku}-L{r.Next(100, 999)}",
                        FechaCaducidad = p.Sku.StartsWith("LIM") || p.Sku.StartsWith("INS")
                            ? DateTime.UtcNow.AddMonths(r.Next(6, 24))
                            : null,
                        UsuarioId = usuarioInventario?.Id
                    });
                }
            }
        }

        // 6. Rutas
        var rutas = new List<PEIA.Shared.Infra.Logistics.Ruta>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Ruta Norte Express", Origen = "Bodega Norte", Destino = "Zona Industrial", DistanciaKm = 25m },
            new() { Id = Guid.NewGuid(), Nombre = "Ruta Sur Periférico", Origen = "Bodega Sur", Destino = "Centro Histórico", DistanciaKm = 15m },
            new() { Id = Guid.NewGuid(), Nombre = "Ruta CEDIS Aeropuerto", Origen = "CEDIS Central", Destino = "Parque Aeroindustrial", DistanciaKm = 42m },
            new() { Id = Guid.NewGuid(), Nombre = "Ruta Este Retail", Origen = "Bodega Este", Destino = "Plaza Oriente", DistanciaKm = 18m },
            new() { Id = Guid.NewGuid(), Nombre = "Ruta Oeste Mayorista", Origen = "Bodega Oeste", Destino = "Mercado de Abastos", DistanciaKm = 31m }
        };
        db.Rutas.AddRange(rutas);

        // 7. Pedidos
        var transportista = await userManager.FindByEmailAsync("logistica@peia.com");
        if (centroNorte != null && transportista != null)
        {
            var clientes = new[]
            {
                "Tech Corp", "Servicios M", "Global INC", "Comercial Rivera", "Distribuidora Centro",
                "Hospital San Angel", "Talleres Monterrey", "Retail Norte", "Refacciones MX",
                "Oficinas Delta", "Manufacturas Q", "Alimentos La Sierra"
            };
            var estados = new[] { "Creado", "Asignado", "EnRuta", "Entregado", "Cancelado" };
            var pedidos = new List<PEIA.Shared.Infra.Logistics.Pedido>();
            for (int i = 0; i < 24; i++)
            {
                var centro = centros[i % centros.Count];
                var estado = estados[i % estados.Length];
                var ruta = rutas[i % rutas.Count];
                pedidos.Add(new PEIA.Shared.Infra.Logistics.Pedido
                {
                    Id = Guid.NewGuid(),
                    Codigo = $"PED-{(i + 1):000}",
                    Cliente = clientes[i % clientes.Length],
                    DireccionEntrega = $"Calle Operativa {100 + i}, Zona {((i % 5) + 1)}",
                    CentroId = centro.Id,
                    RutaId = ruta.Id,
                    TransportistaId = estado == "Creado" ? null : transportista.Id,
                    Estado = estado,
                    FechaPedido = DateTime.UtcNow.AddDays(-10 + i % 10),
                    FechaEstimadaEntrega = DateTime.UtcNow.AddHours((i % 8) - 2)
                });
            }
            db.Pedidos.AddRange(pedidos);

            foreach (var pedido in pedidos)
            {
                var incumplido = pedido.Estado != "Entregado" && pedido.FechaEstimadaEntrega < DateTime.UtcNow;
                db.SLAs.Add(new PEIA.Shared.Infra.Logistics.SLA
                {
                    Id = Guid.NewGuid(),
                    PedidoId = pedido.Id,
                    TiempoLimite = pedido.FechaEstimadaEntrega,
                    EstadoSLA = pedido.Estado == "Entregado" ? "Cumplido" : incumplido ? "Incumplido" : "EnRiesgo",
                    FechaResolucion = pedido.Estado == "Entregado" ? pedido.FechaEstimadaEntrega.AddHours(-1) : null
                });

                db.EntregaEstados.Add(new PEIA.Shared.Infra.Logistics.EntregaEstado
                {
                    Id = Guid.NewGuid(),
                    PedidoId = pedido.Id,
                    Estado = "Creado",
                    Descripcion = "Pedido registrado en ambiente demo.",
                    FechaActualizacion = pedido.FechaPedido,
                    ActualizadoPorId = transportista.Id
                });

                if (pedido.Estado is "Asignado" or "EnRuta" or "Entregado")
                {
                    db.EntregaEstados.Add(new PEIA.Shared.Infra.Logistics.EntregaEstado
                    {
                        Id = Guid.NewGuid(),
                        PedidoId = pedido.Id,
                        Estado = pedido.Estado,
                        Descripcion = $"Pedido actualizado a {pedido.Estado}.",
                        Latitud = 19.4326m + (pedido.Codigo[^1] - '0') / 1000m,
                        Longitud = -99.1332m - (pedido.Codigo[^1] - '0') / 1000m,
                        FechaActualizacion = pedido.FechaPedido.AddHours(4),
                        ActualizadoPorId = transportista.Id
                    });
                }
            }
        }

        // 8. Notificaciones y reglas de automatizacion
        if (centros.Count > 0)
        {
            db.Notificaciones.AddRange(
                centros.Take(4).SelectMany((centro, index) => new[]
                {
                    new PEIA.Shared.Infra.Notifications.Notificacion
                    {
                        Id = Guid.NewGuid(),
                        CentroId = centro.Id,
                        Tipo = "stock_critico",
                        Titulo = $"Stock bajo en {centro.Nombre}",
                        Descripcion = "Producto por debajo del mínimo configurado. Revisar reposición.",
                        Leida = index % 2 == 0,
                        FechaCreacion = DateTime.UtcNow.AddHours(-index - 1)
                    },
                    new PEIA.Shared.Infra.Notifications.Notificacion
                    {
                        Id = Guid.NewGuid(),
                        CentroId = centro.Id,
                        Tipo = "sla_vencido",
                        Titulo = $"Pedido con SLA en riesgo",
                        Descripcion = "Existe al menos un pedido cercano a vencer o vencido.",
                        Leida = false,
                        FechaCreacion = DateTime.UtcNow.AddHours(-index - 3)
                    }
                })
            );
        }

        db.ReglasAutomatizacion.AddRange(
            new PEIA.Shared.Infra.Automation.ReglaAutomatizacion
            {
                Id = Guid.NewGuid(),
                Nombre = "Alerta automática de stock bajo",
                EventoOrigen = "stock_critico",
                Condicion = "Stock < Minimo",
                Accion = "notificar",
                Responsable = "OperadorInventario"
            },
            new PEIA.Shared.Infra.Automation.ReglaAutomatizacion
            {
                Id = Guid.NewGuid(),
                Nombre = "Aviso de pedido nuevo",
                EventoOrigen = "nuevo_pedido",
                Condicion = "Todos",
                Accion = "notificar",
                Responsable = "Logistica"
            },
            new PEIA.Shared.Infra.Automation.ReglaAutomatizacion
            {
                Id = Guid.NewGuid(),
                Nombre = "Seguimiento de SLA vencido",
                EventoOrigen = "sla_vencido",
                Condicion = "Todos",
                Accion = "notificar",
                Responsable = "Supervisor"
            }
        );

        await db.SaveChangesAsync();
        logger.LogInformation("Datos de negocio sembrados exitosamente.");

        if (isDemoDatabase)
        {
            await SeedDemoShowcaseDataAsync(db, logger, userManager);
        }
    }

    private static async Task SeedDemoShowcaseDataAsync(Data.PeiaDbContext db, ILogger logger, UserManager<Usuario> userManager)
    {
        var centros = await db.Centros.OrderBy(c => c.Codigo).ToListAsync();
        if (centros.Count == 0) return;

        var categorias = await db.Categorias.ToDictionaryAsync(c => c.Nombre);
        var categoriaObjetivo = categorias.TryGetValue("Insumos", out var insumos)
            ? insumos
            : await db.Categorias.OrderBy(c => c.Nombre).FirstAsync();

        var productosExtra = new[]
        {
            ("ELEC-005", "Router Industrial LTE", "Conectividad de respaldo para zonas de monitoreo.", "Pieza", 185m, 9),
            ("ELEC-006", "UPS 1500VA", "Respaldo eléctrico para estaciones de captura y cámaras.", "Pieza", 240m, 12),
            ("INS-005", "Formato de Recepción", "Paquete de formatos impresos para entradas de mercancía.", "Paquete", 14m, 120),
            ("INS-006", "Cinta de Seguridad", "Cinta inviolable para preparación de pedidos.", "Rollo", 8m, 160),
            ("REF-004", "Sensor de Banda", "Sensor para mantenimiento de línea transportadora.", "Pieza", 65m, 18),
            ("REF-005", "Motor Reductor", "Refacción crítica para equipos de surtido.", "Pieza", 310m, 5),
            ("EMB-004", "Tarima Plástica", "Tarima reutilizable para alto volumen.", "Pieza", 38m, 45),
            ("EMB-005", "Separador Corrugado", "Separador para protección de producto frágil.", "Paquete", 12m, 90),
            ("SEG-004", "Lentes de Seguridad", "Protección ocular para operadores de almacén.", "Pieza", 6m, 70),
            ("LIM-004", "Sanitizante Concentrado", "Consumible de limpieza para zonas operativas.", "Litro", 9m, 55)
        };

        var productosExistentes = await db.Productos.Select(p => p.Sku).ToListAsync();
        var nuevosProductos = productosExtra
            .Where(p => !productosExistentes.Contains(p.Item1))
            .Select(p => new PEIA.Shared.Infra.Inventory.Producto
            {
                Id = Guid.NewGuid(),
                Sku = p.Item1,
                Nombre = p.Item2,
                Descripcion = p.Item3,
                UnidadMedida = p.Item4,
                PrecioUnitario = p.Item5,
                StockMinimo = p.Item6,
                CategoriaId = categoriaObjetivo.Id
            })
            .ToList();

        if (nuevosProductos.Count > 0)
        {
            db.Productos.AddRange(nuevosProductos);
            await db.SaveChangesAsync();
            logger.LogInformation("Productos demo adicionales creados: {Cantidad}", nuevosProductos.Count);
        }

        var productos = await db.Productos.OrderBy(p => p.Sku).ToListAsync();
        var random = new Random(20260721);
        foreach (var producto in productos)
        {
            foreach (var centro in centros)
            {
                var hasStock = await db.Stocks.AnyAsync(s => s.ProductoId == producto.Id && s.CentroId == centro.Id);
                if (hasStock) continue;

                db.Stocks.Add(new PEIA.Shared.Infra.Inventory.Stock
                {
                    Id = Guid.NewGuid(),
                    ProductoId = producto.Id,
                    CentroId = centro.Id,
                    Cantidad = random.Next(producto.StockMinimo, Math.Max(producto.StockMinimo * 5, producto.StockMinimo + 30)),
                    Ubicacion = $"{(char)('A' + random.Next(0, 6))}-{random.Next(1, 18):00}",
                    Lote = $"{producto.Sku}-D{random.Next(100, 999)}",
                    FechaCaducidad = producto.Sku.StartsWith("INS") || producto.Sku.StartsWith("LIM")
                        ? DateTime.UtcNow.AddMonths(random.Next(4, 18))
                        : null
                });
            }
        }

        var usuarioInventario = await userManager.FindByEmailAsync("julian@peia.com")
            ?? await userManager.FindByEmailAsync("inventario@peia.com");

        var movimientosActuales = await db.Movimientos.CountAsync();
        if (movimientosActuales < 1200)
        {
            var movimientosPorCrear = 1200 - movimientosActuales;
            for (var i = 0; i < movimientosPorCrear; i++)
            {
                var producto = productos[i % productos.Count];
                var centro = centros[(i + 2) % centros.Count];
                var tipo = i % 5 == 0 ? "Ajuste" : i % 2 == 0 ? "Entrada" : "Salida";
                var cantidad = random.Next(3, 85);
                var stockAnterior = random.Next(producto.StockMinimo, Math.Max(producto.StockMinimo * 7, producto.StockMinimo + 80));
                var stockNuevo = tipo == "Entrada"
                    ? stockAnterior + cantidad
                    : tipo == "Salida"
                        ? Math.Max(0, stockAnterior - cantidad)
                        : Math.Max(0, stockAnterior + random.Next(-12, 18));

                db.Movimientos.Add(new PEIA.Shared.Infra.Inventory.Movimiento
                {
                    Id = Guid.NewGuid(),
                    ProductoId = producto.Id,
                    CentroId = centro.Id,
                    Tipo = tipo,
                    Cantidad = cantidad,
                    StockAnterior = stockAnterior,
                    StockNuevo = stockNuevo,
                    FechaMovimiento = DateTime.UtcNow.AddDays(-random.Next(0, 60)).AddHours(-random.Next(0, 23)),
                    Motivo = tipo == "Entrada" ? "Recepción de proveedor" : tipo == "Salida" ? "Surtido de pedido" : "Conteo cíclico",
                    Referencia = $"{(tipo == "Entrada" ? "OC" : tipo == "Salida" ? "PED" : "AJ")}-{random.Next(10000, 99999)}",
                    Lote = $"{producto.Sku}-D{random.Next(100, 999)}",
                    FechaCaducidad = producto.Sku.StartsWith("INS") || producto.Sku.StartsWith("LIM")
                        ? DateTime.UtcNow.AddMonths(random.Next(4, 18))
                        : null,
                    UsuarioId = usuarioInventario?.Id
                });
            }
        }

        var rutas = await db.Rutas.OrderBy(r => r.Nombre).ToListAsync();
        var transportistas = await userManager.GetUsersInRoleAsync("Logistica");
        var transportista = transportistas.FirstOrDefault() ?? await userManager.FindByEmailAsync("logistica@peia.com");
        var pedidosActuales = await db.Pedidos.CountAsync();
        if (rutas.Count > 0 && transportista != null && pedidosActuales < 60)
        {
            var clientes = new[]
            {
                "AutoPartes Rivera", "Farmacia Central", "Hotel Alameda", "Mueblería Norte", "Clínica del Valle",
                "Papelería Express", "Constructora Atlas", "Refacciones Laguna", "Distribuidora Pacífico",
                "Supermercado El Roble", "Servicios Industriales Alfa", "Tecnología Aplicada MX"
            };
            var estados = new[] { "Creado", "Asignado", "EnRuta", "Entregado", "Cancelado" };
            for (var i = pedidosActuales + 1; i <= 60; i++)
            {
                var estado = estados[i % estados.Length];
                var pedido = new PEIA.Shared.Infra.Logistics.Pedido
                {
                    Id = Guid.NewGuid(),
                    Codigo = $"PED-{i:000}",
                    Cliente = clientes[i % clientes.Length],
                    DireccionEntrega = $"Av. Demo Operativa {200 + i}, Col. Zona {i % 8 + 1}",
                    CentroId = centros[i % centros.Count].Id,
                    RutaId = rutas[i % rutas.Count].Id,
                    TransportistaId = estado == "Creado" ? null : transportista.Id,
                    Estado = estado,
                    FechaPedido = DateTime.UtcNow.AddDays(-random.Next(0, 45)).AddHours(-random.Next(0, 12)),
                    FechaEstimadaEntrega = DateTime.UtcNow.AddHours(random.Next(-18, 48))
                };

                db.Pedidos.Add(pedido);
                var incumplido = pedido.Estado != "Entregado" && pedido.FechaEstimadaEntrega < DateTime.UtcNow;
                db.SLAs.Add(new PEIA.Shared.Infra.Logistics.SLA
                {
                    Id = Guid.NewGuid(),
                    PedidoId = pedido.Id,
                    TiempoLimite = pedido.FechaEstimadaEntrega,
                    EstadoSLA = pedido.Estado == "Entregado" ? "Cumplido" : incumplido ? "Incumplido" : "EnRiesgo",
                    FechaResolucion = pedido.Estado == "Entregado" ? pedido.FechaEstimadaEntrega.AddHours(-2) : null
                });
                db.EntregaEstados.Add(new PEIA.Shared.Infra.Logistics.EntregaEstado
                {
                    Id = Guid.NewGuid(),
                    PedidoId = pedido.Id,
                    Estado = "Creado",
                    Descripcion = "Pedido demo registrado para pruebas de reportes.",
                    FechaActualizacion = pedido.FechaPedido,
                    ActualizadoPorId = transportista.Id
                });
            }
        }

        var notificacionesActuales = await db.Notificaciones.CountAsync();
        if (notificacionesActuales < 18)
        {
            foreach (var centro in centros)
            {
                db.Notificaciones.Add(new PEIA.Shared.Infra.Notifications.Notificacion
                {
                    Id = Guid.NewGuid(),
                    CentroId = centro.Id,
                    Tipo = "camara",
                    Titulo = $"Cámara revisada en {centro.Nombre}",
                    Descripcion = "Monitoreo visual disponible para operación demo.",
                    Leida = false,
                    FechaCreacion = DateTime.UtcNow.AddMinutes(-random.Next(10, 240))
                });
            }
        }

        await db.SaveChangesAsync();
        await SeedDataProductosDemo.EnsureAsync(db, logger);
        await SeedDataNotificacionesDemo.EnsureAsync(db, logger);
        await SeedDataReglasDemo.EnsureAsync(db, logger);
        await SeedDataPedidosVariadosDemo.EnsureAsync(db, userManager, logger);
        logger.LogInformation("Datos demo complementarios verificados.");
    }
}
