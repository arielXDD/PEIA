using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PEIA.Shared.Infra.Configuration;
using PEIA.Shared.Infra.Identity;
using PEIA.Shared.Infra.Inventory;
using PEIA.Shared.Infra.Logistics;
using PEIA.Shared.Infra.Notifications;
using PEIA.Shared.Infra.Automation;
using PEIA.Shared.Infra.Cameras;

namespace PEIA.Shared.Infra.Data;

public class PeiaDbContext : IdentityDbContext<Usuario, Rol, Guid>
{
    public PeiaDbContext(DbContextOptions<PeiaDbContext> options) : base(options)
    {
    }

    public DbSet<Centro> Centros => Set<Centro>();
    public DbSet<UsuarioCentro> UsuarioCentros => Set<UsuarioCentro>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    // DbSets de Logística
    public DbSet<Ruta> Rutas => Set<Ruta>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<SLA> SLAs => Set<SLA>();
    public DbSet<EntregaEstado> EntregaEstados => Set<EntregaEstado>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<ReglaAutomatizacion> ReglasAutomatizacion => Set<ReglaAutomatizacion>();
    public DbSet<Captura> Capturas => Set<Captura>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuración de Schema Identity
        builder.Entity<Usuario>(b =>
        {
            b.ToTable("Usuarios");
            b.Property(u => u.NombreCompleto).HasMaxLength(200).IsRequired();
        });

        builder.Entity<Rol>(b =>
        {
            b.ToTable("Roles");
        });

        // Configuración Centro
        builder.Entity<Centro>(b =>
        {
            b.ToTable("Centros");
            b.HasKey(c => c.Id);
            b.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
            b.Property(c => c.Codigo).HasMaxLength(20).IsRequired();
            b.Property(c => c.Direccion).HasMaxLength(300);
            b.Property(c => c.Version).IsRowVersion();

            // Índice para búsqueda rápida y unicidad
            b.HasIndex(c => c.Codigo).IsUnique();
        });

        // Configuración UsuarioCentro
        builder.Entity<UsuarioCentro>(b =>
        {
            b.ToTable("UsuarioCentros");
            b.HasKey(uc => new { uc.UsuarioId, uc.CentroId });

            b.HasOne(uc => uc.Usuario)
                .WithMany(u => u.UsuarioCentros)
                .HasForeignKey(uc => uc.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(uc => uc.Centro)
                .WithMany(c => c.UsuarioCentros)
                .HasForeignKey(uc => uc.CentroId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<UserSession>(b =>
        {
            b.ToTable("UserSessions");
            b.HasKey(s => s.Id);
            b.Property(s => s.JwtId).HasMaxLength(120).IsRequired();
            b.Property(s => s.IpAddress).HasMaxLength(80);
            b.Property(s => s.UserAgent).HasMaxLength(500);
            b.HasIndex(s => s.JwtId).IsUnique();
            b.HasIndex(s => new { s.UsuarioId, s.Revocada, s.FechaExpiracion });

            b.HasOne(s => s.Usuario)
                .WithMany()
                .HasForeignKey(s => s.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        // Configuración de Inventario
        builder.Entity<Categoria>(b =>
        {
            b.ToTable("Categorias");
            b.HasKey(c => c.Id);
            b.Property(c => c.Nombre).HasMaxLength(120).IsRequired();
            b.Property(c => c.Descripcion).HasMaxLength(500);
            b.HasIndex(c => c.Nombre).IsUnique();
        });

        builder.Entity<Producto>(b =>
        {
            b.ToTable("Productos");
            b.HasKey(p => p.Id);
            b.Property(p => p.Sku).HasMaxLength(60).IsRequired();
            b.Property(p => p.Nombre).HasMaxLength(180).IsRequired();
            b.Property(p => p.Descripcion).HasMaxLength(500);
            b.Property(p => p.UnidadMedida).HasMaxLength(40).IsRequired();
            b.Property(p => p.PrecioUnitario).HasPrecision(12, 2);
            b.HasIndex(p => p.Sku).IsUnique();

            b.HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Stock>(b =>
        {
            b.ToTable("Stocks");
            b.HasKey(s => s.Id);
            b.Property(s => s.Ubicacion).HasMaxLength(120);
            b.HasIndex(s => new { s.ProductoId, s.CentroId }).IsUnique();

            b.HasOne(s => s.Producto)
                .WithMany(p => p.Stocks)
                .HasForeignKey(s => s.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(s => s.Centro)
                .WithMany()
                .HasForeignKey(s => s.CentroId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Movimiento>(b =>
        {
            b.ToTable("Movimientos");
            b.HasKey(m => m.Id);
            b.Property(m => m.Tipo).HasMaxLength(30).IsRequired();
            b.Property(m => m.Motivo).HasMaxLength(300);
            b.Property(m => m.Referencia).HasMaxLength(120);
            b.HasIndex(m => new { m.CentroId, m.FechaMovimiento });

            b.HasOne(m => m.Producto)
                .WithMany(p => p.Movimientos)
                .HasForeignKey(m => m.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(m => m.Centro)
                .WithMany()
                .HasForeignKey(m => m.CentroId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<SystemSetting>(b =>
        {
            b.ToTable("SystemSettings");
            b.HasKey(s => s.Key);
            b.Property(s => s.Key).HasMaxLength(120).IsRequired();
            b.Property(s => s.Value).HasColumnType("jsonb").IsRequired();
        });

        // Configuración de Rutas
        builder.Entity<Ruta>(b =>
        {
            b.ToTable("Rutas");
            b.HasKey(r => r.Id);
            b.Property(r => r.Nombre).HasMaxLength(150).IsRequired();
            b.Property(r => r.Origen).HasMaxLength(250).IsRequired();
            b.Property(r => r.Destino).HasMaxLength(250).IsRequired();
            b.Property(r => r.DistanciaKm).HasPrecision(10, 2).IsRequired();
        });

        // Configuración de Pedidos
        builder.Entity<Pedido>(b =>
        {
            b.ToTable("Pedidos");
            b.HasKey(p => p.Id);
            b.Property(p => p.Codigo).HasMaxLength(50).IsRequired();
            b.Property(p => p.Cliente).HasMaxLength(200).IsRequired();
            b.Property(p => p.DireccionEntrega).HasMaxLength(300).IsRequired();
            b.Property(p => p.Estado).HasMaxLength(50).IsRequired();

            b.HasOne(p => p.Centro)
                .WithMany()
                .HasForeignKey(p => p.CentroId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(p => p.Ruta)
                .WithMany(r => r.Pedidos)
                .HasForeignKey(p => p.RutaId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(p => p.Transportista)
                .WithMany()
                .HasForeignKey(p => p.TransportistaId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(p => p.Codigo).IsUnique();
        });

        // Configuración de SLAs
        builder.Entity<SLA>(b =>
        {
            b.ToTable("SLAs");
            b.HasKey(s => s.Id);
            b.Property(s => s.EstadoSLA).HasMaxLength(50).IsRequired();

            b.HasOne(s => s.Pedido)
                .WithOne(p => p.SLA)
                .HasForeignKey<SLA>(s => s.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de EntregaEstados
        builder.Entity<EntregaEstado>(b =>
        {
            b.ToTable("EntregaEstados");
            b.HasKey(ee => ee.Id);
            b.Property(ee => ee.Estado).HasMaxLength(50).IsRequired();
            b.Property(ee => ee.Descripcion).HasMaxLength(500);
            b.Property(ee => ee.Latitud).HasPrecision(9, 6);
            b.Property(ee => ee.Longitud).HasPrecision(9, 6);

            b.HasOne(ee => ee.Pedido)
                .WithMany(p => p.EntregaEstados)
                .HasForeignKey(ee => ee.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ee => ee.ActualizadoPor)
                .WithMany()
                .HasForeignKey(ee => ee.ActualizadoPorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración de Notificaciones
        builder.Entity<Notificacion>(b =>
        {
            b.ToTable("Notificaciones");
            b.HasKey(n => n.Id);
            b.Property(n => n.Tipo).HasMaxLength(30).IsRequired();
            b.Property(n => n.Titulo).HasMaxLength(250).IsRequired();
            b.Property(n => n.Descripcion).HasMaxLength(1000);
            b.HasIndex(n => new { n.CentroId, n.FechaCreacion });
        });

        // Configuración de Reglas de Automatización
        builder.Entity<ReglaAutomatizacion>(b =>
        {
            b.ToTable("ReglasAutomatizacion");
            b.HasKey(r => r.Id);
            b.Property(r => r.Nombre).HasMaxLength(150).IsRequired();
            b.Property(r => r.EventoOrigen).HasMaxLength(50).IsRequired();
            b.Property(r => r.Condicion).HasMaxLength(250).IsRequired();
            b.Property(r => r.Accion).HasMaxLength(50).IsRequired();
            b.Property(r => r.Responsable).HasMaxLength(150).IsRequired();
        });

        // Configuración de Capturas de Cámaras
        builder.Entity<Captura>(b =>
        {
            b.ToTable("Capturas");
            b.HasKey(c => c.Id);
            b.Property(c => c.NombreCamara).HasMaxLength(200).IsRequired();
            b.Property(c => c.Zona).HasMaxLength(100).IsRequired();
            b.Property(c => c.ImagenUrl).HasMaxLength(2000);
            b.Property(c => c.Descripcion).HasMaxLength(500);
            b.HasIndex(c => c.FechaCaptura);
        });
    }
}
