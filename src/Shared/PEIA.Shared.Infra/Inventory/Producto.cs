using System;
using System.Collections.Generic;

namespace PEIA.Shared.Infra.Inventory;

public class Producto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string UnidadMedida { get; set; } = "Pieza";
    public decimal PrecioUnitario { get; set; }
    public int StockMinimo { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Guid CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
