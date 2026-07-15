using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PEIA.Modules.ERP.Controllers;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Identity;

namespace PEIA.Modules.ERP.Handlers;

// ── QUERIES ──

public record GetUsuariosQuery() : IRequest<List<UsuarioResponse>>;
public record GetUsuarioByIdQuery(Guid Id) : IRequest<UsuarioResponse?>;

public class GetUsuariosQueryHandler : IRequestHandler<GetUsuariosQuery, List<UsuarioResponse>>
{
    private readonly PeiaDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public GetUsuariosQueryHandler(PeiaDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<UsuarioResponse>> Handle(GetUsuariosQuery request, CancellationToken cancellationToken)
    {
        var usuarios = await _context.Users
            .Include(u => u.UsuarioCentros)
            .ThenInclude(uc => uc.Centro)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync(cancellationToken);

        var response = new List<UsuarioResponse>();
        foreach (var usuario in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            response.Add(MapUsuario(usuario, roles));
        }

        return response;
    }

    private static UsuarioResponse MapUsuario(Usuario usuario, IList<string> roles)
    {
        var centros = usuario.UsuarioCentros
            .Where(uc => uc.Activo && uc.Centro is not null)
            .OrderBy(uc => uc.Centro!.Codigo)
            .Select(uc => new CentroResponse(uc.CentroId, uc.Centro!.Nombre, uc.Centro.Codigo, uc.Centro.Direccion, uc.Centro.Activo))
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
}

public class GetUsuarioByIdQueryHandler : IRequestHandler<GetUsuarioByIdQuery, UsuarioResponse?>
{
    private readonly PeiaDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public GetUsuarioByIdQueryHandler(PeiaDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<UsuarioResponse?> Handle(GetUsuarioByIdQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _context.Users
            .Include(u => u.UsuarioCentros)
            .ThenInclude(uc => uc.Centro)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (usuario is null) return null;

        var roles = await _userManager.GetRolesAsync(usuario);
        return MapUsuario(usuario, roles);
    }

    private static UsuarioResponse MapUsuario(Usuario usuario, IList<string> roles)
    {
        var centros = usuario.UsuarioCentros
            .Where(uc => uc.Activo && uc.Centro is not null)
            .OrderBy(uc => uc.Centro!.Codigo)
            .Select(uc => new CentroResponse(uc.CentroId, uc.Centro!.Nombre, uc.Centro.Codigo, uc.Centro.Direccion, uc.Centro.Activo))
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
}

// ── COMMANDS ──

public record CreateUsuarioCommand(CreateUsuarioRequest Request) : IRequest<UsuarioResult>;
public record UpdateUsuarioCommand(Guid Id, UpdateUsuarioRequest Request) : IRequest<UsuarioResult>;
public record DeleteUsuarioCommand(Guid Id) : IRequest<UsuarioResult>;
public record AsignarCentrosCommand(Guid Id, AsignarCentrosRequest Request) : IRequest<UsuarioResult>;
public record CambiarCentroActivoCommand(Guid UsuarioId, CambiarCentroActivoRequest Request) : IRequest<UsuarioResult>;

public record UsuarioResult(bool Success, UsuarioResponse? Usuario = null, IEnumerable<string>? Errors = null, CentroResponse? CentroActivo = null);

public class CreateUsuarioCommandHandler : IRequestHandler<CreateUsuarioCommand, UsuarioResult>
{
    private const int MaxCentrosPorUsuario = 2;
    private readonly UserManager<Usuario> _userManager;
    private readonly PeiaDbContext _context;

    public CreateUsuarioCommandHandler(UserManager<Usuario> userManager, PeiaDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<UsuarioResult> Handle(CreateUsuarioCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        // Validations
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.NombreCompleto))
            return new UsuarioResult(false, null, new[] { "Nombre completo, usuario y email son obligatorios." });

        if (request.CentroIds.Count > MaxCentrosPorUsuario)
            return new UsuarioResult(false, null, new[] { $"Un usuario puede estar asignado a máximo {MaxCentrosPorUsuario} centros." });

        if (request.CentroIds.Count != request.CentroIds.Distinct().Count())
            return new UsuarioResult(false, null, new[] { "No repitas centros en la asignación." });

        if (await _context.Users.AnyAsync(u => u.Email == request.Email.Trim(), cancellationToken))
            return new UsuarioResult(false, null, new[] { "Ya existe un usuario con ese email." });

        if (await _context.Users.AnyAsync(u => u.UserName == request.UserName.Trim(), cancellationToken))
            return new UsuarioResult(false, null, new[] { "Ya existe un usuario con ese nombre de usuario." });

        var centrosExistentes = await _context.Centros.CountAsync(c => request.CentroIds.Contains(c.Id) && c.Activo, cancellationToken);
        if (centrosExistentes != request.CentroIds.Count)
            return new UsuarioResult(false, null, new[] { "Uno o más centros no existen o están inactivos." });

        if (string.IsNullOrWhiteSpace(request.Password))
            return new UsuarioResult(false, null, new[] { "La contraseña es obligatoria." });

        // Logic
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
            return new UsuarioResult(false, null, result.Errors.Select(e => e.Description));

        // Roles
        var requestedRoles = request.Roles.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (requestedRoles.Count > 0)
        {
            var existingRoles = await _context.Roles.Select(r => r.Name!).ToListAsync(cancellationToken);
            var missingRoles = requestedRoles.Except(existingRoles, StringComparer.OrdinalIgnoreCase).ToList();
            if (missingRoles.Count > 0)
                return new UsuarioResult(false, null, new[] { "Uno o más roles no existen." });

            var addResult = await _userManager.AddToRolesAsync(usuario, requestedRoles);
            if (!addResult.Succeeded)
                return new UsuarioResult(false, null, addResult.Errors.Select(e => e.Description));
        }

        // Centros
        foreach (var centroId in request.CentroIds.Distinct().Take(MaxCentrosPorUsuario))
        {
            _context.UsuarioCentros.Add(new UsuarioCentro { UsuarioId = usuario.Id, CentroId = centroId, Activo = true });
        }
        await _context.SaveChangesAsync(cancellationToken);

        // Map response
        var created = await _context.Users.Include(u => u.UsuarioCentros).ThenInclude(uc => uc.Centro).FirstAsync(u => u.Id == usuario.Id, cancellationToken);
        var roles = await _userManager.GetRolesAsync(created);
        
        var centrosResponse = created.UsuarioCentros.Where(uc => uc.Activo && uc.Centro is not null).Select(uc => new CentroResponse(uc.CentroId, uc.Centro!.Nombre, uc.Centro!.Codigo, uc.Centro!.Direccion, uc.Centro!.Activo)).ToList();
        
        return new UsuarioResult(true, new UsuarioResponse(created.Id, created.UserName!, created.Email!, created.NombreCompleto, created.Activo, created.FechaCreacion, roles.ToList(), centrosResponse));
    }
}

public class UpdateUsuarioCommandHandler : IRequestHandler<UpdateUsuarioCommand, UsuarioResult>
{
    private const int MaxCentrosPorUsuario = 2;
    private readonly UserManager<Usuario> _userManager;
    private readonly PeiaDbContext _context;

    public UpdateUsuarioCommandHandler(UserManager<Usuario> userManager, PeiaDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<UsuarioResult> Handle(UpdateUsuarioCommand command, CancellationToken cancellationToken)
    {
        var id = command.Id;
        var request = command.Request;
        var usuario = await _context.Users.Include(u => u.UsuarioCentros).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (usuario is null) return new UsuarioResult(false, null, new[] { "Usuario no encontrado." });

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.NombreCompleto))
            return new UsuarioResult(false, null, new[] { "Nombre completo, usuario y email son obligatorios." });

        if (request.CentroIds.Count > MaxCentrosPorUsuario)
            return new UsuarioResult(false, null, new[] { $"Un usuario puede estar asignado a máximo {MaxCentrosPorUsuario} centros." });

        if (await _context.Users.AnyAsync(u => u.Id != id && u.Email == request.Email.Trim(), cancellationToken))
            return new UsuarioResult(false, null, new[] { "Ya existe un usuario con ese email." });

        if (await _context.Users.AnyAsync(u => u.Id != id && u.UserName == request.UserName.Trim(), cancellationToken))
            return new UsuarioResult(false, null, new[] { "Ya existe un usuario con ese nombre de usuario." });

        usuario.UserName = request.UserName.Trim();
        usuario.Email = request.Email.Trim();
        usuario.NombreCompleto = request.NombreCompleto.Trim();
        usuario.Activo = request.Activo;

        var updateResult = await _userManager.UpdateAsync(usuario);
        if (!updateResult.Succeeded) return new UsuarioResult(false, null, updateResult.Errors.Select(e => e.Description));

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var passwordResult = await _userManager.ResetPasswordAsync(usuario, token, request.Password);
            if (!passwordResult.Succeeded) return new UsuarioResult(false, null, passwordResult.Errors.Select(e => e.Description));
        }

        // Roles
        var requestedRoles = request.Roles.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var currentRoles = await _userManager.GetRolesAsync(usuario);
        await _userManager.RemoveFromRolesAsync(usuario, currentRoles);
        
        if (requestedRoles.Count > 0)
        {
            await _userManager.AddToRolesAsync(usuario, requestedRoles);
        }

        // Centros
        _context.UsuarioCentros.RemoveRange(usuario.UsuarioCentros);
        foreach (var centroId in request.CentroIds.Distinct().Take(MaxCentrosPorUsuario))
        {
            _context.UsuarioCentros.Add(new UsuarioCentro { UsuarioId = usuario.Id, CentroId = centroId, Activo = true });
        }
        await _context.SaveChangesAsync(cancellationToken);

        var updated = await _context.Users.Include(u => u.UsuarioCentros).ThenInclude(uc => uc.Centro).FirstAsync(u => u.Id == id, cancellationToken);
        var roles = await _userManager.GetRolesAsync(updated);
        var centrosResponse = updated.UsuarioCentros.Where(uc => uc.Activo && uc.Centro is not null).Select(uc => new CentroResponse(uc.CentroId, uc.Centro!.Nombre, uc.Centro!.Codigo, uc.Centro!.Direccion, uc.Centro!.Activo)).ToList();

        return new UsuarioResult(true, new UsuarioResponse(updated.Id, updated.UserName!, updated.Email!, updated.NombreCompleto, updated.Activo, updated.FechaCreacion, roles.ToList(), centrosResponse));
    }
}

public class DeleteUsuarioCommandHandler : IRequestHandler<DeleteUsuarioCommand, UsuarioResult>
{
    private readonly UserManager<Usuario> _userManager;
    public DeleteUsuarioCommandHandler(UserManager<Usuario> userManager) => _userManager = userManager;

    public async Task<UsuarioResult> Handle(DeleteUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _userManager.FindByIdAsync(request.Id.ToString());
        if (usuario is null) return new UsuarioResult(false, null, new[] { "Usuario no encontrado." });

        usuario.Activo = false;
        await _userManager.UpdateAsync(usuario);
        return new UsuarioResult(true);
    }
}

public class AsignarCentrosCommandHandler : IRequestHandler<AsignarCentrosCommand, UsuarioResult>
{
    private const int MaxCentrosPorUsuario = 2;
    private readonly UserManager<Usuario> _userManager;
    private readonly PeiaDbContext _context;

    public AsignarCentrosCommandHandler(UserManager<Usuario> userManager, PeiaDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<UsuarioResult> Handle(AsignarCentrosCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        if (await _userManager.FindByIdAsync(command.Id.ToString()) is null)
            return new UsuarioResult(false, null, new[] { "Usuario no encontrado." });

        if (request.CentroIds.Count > MaxCentrosPorUsuario)
            return new UsuarioResult(false, null, new[] { $"Un usuario puede estar asignado a máximo {MaxCentrosPorUsuario} centros." });

        var actuales = await _context.UsuarioCentros.Where(uc => uc.UsuarioId == command.Id).ToListAsync(cancellationToken);
        _context.UsuarioCentros.RemoveRange(actuales);
        
        foreach (var centroId in request.CentroIds.Distinct().Take(MaxCentrosPorUsuario))
        {
            _context.UsuarioCentros.Add(new UsuarioCentro { UsuarioId = command.Id, CentroId = centroId, Activo = true });
        }
        await _context.SaveChangesAsync(cancellationToken);
        
        return new UsuarioResult(true);
    }
}

public class CambiarCentroActivoCommandHandler : IRequestHandler<CambiarCentroActivoCommand, UsuarioResult>
{
    private readonly PeiaDbContext _context;
    public CambiarCentroActivoCommandHandler(PeiaDbContext context) => _context = context;

    public async Task<UsuarioResult> Handle(CambiarCentroActivoCommand command, CancellationToken cancellationToken)
    {
        var asignado = await _context.UsuarioCentros
            .AnyAsync(uc => uc.UsuarioId == command.UsuarioId && uc.CentroId == command.Request.CentroId && uc.Activo && uc.Centro.Activo, cancellationToken);

        if (!asignado)
            return new UsuarioResult(false, null, new[] { "No autorizado o no asignado a este centro." });

        var centro = await _context.Centros
            .Where(c => c.Id == command.Request.CentroId)
            .Select(c => new CentroResponse(c.Id, c.Nombre, c.Codigo, c.Direccion, c.Activo))
            .FirstAsync(cancellationToken);

        return new UsuarioResult(true, null, null, centro);
    }
}
