using Microsoft.AspNetCore.Identity;

namespace PEIA.Shared.Infra.Identity;

public class Usuario : IdentityUser<Guid>
{
    public string NombreCompleto { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relación N:M
    public ICollection<UsuarioCentro> UsuarioCentros { get; set; } = new List<UsuarioCentro>();
}
