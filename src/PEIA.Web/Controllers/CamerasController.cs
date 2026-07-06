using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PEIA.Web.Controllers;

[ApiController]
[Route("api/camaras")]
public class CamerasController : ControllerBase
{
    private static readonly List<CamaraDto> CamarasMock = new()
    {
        new CamaraDto
        {
            Id = 1, Nombre = "Cámara 1", Zona = "Zona A", ZonaDesc = "Interior bodega",
            Ip = "192.168.1.101", Online = true, Activa = true,
            UltimaRevision = "18/06/2026 14:30"
        },
        new CamaraDto
        {
            Id = 2, Nombre = "Cámara 2", Zona = "Zona B", ZonaDesc = "Montacargas",
            Ip = "192.168.1.102", Online = true, Activa = true,
            UltimaRevision = "18/06/2026 13:15"
        },
        new CamaraDto
        {
            Id = 3, Nombre = "Cámara 3", Zona = "Entrada", ZonaDesc = "Entrada principal",
            Ip = "192.168.1.103", Online = true, Activa = false,
            UltimaRevision = "17/06/2026 09:00"
        },
        new CamaraDto
        {
            Id = 4, Nombre = "Cámara 4", Zona = "Despacho", ZonaDesc = "Zona de despacho",
            Ip = "192.168.1.104", Online = false, Activa = false,
            UltimaRevision = "15/06/2026 11:45"
        }
    };

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(CamarasMock);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var camara = CamarasMock.FirstOrDefault(c => c.Id == id);
        if (camara == null)
            return NotFound(new { message = $"Cámara con ID {id} no encontrada." });

        return Ok(camara);
    }

    [AllowAnonymous]
    [HttpPost("{id:int}/capturar")]
    public IActionResult Capturar(int id)
    {
        var camara = CamarasMock.FirstOrDefault(c => c.Id == id);
        if (camara == null)
            return NotFound(new { message = $"Cámara con ID {id} no encontrada." });

        return Ok(new { message = "Captura realizada con éxito.", imagenUrl = $"/api/camaras/{id}/snapshot" });
    }

    [AllowAnonymous]
    [HttpPost("{id:int}/reportar-incidencia")]
    public IActionResult ReportarIncidencia(int id, [FromBody] ReportarIncidenciaRequest request)
    {
        var camara = CamarasMock.FirstOrDefault(c => c.Id == id);
        if (camara == null)
            return NotFound(new { message = $"Cámara con ID {id} no encontrada." });

        return Ok(new
        {
            message = "Incidencia reportada correctamente.",
            incidencia = new
            {
                camaraId = id,
                camaraNombre = camara.Nombre,
                descripcion = request.Descripcion,
                fecha = DateTime.UtcNow
            }
        });
    }
}

public class CamaraDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public string ZonaDesc { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public bool Online { get; set; }
    public bool Activa { get; set; }
    public string UltimaRevision { get; set; } = string.Empty;
}

public class ReportarIncidenciaRequest
{
    public string Descripcion { get; set; } = string.Empty;
}
