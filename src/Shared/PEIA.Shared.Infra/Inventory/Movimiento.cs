using System;
using PEIA.Shared.Infra.Identity;

namespace PEIA.Shared.Infra.Inventory;

public class Movimiento
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty; // Entrada, Salida, Ajuste
    public int Cantidad { get; set; }
    public int StockAnterior { get; set; }
    public int StockNuevo { get; set; }
    public string? Motivo { get; set; }
    public string? Referencia { get; set; }
    public string? Lote { get; set; }
    public DateTime? FechaCaducidad { get; set; }
    public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;

    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public Guid CentroId { get; set; }
    public Centro? Centro { get; set; }

    public Guid? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
}
