using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PEIA.Shared.Infra.Identity;

namespace PEIA.Shared.Infra.Data;

public class PeiaDbContext : IdentityDbContext<Usuario, Rol, Guid>
{
    public PeiaDbContext(DbContextOptions<PeiaDbContext> options) : base(options)
    {
    }

    public DbSet<Centro> Centros => Set<Centro>();
    public DbSet<UsuarioCentro> UsuarioCentros => Set<UsuarioCentro>();

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
    }
}
