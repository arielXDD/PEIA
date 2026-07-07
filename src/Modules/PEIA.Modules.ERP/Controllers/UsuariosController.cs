using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Identity;

namespace PEIA.Modules.ERP.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private const int MaxCentrosPorUsuario = 2;
    private readonly UserManager<Usuario> _userManager;
    private readonly PeiaDbContext _context;

    public UsuariosController(UserManager<Usuario> userManager, PeiaDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetUsuarios()
    {
        var usuarios = await _context.Users
            .Include(u => u.UsuarioCentros)
            .ThenInclude(uc => uc.Centro)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync();

        var response = new List<UsuarioResponse>();
        foreach (var usuario in usuarios)
        {
            response.Add(await MapUsuarioAsync(usuario));
        }

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetUsuario(Guid id)
    {
        var usuario = await _context.Users
            .Include(u => u.UsuarioCentros)
            .ThenInclude(uc => uc.Centro)
            .FirstOrDefaultAsync(u => u.Id == id);

        return usuario is null ? NotFound(new { message = "Usuario no encontrado." }) : Ok(await MapUsuarioAsync(usuario));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CreateUsuario([FromBody] CreateUsuarioRequest request)
    {
        var validation = await ValidateUsuarioRequestAsync(request.Email, request.UserName, request.NombreCompleto, request.CentroIds);
        if (validation is not null)
        {
            return validation;
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "La contraseña es obligatoria." });
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            NombreCompleto = request.NombreCompleto.Trim(),
            EmailConfirmed = true,
            Activo = request.Activo,
            FechaCreacion = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(usuario, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        var roleResult = await SetRolesAsync(usuario, request.Roles);
        if (roleResult is not null)
        {
            return roleResult;
        }

        await SetCentrosAsync(usuario.Id, request.CentroIds);

        var created = await _context.Users
            .Include(u => u.UsuarioCentros)
            .ThenInclude(uc => uc.Centro)
            .FirstAsync(u => u.Id == usuario.Id);

        return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, await MapUsuarioAsync(created));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateUsuario(Guid id, [FromBody] UpdateUsuarioRequest request)
    {
        var usuario = await _context.Users
            .Include(u => u.UsuarioCentros)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario is null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        var validation = await ValidateUsuarioRequestAsync(request.Email, request.UserName, request.NombreCompleto, request.CentroIds, id);
        if (validation is not null)
        {
            return validation;
        }

        usuario.UserName = request.UserName.Trim();
        usuario.Email = request.Email.Trim();
        usuario.NombreCompleto = request.NombreCompleto.Trim();
        usuario.Activo = request.Activo;

        var updateResult = await _userManager.UpdateAsync(usuario);
        if (!updateResult.Succeeded)
        {
            return BadRequest(new { errors = updateResult.Errors.Select(e => e.Description) });
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var passwordResult = await _userManager.ResetPasswordAsync(usuario, token, request.Password);
            if (!passwordResult.Succeeded)
            {
                return BadRequest(new { errors = passwordResult.Errors.Select(e => e.Description) });
            }
        }

        var roleResult = await SetRolesAsync(usuario, request.Roles);
        if (roleResult is not null)
        {
            return roleResult;
        }

        await SetCentrosAsync(usuario.Id, request.CentroIds);

        var updated = await _context.Users
            .Include(u => u.UsuarioCentros)
            .ThenInclude(uc => uc.Centro)
            .FirstAsync(u => u.Id == id);

        return Ok(await MapUsuarioAsync(updated));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteUsuario(Guid id)
    {
        var usuario = await _userManager.FindByIdAsync(id.ToString());
        if (usuario is null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        usuario.Activo = false;
        await _userManager.UpdateAsync(usuario);

        return NoContent();
    }

    [HttpPut("{id:guid}/centros")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> AsignarCentros(Guid id, [FromBody] AsignarCentrosRequest request)
    {
        if (await _userManager.FindByIdAsync(id.ToString()) is null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        if (request.CentroIds.Count > MaxCentrosPorUsuario)
        {
            return BadRequest(new { message = $"Un usuario puede estar asignado a máximo {MaxCentrosPorUsuario} centros." });
        }

        if (request.CentroIds.Count != request.CentroIds.Distinct().Count())
        {
            return BadRequest(new { message = "No repitas centros en la asignación." });
        }

        var centrosExistentes = await _context.Centros.CountAsync(c => request.CentroIds.Contains(c.Id) && c.Activo);
        if (centrosExistentes != request.CentroIds.Count)
        {
            return BadRequest(new { message = "Uno o más centros no existen o están inactivos." });
        }

        await SetCentrosAsync(id, request.CentroIds);
        return Ok(new { message = "Centros asignados correctamente." });
    }

    [HttpPut("centro-activo")]
    public async Task<IActionResult> CambiarCentroActivo([FromBody] CambiarCentroActivoRequest request)
    {
        var usuarioId = GetCurrentUserId();
        if (usuarioId is null)
        {
            return Unauthorized(new { message = "No se pudo identificar al usuario actual." });
        }

        var asignado = await _context.UsuarioCentros
            .AnyAsync(uc => uc.UsuarioId == usuarioId.Value && uc.CentroId == request.CentroId && uc.Activo && uc.Centro.Activo);

        if (!asignado)
        {
            return Forbid();
        }

        var centro = await _context.Centros
            .Where(c => c.Id == request.CentroId)
            .Select(c => new CentroResponse(c.Id, c.Nombre, c.Codigo, c.Direccion, c.Activo))
            .FirstAsync();

        return Ok(new { message = "Centro activo cambiado correctamente.", centroActivo = centro });
    }

    private async Task<IActionResult?> ValidateUsuarioRequestAsync(
        string email,
        string userName,
        string nombreCompleto,
        IReadOnlyCollection<Guid> centroIds,
        Guid? usuarioId = null)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(nombreCompleto))
        {
            return BadRequest(new { message = "Nombre completo, usuario y email son obligatorios." });
        }

        if (centroIds.Count > MaxCentrosPorUsuario)
        {
            return BadRequest(new { message = $"Un usuario puede estar asignado a máximo {MaxCentrosPorUsuario} centros." });
        }

        if (centroIds.Count != centroIds.Distinct().Count())
        {
            return BadRequest(new { message = "No repitas centros en la asignación." });
        }

        if (await _context.Users.AnyAsync(u => u.Id != usuarioId && u.Email == email.Trim()))
        {
            return Conflict(new { message = "Ya existe un usuario con ese email." });
        }

        if (await _context.Users.AnyAsync(u => u.Id != usuarioId && u.UserName == userName.Trim()))
        {
            return Conflict(new { message = "Ya existe un usuario con ese nombre de usuario." });
        }

        var centrosExistentes = await _context.Centros.CountAsync(c => centroIds.Contains(c.Id) && c.Activo);
        if (centrosExistentes != centroIds.Count)
        {
            return BadRequest(new { message = "Uno o más centros no existen o están inactivos." });
        }

        return null;
    }

    private async Task<IActionResult?> SetRolesAsync(Usuario usuario, IReadOnlyCollection<string> roles)
    {
        var requestedRoles = roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingRoles = await _context.Roles.Select(r => r.Name!).ToListAsync();
        var missingRoles = requestedRoles.Except(existingRoles, StringComparer.OrdinalIgnoreCase).ToList();
        if (missingRoles.Count > 0)
        {
            return BadRequest(new { message = "Uno o más roles no existen.", roles = missingRoles });
        }

        var currentRoles = await _userManager.GetRolesAsync(usuario);
        var removeResult = await _userManager.RemoveFromRolesAsync(usuario, currentRoles);
        if (!removeResult.Succeeded)
        {
            return BadRequest(new { errors = removeResult.Errors.Select(e => e.Description) });
        }

        if (requestedRoles.Count == 0)
        {
            return null;
        }

        var addResult = await _userManager.AddToRolesAsync(usuario, requestedRoles);
        return addResult.Succeeded ? null : BadRequest(new { errors = addResult.Errors.Select(e => e.Description) });
    }

    private async Task SetCentrosAsync(Guid usuarioId, IReadOnlyCollection<Guid> centroIds)
    {
        var actuales = await _context.UsuarioCentros
            .Where(uc => uc.UsuarioId == usuarioId)
            .ToListAsync();

        _context.UsuarioCentros.RemoveRange(actuales);
        foreach (var centroId in centroIds.Distinct().Take(MaxCentrosPorUsuario))
        {
            _context.UsuarioCentros.Add(new UsuarioCentro
            {
                UsuarioId = usuarioId,
                CentroId = centroId,
                Activo = true
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task<UsuarioResponse> MapUsuarioAsync(Usuario usuario)
    {
        var roles = await _userManager.GetRolesAsync(usuario);
        var centros = usuario.UsuarioCentros
            .Where(uc => uc.Activo && uc.Centro is not null)
            .OrderBy(uc => uc.Centro.Codigo)
            .Select(uc => new CentroResponse(uc.CentroId, uc.Centro.Nombre, uc.Centro.Codigo, uc.Centro.Direccion, uc.Centro.Activo))
            .ToList();

        return new UsuarioResponse(
            usuario.Id,
            usuario.UserName ?? string.Empty,
            usuario.Email ?? string.Empty,
            usuario.NombreCompleto,
            usuario.Activo,
            usuario.FechaCreacion,
            roles.ToList(),
            centros);
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
