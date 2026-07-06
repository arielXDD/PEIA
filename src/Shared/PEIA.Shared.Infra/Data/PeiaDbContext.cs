using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PEIA.Shared.Infra.Identity;
using PEIA.Shared.Infra.Logistics;

namespace PEIA.Shared.Infra.Data;

public class PeiaDbContext : IdentityDbContext<Usuario, Rol, Guid>
{
    public PeiaDbContext(DbContextOptions<PeiaDbContext> options) : base(options)
    {
    }

    public DbSet<Centro> Centros => Set<Centro>();
    public DbSet<UsuarioCentro> UsuarioCentros => Set<UsuarioCentro>();
    // DbSets de Logística
    public DbSet<Ruta> Rutas => Set<Ruta>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<SLA> SLAs => Set<SLA>();
    public DbSet<EntregaEstado> EntregaEstados => Set<EntregaEstado>();

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
    }
}
