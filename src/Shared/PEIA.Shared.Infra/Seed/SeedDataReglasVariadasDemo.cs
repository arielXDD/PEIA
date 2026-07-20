using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PEIA.Shared.Infra.Automation;
using PEIA.Shared.Infra.Data;

namespace PEIA.Shared.Infra.Seed;

public static class SeedDataReglasVariadasDemo
{
    public static async Task EnsureAsync(PeiaDbContext db, ILogger logger)
    {
        var reglas = new[]
        {
            Rule("VAR Stock critico por categoria", "stock_critico", "Stock <= Minimo AND Categoria != 'Seguridad'", "notificar_inventario", "OperadorInventario"),
            Rule("VAR Stock seguridad obligatorio", "stock_critico", "Categoria = 'Seguridad' AND Stock <= Minimo", "notificar_supervisor", "Supervisor"),
            Rule("VAR Pedido urgente cliente prioritario", "nuevo_pedido", "ClientePrioritario = true", "priorizar_ruta", "Logistica"),
            Rule("VAR SLA en riesgo menor a dos horas", "sla_en_riesgo", "HorasRestantes <= 2", "alertar_logistica", "Logistica"),
            Rule("VAR Movimiento negativo bloqueado", "salida_stock", "StockNuevo < 0", "bloquear_movimiento", "Administrador"),
            Rule("VAR Conteo por ajustes frecuentes", "inventario", "AjustesSemana >= 5", "programar_conteo", "OperadorInventario"),
            Rule("VAR Camara critica desconectada", "camara", "Zona = 'Anden' AND MinutosSinSenal >= 5", "crear_incidencia", "Supervisor"),
            Rule("VAR Reporte diario automatico", "reporte_diario", "Hora = 18:30", "enviar_reporte", "Reportes"),
            Rule("VAR Ruta larga requiere validacion", "asignacion_ruta", "DistanciaKm >= 40", "solicitar_confirmacion", "Logistica"),
            Rule("VAR Producto caducidad cercana", "caducidad", "DiasCaducidad <= 20", "notificar_reposicion", "OperadorInventario"),
            Rule("VAR Pedido cancelado auditable", "pedido_cancelado", "Todos", "registrar_bitacora", "Supervisor"),
            Rule("VAR Cambio de permisos sensible", "usuario_actualizado", "Rol IN ('Administrador','Supervisor')", "solicitar_aprobacion", "Administrador")
        };

        var existentes = (await db.ReglasAutomatizacion.Select(r => r.Nombre).ToListAsync()).ToHashSet();
        var nuevas = reglas
            .Where(r => !existentes.Contains(r.Nombre))
            .Select(r => new ReglaAutomatizacion
            {
                Id = Guid.NewGuid(),
                Nombre = r.Nombre,
                EventoOrigen = r.EventoOrigen,
                Condicion = r.Condicion,
                Accion = r.Accion,
                Responsable = r.Responsable,
                Activa = true
            })
            .ToList();

        if (nuevas.Count == 0) return;

        db.ReglasAutomatizacion.AddRange(nuevas);
        await db.SaveChangesAsync();
        logger.LogInformation("Seed reglas variadas demo agregado: {Cantidad} reglas.", nuevas.Count);
    }

    private static (string Nombre, string EventoOrigen, string Condicion, string Accion, string Responsable)
        Rule(string nombre, string eventoOrigen, string condicion, string accion, string responsable)
        => (nombre, eventoOrigen, condicion, accion, responsable);
}
