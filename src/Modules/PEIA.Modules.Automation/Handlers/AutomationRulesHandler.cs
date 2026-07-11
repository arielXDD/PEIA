using MediatR;
using Microsoft.EntityFrameworkCore;
using PEIA.Modules.Inventory.Notifications;
using PEIA.Modules.Logistics.Notifications;
using PEIA.Shared.Infra.Automation;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Notifications;
using PEIA.Shared.Kernel.Notifications;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PEIA.Modules.Automation.Handlers;

public class AutomationRulesHandler : 
    INotificationHandler<StockMinimoAlcanzadoNotification>,
    INotificationHandler<PedidoCreadoNotification>,
    INotificationHandler<SlaVencidoNotification>
{
    private readonly PeiaDbContext _context;
    private readonly IMediator _mediator;

    public AutomationRulesHandler(PeiaDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    // 1. Manejar Stock Mínimo Alcanzado
    public async Task Handle(StockMinimoAlcanzadoNotification notification, CancellationToken cancellationToken)
    {
        var rules = await GetActiveRulesAsync("stock_critico", cancellationToken);
        foreach (var rule in rules)
        {
            if (EvaluateStockCondition(rule.Condicion, notification))
            {
                await TriggerRuleAsync(rule, notification.CentroId, 
                    $"Alerta de stock crítico para {notification.Producto}. Quedan {notification.StockActual} unidades (Mínimo: {notification.StockMinimo}).", 
                    cancellationToken);
            }
        }
    }

    // 2. Manejar Nuevo Pedido Registrado
    public async Task Handle(PedidoCreadoNotification notification, CancellationToken cancellationToken)
    {
        var rules = await GetActiveRulesAsync("nuevo_pedido", cancellationToken);
        foreach (var rule in rules)
        {
            if (EvaluatePedidoCondition(rule.Condicion, notification))
            {
                await TriggerRuleAsync(rule, notification.CentroId, 
                    $"Nuevo pedido registrado: {notification.Codigo} para el cliente {notification.Cliente}.", 
                    cancellationToken);
            }
        }
    }

    // 3. Manejar SLA Vencido
    public async Task Handle(SlaVencidoNotification notification, CancellationToken cancellationToken)
    {
        var rules = await GetActiveRulesAsync("sla_vencido", cancellationToken);
        foreach (var rule in rules)
        {
            if (EvaluateSlaCondition(rule.Condicion, notification))
            {
                await TriggerRuleAsync(rule, notification.CentroId, 
                    $"SLA Vencido para el pedido {notification.PedidoCodigo}. Límite era: {notification.TiempoLimite:g}.", 
                    cancellationToken);
            }
        }
    }

    private async Task<List<ReglaAutomatizacion>> GetActiveRulesAsync(string eventSource, CancellationToken cancellationToken)
    {
        return await _context.ReglasAutomatizacion
            .Where(r => r.Activa && r.EventoOrigen == eventSource)
            .ToListAsync(cancellationToken);
    }

    private bool EvaluateStockCondition(string condition, StockMinimoAlcanzadoNotification notif)
    {
        if (string.IsNullOrWhiteSpace(condition) || 
            condition.Equals("Todos", StringComparison.OrdinalIgnoreCase) || 
            condition.Equals("Siempre", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Condición personalizada: ej. SKU=123, o Umbral específico
        if (condition.StartsWith("SKU=", StringComparison.OrdinalIgnoreCase))
        {
            var sku = condition.Substring(4).Trim();
            return notif.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private bool EvaluatePedidoCondition(string condition, PedidoCreadoNotification notif)
    {
        if (string.IsNullOrWhiteSpace(condition) || 
            condition.Equals("Todos", StringComparison.OrdinalIgnoreCase) || 
            condition.Equals("Siempre", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (condition.StartsWith("Cliente=", StringComparison.OrdinalIgnoreCase))
        {
            var cliente = condition.Substring(8).Trim();
            return notif.Cliente.Contains(cliente, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private bool EvaluateSlaCondition(string condition, SlaVencidoNotification notif)
    {
        return true; // Para SLA vencido, habitualmente se disparan todas las reglas activas
    }

    private async Task TriggerRuleAsync(ReglaAutomatizacion rule, Guid centroId, string details, CancellationToken cancellationToken)
    {
        var titulo = $"[Regla] {rule.Nombre}";
        var descripcion = $"{details} Asignado a: {rule.Responsable}.";

        var notificacion = new Notificacion
        {
            Id = Guid.NewGuid(),
            Tipo = rule.Accion.ToLower() == "email" ? "email" : "regla",
            Titulo = titulo,
            Descripcion = descripcion,
            CentroId = centroId,
            Leida = false,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Notificaciones.Add(notificacion);
        await _context.SaveChangesAsync(cancellationToken);

        // Publicar notificación de integración de SignalR
        var sharedEvent = new AutomationRuleTriggeredNotification(
            notificacion.Id,
            notificacion.Tipo,
            notificacion.Titulo,
            notificacion.Descripcion,
            centroId,
            rule.Responsable,
            notificacion.FechaCreacion);

        await _mediator.Publish(sharedEvent, cancellationToken);
    }
}
