using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PEIA.Shared.Infra.Configuration;
using PEIA.Shared.Infra.Data;

namespace PEIA.Shared.Infra.Seed;

public static class SeedDataConfiguracionVariadaDemo
{
    public static async Task EnsureAsync(PeiaDbContext db, ILogger logger)
    {
        var settings = new Dictionary<string, object>
        {
            ["demo.dashboard.widgets"] = new
            {
                visible = new[] { "inventario", "pedidos", "alertas", "camaras", "prediccion" },
                refreshSeconds = 45,
                periodoDefault = "ultimos_7_dias"
            },
            ["demo.reportes.exportacion"] = new
            {
                formatos = new[] { "pdf", "excel", "csv" },
                limiteFilas = 500000,
                incluirGraficas = true
            },
            ["demo.inventario.auditoria"] = new
            {
                conteoCiclicoDias = 14,
                toleranciaDiferencia = 3,
                requiereEvidencia = true
            },
            ["demo.logistica.sla"] = new
            {
                horasEntregaLocal = 24,
                horasEntregaForanea = 72,
                alertarAntesMinutos = 120
            },
            ["demo.camaras.monitoreo"] = new
            {
                zonasCriticas = new[] { "Anden", "Picking", "Embarques", "Patio" },
                alertaSinSenalMinutos = 5,
                retencionDias = 30
            }
        };

        foreach (var setting in settings)
        {
            var value = JsonSerializer.Serialize(setting.Value);
            var existing = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == setting.Key);
            if (existing is null)
            {
                db.SystemSettings.Add(new SystemSetting
                {
                    Key = setting.Key,
                    Value = value,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.Value = value;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seed configuracion variada demo verificado: {Cantidad} settings.", settings.Count);
    }
}
