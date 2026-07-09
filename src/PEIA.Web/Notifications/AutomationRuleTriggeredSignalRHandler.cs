using MediatR;
using Microsoft.AspNetCore.SignalR;
using PEIA.Shared.Kernel.Notifications;
using PEIA.Web.Hubs;
using System.Threading;
using System.Threading.Tasks;

namespace PEIA.Web.Notifications;

public class AutomationRuleTriggeredSignalRHandler : INotificationHandler<AutomationRuleTriggeredNotification>
{
    private readonly IHubContext<PeiaHub> _hubContext;

    public AutomationRuleTriggeredSignalRHandler(IHubContext<PeiaHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(AutomationRuleTriggeredNotification notification, CancellationToken cancellationToken)
    {
        var payload = new
        {
            id = notification.NotificacionId,
            tipo = notification.Tipo,
            prioridad = "alta",
            titulo = notification.Titulo,
            descripcion = notification.Descripcion,
            centroId = notification.CentroId,
            responsable = notification.Responsable,
            fecha = notification.FechaCreacion
        };

        // Enviar por SignalR a todos los clientes del centro
        await _hubContext.Clients
            .Group(PeiaHub.GetCentroGroup(notification.CentroId))
            .SendAsync("notificacion", payload, cancellationToken);
    }
}
