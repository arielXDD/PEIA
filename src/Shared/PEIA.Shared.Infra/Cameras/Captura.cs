namespace PEIA.Shared.Infra.Cameras;

public class Captura
{
    public Guid Id { get; set; }
    public int CameraId { get; set; }
    public string NombreCamara { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public string? Descripcion { get; set; }
    public Guid? UsuarioId { get; set; }
    public DateTime FechaCaptura { get; set; } = DateTime.UtcNow;
}
