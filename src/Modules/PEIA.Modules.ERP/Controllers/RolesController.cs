using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PEIA.Shared.Infra.Identity;

namespace PEIA.Modules.ERP.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Administrador")]
public class RolesController : ControllerBase
{
    private readonly RoleManager<Rol> _roleManager;

    public RolesController(RoleManager<Rol> roleManager)
    {
        _roleManager = roleManager;
    }

    [HttpGet]
    public IActionResult GetRoles()
    {
        var roles = _roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => new RoleResponse(r.Id, r.Name ?? string.Empty))
            .ToList();

        return Ok(roles);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRole(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        return role is null
            ? NotFound(new { message = "Rol no encontrado." })
            : Ok(new RoleResponse(role.Id, role.Name ?? string.Empty));
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] RoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new { message = "El nombre del rol es obligatorio." });
        }

        var nombre = request.Nombre.Trim();
        if (await _roleManager.RoleExistsAsync(nombre))
        {
            return Conflict(new { message = $"Ya existe el rol '{nombre}'." });
        }

        var role = new Rol { Id = Guid.NewGuid(), Name = nombre };
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        return CreatedAtAction(nameof(GetRole), new { id = role.Id }, new RoleResponse(role.Id, role.Name));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] RoleRequest request)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role is null)
        {
            return NotFound(new { message = "Rol no encontrado." });
        }

        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new { message = "El nombre del rol es obligatorio." });
        }

        role.Name = request.Nombre.Trim();
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new RoleResponse(role.Id, role.Name ?? string.Empty));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role is null)
        {
            return NotFound(new { message = "Rol no encontrado." });
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        return NoContent();
    }
}

public record RoleRequest(string Nombre);
public record RoleResponse(Guid Id, string Nombre);
