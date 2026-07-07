using MediatR;
using Microsoft.AspNetCore.SignalR;
using PEIA.Modules.Logistics.Notifications;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Notifications;
using PEIA.Web.Hubs;

namespace PEIA.Web.Notifications;

public class PedidoCreadoSignalRHandler : INotificationHandler<PedidoCreadoNotification>
{
    private readonly IHubContext<PeiaHub> _hubContext;
    private readonly PeiaDbContext _context;

    public PedidoCreadoSignalRHandler(IHubContext<PeiaHub> hubContext, PeiaDbContext context)
    {
        _hubContext = hubContext;
        _context = context;
    }

    public async Task Handle(PedidoCreadoNotification notification, CancellationToken cancellationToken)
    {
        var titulo = $"Nuevo pedido: {notification.Codigo}";
        var descripcion = $"{notification.Cliente} ha realizado el pedido {notification.Codigo}.";

        var registro = new Notificacion
        {
            Id = Guid.NewGuid(),
            Tipo = "pedido",
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
            tipo = "pedido",
            prioridad = "media",
            titulo,
            descripcion,
            pedidoId = notification.PedidoId,
            notification.Codigo,
            notification.Cliente,
            notification.CentroId,
            notification.FechaEstimadaEntrega,
            fecha = registro.FechaCreacion
        };

        await _hubContext.Clients
            .Group(PeiaHub.GetCentroGroup(notification.CentroId))
            .SendAsync("notificacion", payload, cancellationToken);
    }
}
