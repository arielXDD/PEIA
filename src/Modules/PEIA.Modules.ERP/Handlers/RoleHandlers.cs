using MediatR;
using Microsoft.AspNetCore.Identity;
using PEIA.Modules.ERP.Controllers;
using PEIA.Shared.Infra.Identity;
using Microsoft.Extensions.Caching.Memory;
using PEIA.Modules.ERP.Controllers;
using PEIA.Shared.Infra.Identity;

namespace PEIA.Modules.ERP.Handlers;

// ── QUERIES ──

public record GetRolesQuery() : IRequest<List<RoleResponse>>;
public record GetRoleByIdQuery(Guid Id) : IRequest<RoleResponse?>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleResponse>>
{
    private readonly RoleManager<Rol> _roleManager;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public GetRolesQueryHandler(RoleManager<Rol> roleManager, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _roleManager = roleManager;
        _cache = cache;
    }

    public Task<List<RoleResponse>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = "RolesCache";
        if (!_cache.TryGetValue(cacheKey, out List<RoleResponse>? roles))
        {
            roles = _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new RoleResponse(r.Id, r.Name ?? string.Empty))
                .ToList();

            _cache.Set(cacheKey, roles, TimeSpan.FromMinutes(5));
        }

        return Task.FromResult(roles!);
    }
}

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleResponse?>
{
    private readonly RoleManager<Rol> _roleManager;
    public GetRoleByIdQueryHandler(RoleManager<Rol> roleManager) => _roleManager = roleManager;

    public async Task<RoleResponse?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id.ToString());
        return role is null ? null : new RoleResponse(role.Id, role.Name ?? string.Empty);
    }
}

// ── COMMANDS ──

public record CreateRoleCommand(string Nombre) : IRequest<RoleResult>;
public record UpdateRoleCommand(Guid Id, string Nombre) : IRequest<RoleResult>;
public record DeleteRoleCommand(Guid Id) : IRequest<RoleResult>;

public record RoleResult(bool Success, RoleResponse? Role = null, IEnumerable<string>? Errors = null);

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleResult>
{
    private readonly RoleManager<Rol> _roleManager;
    public CreateRoleCommandHandler(RoleManager<Rol> roleManager) => _roleManager = roleManager;

    public async Task<RoleResult> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return new RoleResult(false, null, new[] { "El nombre del rol es obligatorio." });

        var nombre = request.Nombre.Trim();
        if (await _roleManager.RoleExistsAsync(nombre))
            return new RoleResult(false, null, new[] { $"Ya existe el rol '{nombre}'." });

        var role = new Rol { Id = Guid.NewGuid(), Name = nombre };
        var result = await _roleManager.CreateAsync(role);
        
        if (!result.Succeeded)
            return new RoleResult(false, null, result.Errors.Select(e => e.Description));

        return new RoleResult(true, new RoleResponse(role.Id, role.Name!));
    }
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleResult>
{
    private readonly RoleManager<Rol> _roleManager;
    public UpdateRoleCommandHandler(RoleManager<Rol> roleManager) => _roleManager = roleManager;

    public async Task<RoleResult> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id.ToString());
        if (role is null)
            return new RoleResult(false, null, new[] { "Rol no encontrado." });

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return new RoleResult(false, null, new[] { "El nombre del rol es obligatorio." });

        role.Name = request.Nombre.Trim();
        var result = await _roleManager.UpdateAsync(role);
        
        if (!result.Succeeded)
            return new RoleResult(false, null, result.Errors.Select(e => e.Description));

        return new RoleResult(true, new RoleResponse(role.Id, role.Name!));
    }
}

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, RoleResult>
{
    private readonly RoleManager<Rol> _roleManager;
    public DeleteRoleCommandHandler(RoleManager<Rol> roleManager) => _roleManager = roleManager;

    public async Task<RoleResult> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id.ToString());
        if (role is null)
            return new RoleResult(false, null, new[] { "Rol no encontrado." });

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return new RoleResult(false, null, result.Errors.Select(e => e.Description));

        return new RoleResult(true);
    }
}
