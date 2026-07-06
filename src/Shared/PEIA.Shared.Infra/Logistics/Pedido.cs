using System;
using System.Collections.Generic;
using PEIA.Shared.Infra.Identity;

namespace PEIA.Shared.Infra.Logistics;

public class Pedido
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string DireccionEntrega { get; set; } = string.Empty;
    public DateTime FechaPedido { get; set; } = DateTime.UtcNow;
    public DateTime FechaEstimadaEntrega { get; set; }
    public string Estado { get; set; } = "Creado"; // Ej: Creado, Asignado, EnRuta, Entregado, Cancelado

    // Relaciones
    public Guid CentroId { get; set; }
    public Centro? Centro { get; set; }

    public Guid? RutaId { get; set; }
    public Ruta? Ruta { get; set; }

    public Guid? TransportistaId { get; set; }
    public Usuario? Transportista { get; set; }

    // Un pedido tiene un SLA y un historial de estados de entrega
    public SLA? SLA { get; set; }
    public ICollection<EntregaEstado> EntregaEstados { get; set; } = new List<EntregaEstado>();
}
