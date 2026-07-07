using MediatR;
using Microsoft.EntityFrameworkCore;
using PEIA.Modules.Logistics.Notifications;
using PEIA.Shared.Infra.Data;

namespace PEIA.Web.Services;

public class SlaMonitorService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SlaMonitorService> _logger;

    public SlaMonitorService(IServiceScopeFactory scopeFactory, ILogger<SlaMonitorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckSlaAsync(stoppingToken);

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task CheckSlaAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PeiaDbContext>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var now = DateTime.UtcNow;

            var vencidos = await context.SLAs
                .Include(s => s.Pedido)
                .Where(s => s.EstadoSLA == "EnRiesgo" &&
                            s.FechaResolucion == null &&
                            s.TiempoLimite < now &&
                            s.Pedido != null)
                .ToListAsync(cancellationToken);

            foreach (var sla in vencidos)
            {
                sla.EstadoSLA = "Incumplido";

                await mediator.Publish(new SlaVencidoNotification(
                    sla.Id,
                    sla.PedidoId,
                    sla.Pedido!.Codigo,
                    sla.Pedido.Cliente,
                    sla.Pedido.CentroId,
                    sla.TiempoLimite), cancellationToken);
            }

            if (vencidos.Count > 0)
            {
                await context.SaveChangesAsync(cancellationToken);
                _logger.LogWarning("{Count} SLA(s) marcados como incumplidos.", vencidos.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al revisar SLAs vencidos.");
        }
    }
}
