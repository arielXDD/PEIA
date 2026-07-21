using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PEIA.Web.Controllers;

[ApiController]
[Route("api/camaras")]
[Authorize]
public class CamerasController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public CamerasController(IConfiguration configuration) => _configuration = configuration;

    [HttpGet]
    public IActionResult GetAll()
    {
        var configured = _configuration.GetSection("CameraMonitoring:Cameras").Get<List<CameraConfiguration>>() ?? [];
        return Ok(configured.Count > 0 ? configured.Select((camera, index) => ToDto(camera, index + 1)) : DemoCameras());
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var camera = GetCameras().FirstOrDefault(item => item.Id == id);
        return camera is null ? NotFound(new { message = "Cámara no encontrada." }) : Ok(camera);
    }

    [HttpPost("{id:int}/capturar")]
    public IActionResult Capturar(int id)
    {
        var camera = GetCameras().FirstOrDefault(item => item.Id == id);
        if (camera is null) return NotFound(new { message = "Cámara no encontrada." });
        return Ok(new { message = "Captura solicitada correctamente.", imagenUrl = camera.SnapshotUrl, simulada = camera.Simulated });
    }

    [HttpPost("{id:int}/reportar-incidencia")]
    public IActionResult ReportarIncidencia(int id, [FromBody] ReportarIncidenciaRequest request)
    {
        var camera = GetCameras().FirstOrDefault(item => item.Id == id);
        if (camera is null) return NotFound(new { message = "Cámara no encontrada." });
        return Ok(new { message = "Incidencia registrada.", cameraId = id, descripcion = request.Descripcion, fecha = DateTime.UtcNow });
    }

    private List<CameraDto> GetCameras()
    {
        var configured = _configuration.GetSection("CameraMonitoring:Cameras").Get<List<CameraConfiguration>>() ?? [];
        return configured.Count > 0 ? configured.Select((camera, index) => ToDto(camera, index + 1)).ToList() : DemoCameras();
    }

    private static CameraDto ToDto(CameraConfiguration camera, int id) => new()
    {
        Id = id, Nombre = camera.Nombre, Zona = camera.Zona, ZonaDesc = camera.ZonaDesc ?? string.Empty,
        Host = camera.Host ?? "Configurada", Online = true, Activa = true, UltimaRevision = "Conectada",
        SnapshotUrl = camera.SnapshotUrl, StreamUrl = camera.StreamUrl, StreamType = camera.StreamType ?? "snapshot", Simulated = false,
        EmbedUrl = camera.EmbedUrl
    };

    private static List<CameraDto> DemoCameras() =>
    [
        new() { Id = 1, Nombre = "Zócalo de la Ciudad de México", Zona = "Centro Histórico", ZonaDesc = "Plaza principal de CDMX", Host = "webcamsdemexico.com", Online = true, Activa = true, UltimaRevision = "En vivo", StreamType = "embed", EmbedUrl = "https://www.youtube.com/embed/QM0UE9Vk3pE?autoplay=1", SnapshotUrl = "https://webcamsdemexico.net/mexicodf1/live.jpg?live=1" },
        new() { Id = 2, Nombre = "Monumento a la Revolución", Zona = "Centro", ZonaDesc = "Vista panorámica del monumento", Host = "webcamsdemexico.com", Online = true, Activa = true, UltimaRevision = "En vivo", StreamType = "embed", EmbedUrl = "https://www.youtube.com/embed/1Q74cLFObEk?autoplay=1", SnapshotUrl = "https://webcamsdemexico.net/mexicodf7/live.jpg?live=1" },
        new() { Id = 3, Nombre = "Palacio de Bellas Artes", Zona = "Centro Histórico", ZonaDesc = "Vista del palacio", Host = "webcamsdemexico.com", Online = true, Activa = true, UltimaRevision = "En vivo", StreamType = "embed", EmbedUrl = "https://www.youtube.com/embed/1BQx_vxj7bk?autoplay=1", SnapshotUrl = "https://webcamsdemexico.net/mexicodf13/live.jpg?live=1" },
        new() { Id = 4, Nombre = "Paseo de la Reforma", Zona = "Reforma", ZonaDesc = "Desde Hotel B Urban Xaman", Host = "webcamsdemexico.com", Online = true, Activa = true, UltimaRevision = "En vivo", StreamType = "embed", EmbedUrl = "https://www.youtube.com/embed/0Z3ZLD9JNLs?autoplay=1", SnapshotUrl = "https://webcamsdemexico.net/mexicodf20/live.jpg?live=1" }
    ];
}

public class CameraConfiguration
{
    public string Nombre { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public string? ZonaDesc { get; set; }
    public string? Host { get; set; }
    public string? SnapshotUrl { get; set; }
    public string? StreamUrl { get; set; }
    public string? StreamType { get; set; }
    public string? EmbedUrl { get; set; }
}

public class CameraDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public string ZonaDesc { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public bool Online { get; set; }
    public bool Activa { get; set; }
    public string UltimaRevision { get; set; } = string.Empty;
    public string? SnapshotUrl { get; set; }
    public string? StreamUrl { get; set; }
    public string StreamType { get; set; } = "snapshot";
    public bool Simulated { get; set; }
    public string? EmbedUrl { get; set; }
}

public class ReportarIncidenciaRequest { public string Descripcion { get; set; } = string.Empty; }
