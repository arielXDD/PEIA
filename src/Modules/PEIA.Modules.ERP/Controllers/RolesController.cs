using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PEIA.Modules.ERP.Handlers;

namespace PEIA.Modules.ERP.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Administrador")]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _mediator.Send(new GetRolesQuery());
        return Ok(roles);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRole(Guid id)
    {
        var role = await _mediator.Send(new GetRoleByIdQuery(id));
        return role is null ? NotFound(new { message = "Rol no encontrado." }) : Ok(role);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] RoleRequest request)
    {
        var result = await _mediator.Send(new CreateRoleCommand(request.Nombre));
        if (!result.Success) return BadRequest(new { errors = result.Errors });
        return CreatedAtAction(nameof(GetRole), new { id = result.Role!.Id }, result.Role);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] RoleRequest request)
    {
        var result = await _mediator.Send(new UpdateRoleCommand(id, request.Nombre));
        if (!result.Success) return BadRequest(new { errors = result.Errors });
        return Ok(result.Role);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var result = await _mediator.Send(new DeleteRoleCommand(id));
        if (!result.Success) return BadRequest(new { errors = result.Errors });
        return NoContent();
    }
}

public record RoleRequest(string Nombre);
public record RoleResponse(Guid Id, string Nombre);
