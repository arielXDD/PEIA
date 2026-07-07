using MediatR;
using Microsoft.AspNetCore.SignalR;
using PEIA.Modules.Inventory.Notifications;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Notifications;
using PEIA.Web.Hubs;

namespace PEIA.Web.Notifications;

public class StockMinimoSignalRHandler : INotificationHandler<StockMinimoAlcanzadoNotification>
{
    private readonly IHubContext<PeiaHub> _hubContext;
    private readonly PeiaDbContext _context;

    public StockMinimoSignalRHandler(IHubContext<PeiaHub> hubContext, PeiaDbContext context)
    {
        _hubContext = hubContext;
        _context = context;
    }

    public async Task Handle(StockMinimoAlcanzadoNotification notification, CancellationToken cancellationToken)
    {
        var titulo = $"Stock crítico: {notification.Producto}";
        var descripcion = $"Quedan {notification.StockActual} unidades — el mínimo es {notification.StockMinimo}.";

        var registro = new Notificacion
        {
            Id = Guid.NewGuid(),
            Tipo = "stock",
            Titulo = titulo,
            Descripcion = descripcion,
            CentroId = notification.CentroId,
            Leida = false,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Notificaciones.Add(registro);
        await _context.SaveChangesAsync(cancellationToken);

        var payload = new
        {
            id = registro.Id,
            tipo = "stock",
            prioridad = "alta",
            titulo,
            descripcion,
            productoId = notification.ProductoId,
            notification.Sku,
            notification.CentroId,
            notification.StockActual,
            notification.StockMinimo,
            fecha = registro.FechaCreacion
        };

        await _hubContext.Clients
            .Group(PeiaHub.GetCentroGroup(notification.CentroId))
            .SendAsync("notificacion", payload, cancellationToken);
    }
}
