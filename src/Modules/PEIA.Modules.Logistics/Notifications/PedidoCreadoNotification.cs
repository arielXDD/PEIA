using MediatR;

namespace PEIA.Modules.Logistics.Notifications;

public record PedidoCreadoNotification(
    Guid PedidoId,
    string Codigo,
    string Cliente,
    Guid CentroId,
    DateTime FechaEstimadaEntrega) : INotification;
