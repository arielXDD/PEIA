using System;
using PEIA.Shared.Infra.Identity;

namespace PEIA.Shared.Infra.Inventory;

public class Stock
{
    public Guid Id { get; set; }
    public int Cantidad { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public string? Lote { get; set; }
    public DateTime? FechaCaducidad { get; set; }
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public Guid CentroId { get; set; }
    public Centro? Centro { get; set; }
}
