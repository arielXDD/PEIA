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
            if (await userManager.FindByEmailAsync(u.Email) is not null) continue;

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

                // Asociar usuario con todas las bodegas creadas
                var centros = await db.Centros.ToListAsync();
                foreach (var centro in centros)
                {
                    db.UsuarioCentros.Add(new UsuarioCentro
                    {
                        UsuarioId = newUser.Id,
                        CentroId = centro.Id,
                        Activo = true
                    });
                }
                await db.SaveChangesAsync();
            }
            else
            {
                logger.LogError("Error al crear {User}: {Errors}", u.Username,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private record SeedUsuario(
        string NombreCompleto,
        string Username,
        string Email,
        string Rol);
}
