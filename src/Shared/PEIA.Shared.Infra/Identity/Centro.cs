namespace PEIA.Shared.Infra.Identity;

public class Centro
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public byte[]? Version { get; set; } // Concurrency token

    public ICollection<UsuarioCentro> UsuarioCentros { get; set; } = new List<UsuarioCentro>();
}
