using System;

namespace PEIA.Shared.Infra.Logistics;

public class SLA
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public DateTime TiempoLimite { get; set; }
    public string EstadoSLA { get; set; } = "EnRiesgo"; // Ej: Cumplido, EnRiesgo, Incumplido
    public DateTime? FechaResolucion { get; set; }
}
