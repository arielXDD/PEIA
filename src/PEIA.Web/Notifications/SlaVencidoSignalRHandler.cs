using MediatR;
using Microsoft.AspNetCore.SignalR;
using PEIA.Modules.Logistics.Notifications;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Notifications;
using PEIA.Web.Hubs;

namespace PEIA.Web.Notifications;

public class SlaVencidoSignalRHandler : INotificationHandler<SlaVencidoNotification>
{
    private readonly IHubContext<PeiaHub> _hubContext;
    private readonly PeiaDbContext _context;

    public SlaVencidoSignalRHandler(IHubContext<PeiaHub> hubContext, PeiaDbContext context)
    {
        _hubContext = hubContext;
        _context = context;
    }

    public async Task Handle(SlaVencidoNotification notification, CancellationToken cancellationToken)
    {
        var titulo = $"SLA vencido: {notification.PedidoCodigo}";
        var descripcion = $"El pedido {notification.PedidoCodigo} de {notification.Cliente} superó su plazo de entrega.";

        var registro = new Notificacion
        {
            Id = Guid.NewGuid(),
            Tipo = "sla",
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
            tipo = "sla",
            prioridad = "alta",
            titulo,
            descripcion,
            slaId = notification.SlaId,
            pedidoId = notification.PedidoId,
            codigo = notification.PedidoCodigo,
            notification.Cliente,
            notification.CentroId,
            notification.TiempoLimite,
            fecha = registro.FechaCreacion
        };

        await _hubContext.Clients
            .Group(PeiaHub.GetCentroGroup(notification.CentroId))
            .SendAsync("notificacion", payload, cancellationToken);
    }
}
