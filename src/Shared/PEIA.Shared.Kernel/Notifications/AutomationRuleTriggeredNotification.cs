using MediatR;
using System;

namespace PEIA.Shared.Kernel.Notifications;

public record AutomationRuleTriggeredNotification(
    Guid NotificacionId,
    string Tipo,
    string Titulo,
    string Descripcion,
    Guid CentroId,
    string Responsable,
    DateTime FechaCreacion) : INotification;
