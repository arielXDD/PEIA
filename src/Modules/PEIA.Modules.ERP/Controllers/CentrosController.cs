using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PEIA.Modules.ERP.Handlers;

namespace PEIA.Modules.ERP.Controllers;

[ApiController]
[Route("api/centros")]
[Authorize]
public class CentrosController : ControllerBase
{
    private readonly IMediator _mediator;

    public CentrosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCentros([FromQuery] bool incluirInactivos = false)
    {
        var centros = await _mediator.Send(new GetCentrosQuery(incluirInactivos));
        return Ok(centros);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCentro(Guid id)
    {
        var centro = await _mediator.Send(new GetCentroByIdQuery(id));
        return centro is null ? NotFound(new { message = "Centro no encontrado." }) : Ok(centro);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CreateCentro([FromBody] CentroRequest request)
    {
        var result = await _mediator.Send(new CreateCentroCommand(request));
        if (!result.Success) return BadRequest(new { message = result.Error });
        return CreatedAtAction(nameof(GetCentro), new { id = result.Centro!.Id }, result.Centro);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateCentro(Guid id, [FromBody] CentroRequest request)
    {
        var result = await _mediator.Send(new UpdateCentroCommand(id, request));
        if (!result.Success)
        {
            if (result.Error == "Centro no encontrado.") return NotFound(new { message = result.Error });
            if (result.Error!.StartsWith("Ya existe")) return Conflict(new { message = result.Error });
            return BadRequest(new { message = result.Error });
        }
        return Ok(result.Centro);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteCentro(Guid id)
    {
        var result = await _mediator.Send(new DeleteCentroCommand(id));
        if (!result.Success) return NotFound(new { message = result.Error });
        return NoContent();
    }
}

public record CentroRequest(string Nombre, string Codigo, string? Direccion, bool Activo = true);
public record CentroResponse(Guid Id, string Nombre, string Codigo, string Direccion, bool Activo);
