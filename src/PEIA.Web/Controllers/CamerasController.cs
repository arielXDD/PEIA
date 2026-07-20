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
        SnapshotUrl = camera.SnapshotUrl, StreamUrl = camera.StreamUrl, StreamType = camera.StreamType ?? "snapshot", Simulated = false
    };

    private static List<CameraDto> DemoCameras() =>
    [
        new() { Id = 1, Nombre = "Acceso principal", Zona = "Entrada", ZonaDesc = "Acceso a instalaciones", Host = "Simulador local", Online = true, Activa = true, UltimaRevision = "En vivo", StreamType = "simulated", Simulated = true },
        new() { Id = 2, Nombre = "Almacén A", Zona = "Bodega", ZonaDesc = "Pasillo central", Host = "Simulador local", Online = true, Activa = true, UltimaRevision = "En vivo", StreamType = "simulated", Simulated = true },
        new() { Id = 3, Nombre = "Andén de carga", Zona = "Despacho", ZonaDesc = "Área de embarques", Host = "Simulador local", Online = true, Activa = true, UltimaRevision = "En vivo", StreamType = "simulated", Simulated = true },
        new() { Id = 4, Nombre = "Perímetro norte", Zona = "Exterior", ZonaDesc = "Patio de maniobras", Host = "Simulador local", Online = true, Activa = true, UltimaRevision = "En vivo", StreamType = "simulated", Simulated = true }
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
}

public class ReportarIncidenciaRequest { public string Descripcion { get; set; } = string.Empty; }
