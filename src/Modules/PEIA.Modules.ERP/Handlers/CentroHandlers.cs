using MediatR;
using Microsoft.EntityFrameworkCore;
using PEIA.Modules.ERP.Controllers;
using PEIA.Shared.Infra.Data;
using Microsoft.Extensions.Caching.Memory;
using PEIA.Modules.ERP.Controllers;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Identity;

namespace PEIA.Modules.ERP.Handlers;

// ── QUERIES ──

public record GetCentrosQuery(bool IncluirInactivos = false) : IRequest<List<CentroResponse>>;
public record GetCentroByIdQuery(Guid Id) : IRequest<CentroResponse?>;

public class GetCentrosQueryHandler : IRequestHandler<GetCentrosQuery, List<CentroResponse>>
{
    private readonly PeiaDbContext _context;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public GetCentrosQueryHandler(PeiaDbContext context, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<CentroResponse>> Handle(GetCentrosQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"CentrosCache_{request.IncluirInactivos}";
        if (!_cache.TryGetValue(cacheKey, out List<CentroResponse>? centros))
        {
            centros = await _context.Centros
                .Where(c => request.IncluirInactivos || c.Activo)
                .OrderBy(c => c.Codigo)
                .Select(c => new CentroResponse(c.Id, c.Nombre, c.Codigo, c.Direccion, c.Activo))
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, centros, TimeSpan.FromMinutes(5));
        }

        return centros!;
    }
}

public class GetCentroByIdQueryHandler : IRequestHandler<GetCentroByIdQuery, CentroResponse?>
{
    private readonly PeiaDbContext _context;
    public GetCentroByIdQueryHandler(PeiaDbContext context) => _context = context;

    public async Task<CentroResponse?> Handle(GetCentroByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Centros
            .Where(c => c.Id == request.Id)
            .Select(c => new CentroResponse(c.Id, c.Nombre, c.Codigo, c.Direccion, c.Activo))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

// ── COMMANDS ──

public record CreateCentroCommand(CentroRequest Request) : IRequest<CentroResult>;
public record UpdateCentroCommand(Guid Id, CentroRequest Request) : IRequest<CentroResult>;
public record DeleteCentroCommand(Guid Id) : IRequest<CentroResult>;

public record CentroResult(bool Success, CentroResponse? Centro = null, string? Error = null);

public class CreateCentroCommandHandler : IRequestHandler<CreateCentroCommand, CentroResult>
{
    private readonly PeiaDbContext _context;
    public CreateCentroCommandHandler(PeiaDbContext context) => _context = context;

    public async Task<CentroResult> Handle(CreateCentroCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Request.Nombre) || string.IsNullOrWhiteSpace(request.Request.Codigo))
            return new CentroResult(false, null, "Nombre y código son obligatorios.");

        var codigo = request.Request.Codigo.Trim().ToUpperInvariant();
        if (await _context.Centros.AnyAsync(c => c.Codigo == codigo, cancellationToken))
            return new CentroResult(false, null, $"Ya existe un centro con código '{codigo}'.");

        var centro = new Centro
        {
            Id = Guid.NewGuid(),
            Nombre = request.Request.Nombre.Trim(),
            Codigo = codigo,
            Direccion = request.Request.Direccion?.Trim() ?? string.Empty,
            Activo = request.Request.Activo
        };

        _context.Centros.Add(centro);
        await _context.SaveChangesAsync(cancellationToken);

        return new CentroResult(true, new CentroResponse(centro.Id, centro.Nombre, centro.Codigo, centro.Direccion, centro.Activo));
    }
}

public class UpdateCentroCommandHandler : IRequestHandler<UpdateCentroCommand, CentroResult>
{
    private readonly PeiaDbContext _context;
    public UpdateCentroCommandHandler(PeiaDbContext context) => _context = context;

    public async Task<CentroResult> Handle(UpdateCentroCommand request, CancellationToken cancellationToken)
    {
        var centro = await _context.Centros.FindAsync(new object[] { request.Id }, cancellationToken);
        if (centro is null)
            return new CentroResult(false, null, "Centro no encontrado.");

        if (string.IsNullOrWhiteSpace(request.Request.Nombre) || string.IsNullOrWhiteSpace(request.Request.Codigo))
            return new CentroResult(false, null, "Nombre y código son obligatorios.");

        var codigo = request.Request.Codigo.Trim().ToUpperInvariant();
        if (await _context.Centros.AnyAsync(c => c.Id != request.Id && c.Codigo == codigo, cancellationToken))
            return new CentroResult(false, null, $"Ya existe un centro con código '{codigo}'.");

        centro.Nombre = request.Request.Nombre.Trim();
        centro.Codigo = codigo;
        centro.Direccion = request.Request.Direccion?.Trim() ?? string.Empty;
        centro.Activo = request.Request.Activo;

        await _context.SaveChangesAsync(cancellationToken);
        return new CentroResult(true, new CentroResponse(centro.Id, centro.Nombre, centro.Codigo, centro.Direccion, centro.Activo));
    }
}

public class DeleteCentroCommandHandler : IRequestHandler<DeleteCentroCommand, CentroResult>
{
    private readonly PeiaDbContext _context;
    public DeleteCentroCommandHandler(PeiaDbContext context) => _context = context;

    public async Task<CentroResult> Handle(DeleteCentroCommand request, CancellationToken cancellationToken)
    {
        var centro = await _context.Centros.FindAsync(new object[] { request.Id }, cancellationToken);
        if (centro is null)
            return new CentroResult(false, null, "Centro no encontrado.");

        centro.Activo = false;
        await _context.SaveChangesAsync(cancellationToken);

        return new CentroResult(true);
    }
}
