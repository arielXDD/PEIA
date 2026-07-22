namespace PEIA.Shared.Infra.Identity;

public class UserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string JwtId { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
    public DateTime FechaExpiracion { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Revocada { get; set; }
    public DateTime? FechaRevocacion { get; set; }
}
