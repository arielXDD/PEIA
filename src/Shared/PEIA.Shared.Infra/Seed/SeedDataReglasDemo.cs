using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PEIA.Shared.Infra.Automation;
using PEIA.Shared.Infra.Data;

namespace PEIA.Shared.Infra.Seed;

public static class SeedDataReglasDemo
{
    public static async Task EnsureAsync(PeiaDbContext db, ILogger logger)
    {
        var reglas = new[]
        {
            Rule("DEMO Reorden automatico por stock critico", "stock_critico", "Stock <= Minimo AND Categoria IN ('Insumos','Embalaje')", "crear_solicitud_compra", "OperadorInventario"),
            Rule("DEMO Escalar SLA vencido a supervisor", "sla_vencido", "MinutosVencido >= 30", "notificar_supervisor", "Supervisor"),
            Rule("DEMO Asignar pedido urgente", "nuevo_pedido", "ClientePrioritario = true OR SLA <= 24h", "priorizar_asignacion", "Logistica"),
            Rule("DEMO Reportar camara desconectada", "camara", "Estado = 'Desconectada' AND MinutosSinSenal >= 10", "crear_incidencia", "Supervisor"),
            Rule("DEMO Conteo ciclico por ajuste frecuente", "inventario", "AjustesDia >= 3", "programar_auditoria", "OperadorInventario"),
            Rule("DEMO Bloquear salida con stock negativo", "salida_stock", "StockNuevo < 0", "bloquear_movimiento", "Administrador"),
            Rule("DEMO Avisar caducidad proxima", "caducidad", "DiasCaducidad <= 30", "notificar_inventario", "OperadorInventario"),
            Rule("DEMO Validar ruta larga", "asignacion_ruta", "DistanciaKm >= 35", "solicitar_confirmacion", "Logistica"),
            Rule("DEMO Resumen diario de reportes", "reporte_diario", "Hora = 18:00", "enviar_resumen", "Reportes"),
            Rule("DEMO Reglas sensibles requieren aprobacion", "regla_modificada", "Prioridad = 'Alta'", "solicitar_aprobacion", "Administrador")
        };

        var existentes = await db.ReglasAutomatizacion.Select(r => r.Nombre).ToListAsync();
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
        logger.LogInformation("Seed demo reglas: {Cantidad} reglas agregadas.", nuevas.Count);
    }

    private static (string Nombre, string EventoOrigen, string Condicion, string Accion, string Responsable)
        Rule(string nombre, string eventoOrigen, string condicion, string accion, string responsable)
        => (nombre, eventoOrigen, condicion, accion, responsable);
}
