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
            db.Centros.AddRange(
                new Centro { Id = Guid.NewGuid(), Nombre = "Bodega Norte", Codigo = "BN-01", Direccion = "Av. Industrial Norte 100", Activo = true },
                new Centro { Id = Guid.NewGuid(), Nombre = "Bodega Sur",   Codigo = "BS-01", Direccion = "Blvd. Sur 450",           Activo = true }
            );
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

        await SeedBusinessDataAsync(db, logger, userManager);
    }

    private record SeedUsuario(
        string NombreCompleto,
        string Username,
        string Email,
        string Rol);

    private static async Task SeedBusinessDataAsync(Data.PeiaDbContext db, ILogger logger, UserManager<Usuario> userManager)
    {
        if (await db.Categorias.AnyAsync()) return;

        logger.LogInformation("Iniciando creacion de datos de negocio (Categorias, Productos, Stocks, Pedidos)...");

        // 1. Categorías
        var catElectronica = new PEIA.Shared.Infra.Inventory.Categoria { Id = Guid.NewGuid(), Nombre = "Electrónica" };
        var catInsumos = new PEIA.Shared.Infra.Inventory.Categoria { Id = Guid.NewGuid(), Nombre = "Insumos" };
        var catRefacciones = new PEIA.Shared.Infra.Inventory.Categoria { Id = Guid.NewGuid(), Nombre = "Refacciones" };
        db.Categorias.AddRange(catElectronica, catInsumos, catRefacciones);

        // 2. Productos
        var productos = new List<PEIA.Shared.Infra.Inventory.Producto>
        {
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "ELEC-001", Nombre = "Laptop Pro", CategoriaId = catElectronica.Id, PrecioUnitario = 1500m, StockMinimo = 10 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "ELEC-002", Nombre = "Monitor 24", CategoriaId = catElectronica.Id, PrecioUnitario = 200m, StockMinimo = 20 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "INS-001", Nombre = "Papel Carta Caja", CategoriaId = catInsumos.Id, PrecioUnitario = 50m, StockMinimo = 50 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "INS-002", Nombre = "Tinta Impresora", CategoriaId = catInsumos.Id, PrecioUnitario = 30m, StockMinimo = 30 },
            new PEIA.Shared.Infra.Inventory.Producto { Id = Guid.NewGuid(), Sku = "REF-001", Nombre = "Filtro Aceite", CategoriaId = catRefacciones.Id, PrecioUnitario = 15m, StockMinimo = 100 },
        };
        db.Productos.AddRange(productos);

        // 3. Centros
        var centros = await db.Centros.ToListAsync();
        var centroNorte = centros.FirstOrDefault(c => c.Nombre == "Bodega Norte");
        var centroSur = centros.FirstOrDefault(c => c.Nombre == "Bodega Sur");

        if (centroNorte != null && centroSur != null)
        {
            // 4. Stocks
            var r = new Random();
            foreach (var p in productos)
            {
                db.Stocks.Add(new PEIA.Shared.Infra.Inventory.Stock { Id = Guid.NewGuid(), ProductoId = p.Id, CentroId = centroNorte.Id, Cantidad = r.Next(20, 150), Ubicacion = "Estante A" });
                db.Stocks.Add(new PEIA.Shared.Infra.Inventory.Stock { Id = Guid.NewGuid(), ProductoId = p.Id, CentroId = centroSur.Id, Cantidad = r.Next(10, 80), Ubicacion = "Estante B" });

                // 5. Movimientos (Histórico para gráficas)
                for (int i = 0; i < 15; i++)
                {
                    var diasAtras = r.Next(1, 30);
                    var esEntrada = r.Next(0, 2) == 0;
                    var qty = r.Next(5, 50);
                    db.Movimientos.Add(new PEIA.Shared.Infra.Inventory.Movimiento
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = p.Id,
                        CentroId = r.Next(0, 2) == 0 ? centroNorte.Id : centroSur.Id,
                        Tipo = esEntrada ? "Entrada" : "Salida",
                        Cantidad = qty,
                        FechaMovimiento = DateTime.UtcNow.AddDays(-diasAtras),
                        Motivo = esEntrada ? "Compra a proveedor" : "Venta",
                        Referencia = $"REF-{r.Next(1000, 9999)}"
                    });
                }
            }
        }

        // 6. Rutas
        var ruta1 = new PEIA.Shared.Infra.Logistics.Ruta { Id = Guid.NewGuid(), Nombre = "Ruta Norte Express", Origen = "Bodega Norte", Destino = "Zona Industrial", DistanciaKm = 25m };
        var ruta2 = new PEIA.Shared.Infra.Logistics.Ruta { Id = Guid.NewGuid(), Nombre = "Ruta Sur Periférico", Origen = "Bodega Sur", Destino = "Centro Histórico", DistanciaKm = 15m };
        db.Rutas.AddRange(ruta1, ruta2);

        // 7. Pedidos
        var transportista = await userManager.FindByEmailAsync("logistica@peia.com");
        if (centroNorte != null && transportista != null)
        {
            var pedidos = new List<PEIA.Shared.Infra.Logistics.Pedido>
            {
                new PEIA.Shared.Infra.Logistics.Pedido { Id = Guid.NewGuid(), Codigo = "PED-001", Cliente = "Tech Corp", CentroId = centroNorte.Id, RutaId = ruta1.Id, TransportistaId = transportista.Id, Estado = "Entregado", FechaPedido = DateTime.UtcNow.AddDays(-2), FechaEstimadaEntrega = DateTime.UtcNow.AddDays(-1) },
                new PEIA.Shared.Infra.Logistics.Pedido { Id = Guid.NewGuid(), Codigo = "PED-002", Cliente = "Servicios M", CentroId = centroNorte.Id, RutaId = ruta1.Id, TransportistaId = transportista.Id, Estado = "EnRuta", FechaPedido = DateTime.UtcNow.AddDays(-1), FechaEstimadaEntrega = DateTime.UtcNow.AddHours(2) },
                new PEIA.Shared.Infra.Logistics.Pedido { Id = Guid.NewGuid(), Codigo = "PED-003", Cliente = "Global INC", CentroId = centroNorte.Id, Estado = "Creado", FechaPedido = DateTime.UtcNow, FechaEstimadaEntrega = DateTime.UtcNow.AddDays(1) },
            };
            db.Pedidos.AddRange(pedidos);
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Datos de negocio sembrados exitosamente.");
    }
}
