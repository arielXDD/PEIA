namespace PEIA.Shared.Infra.Identity;

public class UsuarioCentro
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public Guid CentroId { get; set; }
    public Centro Centro { get; set; } = null!;

    public bool Activo { get; set; } = true;
}
