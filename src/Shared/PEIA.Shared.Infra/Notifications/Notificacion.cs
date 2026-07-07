namespace PEIA.Shared.Infra.Notifications;

public class Notificacion
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public Guid CentroId { get; set; }
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
