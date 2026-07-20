using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Notifications;

namespace PEIA.Shared.Infra.Seed;

public static class SeedDataNotificacionesDemo
{
    public static async Task EnsureAsync(PeiaDbContext db, ILogger logger)
    {
        var centros = await db.Centros.OrderBy(c => c.Codigo).ToListAsync();
        if (centros.Count == 0) return;

        var plantillas = new[]
        {
            ("stock_critico", "Stock critico en zona de picking", "Un producto prioritario esta por debajo del minimo operativo."),
            ("sla_vencido", "SLA vencido en pedido de cliente", "Un pedido supero su tiempo limite de entrega y requiere seguimiento."),
            ("camara", "Camara con intermitencia", "El monitoreo detecto perdida temporal de senal en una zona operativa."),
            ("pedido", "Pedido pendiente de asignacion", "Hay pedidos creados sin ruta o transportista asignado."),
            ("inventario", "Auditoria ciclica programada", "Se requiere conteo fisico de ubicaciones con alta rotacion."),
            ("seguridad", "Revision de equipo de proteccion", "Equipo de seguridad requiere reposicion o validacion."),
            ("sistema", "Sincronizacion completada", "Los datos operativos fueron actualizados para el centro."),
            ("regla", "Regla automatica ejecutada", "Una regla de negocio genero una accion preventiva.")
        };

        var existentes = await db.Notificaciones.Select(n => n.Titulo).ToListAsync();
        var nuevas = new List<Notificacion>();
        var random = new Random(20260723);

        foreach (var centro in centros)
        {
            for (var i = 0; i < plantillas.Length; i++)
            {
                var plantilla = plantillas[i];
                var titulo = $"DEMO {centro.Codigo}: {plantilla.Item2}";
                if (existentes.Contains(titulo)) continue;

                nuevas.Add(new Notificacion
                {
                    Id = Guid.NewGuid(),
                    CentroId = centro.Id,
                    Tipo = plantilla.Item1,
                    Titulo = titulo,
                    Descripcion = $"{plantilla.Item3} Centro: {centro.Nombre}.",
                    Leida = i % 3 == 0,
                    FechaCreacion = DateTime.UtcNow.AddMinutes(-random.Next(5, 720))
                });
            }
        }

        if (nuevas.Count == 0) return;

        db.Notificaciones.AddRange(nuevas);
        await db.SaveChangesAsync();
        logger.LogInformation("Seed demo notificaciones: {Cantidad} notificaciones agregadas.", nuevas.Count);
    }
}
