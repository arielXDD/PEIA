using System;
using PEIA.Shared.Infra.Identity;

namespace PEIA.Shared.Infra.Logistics;

public class EntregaEstado
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public string Estado { get; set; } = string.Empty; // Ej: Creado, Asignado, EnRuta, Entregado, Cancelado
    public string? Descripcion { get; set; }
    
    // Coordenadas para rastreo GPS en tiempo real
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }

    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public Guid? ActualizadoPorId { get; set; }
    public Usuario? ActualizadoPor { get; set; }
}
