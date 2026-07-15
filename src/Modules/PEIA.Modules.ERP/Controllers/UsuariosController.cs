using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PEIA.Modules.ERP.Handlers;
using System.Security.Claims;

namespace PEIA.Modules.ERP.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsuariosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetUsuarios()
    {
        var response = await _mediator.Send(new GetUsuariosQuery());
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetUsuario(Guid id)
    {
        var usuario = await _mediator.Send(new GetUsuarioByIdQuery(id));
        return usuario is null ? NotFound(new { message = "Usuario no encontrado." }) : Ok(usuario);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CreateUsuario([FromBody] CreateUsuarioRequest request)
    {
        var result = await _mediator.Send(new CreateUsuarioCommand(request));
        if (!result.Success) return BadRequest(new { errors = result.Errors });
        return CreatedAtAction(nameof(GetUsuario), new { id = result.Usuario!.Id }, result.Usuario);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateUsuario(Guid id, [FromBody] UpdateUsuarioRequest request)
    {
        var result = await _mediator.Send(new UpdateUsuarioCommand(id, request));
        if (!result.Success) return BadRequest(new { errors = result.Errors });
        return Ok(result.Usuario);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteUsuario(Guid id)
    {
        var result = await _mediator.Send(new DeleteUsuarioCommand(id));
        if (!result.Success) return NotFound(new { message = result.Errors?.FirstOrDefault() ?? "Error" });
        return NoContent();
    }

    [HttpPut("{id:guid}/centros")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> AsignarCentros(Guid id, [FromBody] AsignarCentrosRequest request)
    {
        var result = await _mediator.Send(new AsignarCentrosCommand(id, request));
        if (!result.Success) return BadRequest(new { message = result.Errors?.FirstOrDefault() ?? "Error" });
        return Ok(new { message = "Centros asignados correctamente." });
    }

    [HttpPut("centro-activo")]
    public async Task<IActionResult> CambiarCentroActivo([FromBody] CambiarCentroActivoRequest request)
    {
        var usuarioId = GetCurrentUserId();
        if (usuarioId is null)
            return Unauthorized(new { message = "No se pudo identificar al usuario actual." });

        var result = await _mediator.Send(new CambiarCentroActivoCommand(usuarioId.Value, request));
        if (!result.Success) return Forbid();

        return Ok(new { message = "Centro activo cambiado correctamente.", centroActivo = result.CentroActivo });
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(value, out var id) ? id : null;
    }
}

public record CreateUsuarioRequest(
    string UserName,
    string Email,
    string NombreCompleto,
    string Password,
    bool Activo,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<Guid> CentroIds);

public record UpdateUsuarioRequest(
    string UserName,
    string Email,
    string NombreCompleto,
    string? Password,
    bool Activo,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<Guid> CentroIds);

public record AsignarCentrosRequest(IReadOnlyCollection<Guid> CentroIds);
public record CambiarCentroActivoRequest(Guid CentroId);

public record UsuarioResponse(
    Guid Id,
    string UserName,
    string Email,
    string NombreCompleto,
    bool Activo,
    DateTime FechaCreacion,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<CentroResponse> Centros);
