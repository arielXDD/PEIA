using MediatR;

namespace PEIA.Modules.Logistics.Notifications;

public record SlaVencidoNotification(
    Guid SlaId,
    Guid PedidoId,
    string PedidoCodigo,
    string Cliente,
    Guid CentroId,
    DateTime TiempoLimite) : INotification;
