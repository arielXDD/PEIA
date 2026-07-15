using MediatR;
using Microsoft.EntityFrameworkCore;
using PEIA.Shared.Infra.Data;

namespace PEIA.Modules.Inventory.Handlers;

public record StockConsolidadoResponse(
    Guid ProductoId,
    string Sku,
    string Nombre,
    string UnidadMedida,
    decimal CantidadTotal
);

public record GetStockConsolidadoQuery() : IRequest<List<StockConsolidadoResponse>>;

public class GetStockConsolidadoQueryHandler : IRequestHandler<GetStockConsolidadoQuery, List<StockConsolidadoResponse>>
{
    private readonly PeiaDbContext _context;

    public GetStockConsolidadoQueryHandler(PeiaDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockConsolidadoResponse>> Handle(GetStockConsolidadoQuery request, CancellationToken cancellationToken)
    {
        var consolidado = await _context.Stocks
            .Include(s => s.Producto)
            .GroupBy(s => new { s.ProductoId, s.Producto!.Sku, s.Producto.Nombre, s.Producto.UnidadMedida })
            .Select(g => new StockConsolidadoResponse(
                g.Key.ProductoId,
                g.Key.Sku,
                g.Key.Nombre,
                g.Key.UnidadMedida,
                g.Sum(s => s.Cantidad)
            ))
            .OrderBy(s => s.Nombre)
            .ToListAsync(cancellationToken);

        return consolidado;
    }
}
