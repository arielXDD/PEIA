using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Notifications;

namespace PEIA.Shared.Infra.Seed;

public static class SeedDataNotificacionesVariadasDemo
{
    public static async Task EnsureAsync(PeiaDbContext db, ILogger logger)
    {
        var centros = await db.Centros.OrderBy(c => c.Codigo).ToListAsync();
        if (centros.Count == 0) return;

        var plantillas = new[]
        {
            Notice("stock_critico", "VAR Stock bajo en producto de alta rotacion", "Revisar reposicion inmediata antes del siguiente corte operativo."),
            Notice("inventario", "VAR Conteo ciclico programado", "Ejecutar conteo fisico en ubicaciones con diferencias recientes."),
            Notice("pedido", "VAR Pedido sin transportista asignado", "Asignar responsable logistico para evitar retrasos en SLA."),
            Notice("sla_vencido", "VAR SLA vencido requiere escalamiento", "El pedido excedio el tiempo limite configurado."),
            Notice("camara", "VAR Camara con perdida de senal", "Validar energia, red y visibilidad de la zona monitoreada."),
            Notice("seguridad", "VAR Equipo de seguridad incompleto", "Solicitar reposicion de EPP para el turno operativo."),
            Notice("reporte", "VAR Reporte listo para revision", "Reporte operativo generado con datos consolidados del centro."),
            Notice("regla", "VAR Regla automatica ejecutada", "Una condicion configurada activo una accion preventiva.")
        };

        var existentes = (await db.Notificaciones.Select(n => n.Titulo).ToListAsync()).ToHashSet();
        var random = new Random(2026072403);
        var nuevas = new List<Notificacion>();

        foreach (var centro in centros)
        {
            for (var i = 0; i < plantillas.Length; i++)
            {
                var plantilla = plantillas[i];
                var titulo = $"{plantilla.Titulo} - {centro.Codigo}";
                if (existentes.Contains(titulo)) continue;

                nuevas.Add(new Notificacion
                {
                    Id = Guid.NewGuid(),
                    CentroId = centro.Id,
                    Tipo = plantilla.Tipo,
                    Titulo = titulo,
                    Descripcion = $"{plantilla.Descripcion} Centro: {centro.Nombre}.",
                    Leida = i % 4 == 0,
                    FechaCreacion = DateTime.UtcNow.AddMinutes(-random.Next(1, 1440))
                });
            }
        }

        if (nuevas.Count == 0) return;

        db.Notificaciones.AddRange(nuevas);
        await db.SaveChangesAsync();
        logger.LogInformation("Seed notificaciones variadas demo agregado: {Cantidad} notificaciones.", nuevas.Count);
    }

    private static (string Tipo, string Titulo, string Descripcion)
        Notice(string tipo, string titulo, string descripcion)
        => (tipo, titulo, descripcion);
}
