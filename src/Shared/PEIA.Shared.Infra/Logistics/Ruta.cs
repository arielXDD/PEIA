using System;
using System.Collections.Generic;

namespace PEIA.Shared.Infra.Logistics;

public class Ruta
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public decimal DistanciaKm { get; set; }
    public bool Activa { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relación: Una ruta puede tener muchos pedidos
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
