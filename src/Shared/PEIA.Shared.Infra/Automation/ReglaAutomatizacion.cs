using System;

namespace PEIA.Shared.Infra.Automation;

public class ReglaAutomatizacion
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string EventoOrigen { get; set; } = string.Empty; // "stock_critico", "nuevo_pedido", "sla_vencido"
    public string Condicion { get; set; } = string.Empty; // Ej. "Stock < Minimo", "Todos"
    public string Accion { get; set; } = string.Empty; // Ej. "notificar", "email"
    public string Responsable { get; set; } = string.Empty; // Usuario o Rol asignado
    public bool Activa { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
