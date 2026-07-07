using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PEIA.Shared.Infra.Configuration;
using PEIA.Shared.Infra.Data;

namespace PEIA.Web.Controllers;

[ApiController]
[Route("api/configuracion")]
[Authorize(Roles = "Administrador")]
public class ConfiguracionController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly PeiaDbContext _context;

    private static readonly EmpresaConfig DefaultEmpresa = new(
        "PEIA Corp.",
        null,
        null,
        null,
        "contacto@peia.local");

    private static readonly PreferenciasConfig DefaultPreferencias = new(
        "America/Mexico_City",
        "DD/MM/YYYY",
        "MXN",
        10);

    private static readonly NotificacionesConfig DefaultNotificaciones = new(
        true,
        true,
        true,
        false,
        false);

    private static readonly SeguridadConfig DefaultSeguridad = new(
        8,
        true,
        true);

    public ConfiguracionController(PeiaDbContext context)
    {
        _context = context;
    }

    [HttpGet("empresa")]
    public async Task<IActionResult> GetEmpresa() => Ok(await GetSettingAsync("empresa", DefaultEmpresa));

    [HttpPut("empresa")]
    public async Task<IActionResult> UpdateEmpresa([FromBody] EmpresaConfig request)
    {
        await SetSettingAsync("empresa", request);
        return Ok(request);
    }

    [HttpGet("preferencias")]
    public async Task<IActionResult> GetPreferencias() => Ok(await GetSettingAsync("preferencias", DefaultPreferencias));

    [HttpPut("preferencias")]
    public async Task<IActionResult> UpdatePreferencias([FromBody] PreferenciasConfig request)
    {
        await SetSettingAsync("preferencias", request);
        return Ok(request);
    }

    [HttpGet("notificaciones")]
    public async Task<IActionResult> GetNotificaciones() => Ok(await GetSettingAsync("notificaciones", DefaultNotificaciones));

    [HttpPut("notificaciones")]
    public async Task<IActionResult> UpdateNotificaciones([FromBody] NotificacionesConfig request)
    {
        await SetSettingAsync("notificaciones", request);
        return Ok(request);
    }

    [HttpGet("seguridad")]
    public async Task<IActionResult> GetSeguridad() => Ok(await GetSettingAsync("seguridad", DefaultSeguridad));

    [HttpPut("seguridad")]
    public async Task<IActionResult> UpdateSeguridad([FromBody] SeguridadConfig request)
    {
        if (request.PasswordMinLength < 6)
        {
            return BadRequest(new { message = "La longitud mínima de contraseña debe ser al menos 6." });
        }

        await SetSettingAsync("seguridad", request);
        return Ok(request);
    }

    private async Task<T> GetSettingAsync<T>(string key, T defaultValue)
    {
        var setting = await _context.SystemSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null)
        {
            return defaultValue;
        }

        return JsonSerializer.Deserialize<T>(setting.Value, JsonOptions) ?? defaultValue;
    }

    private async Task SetSettingAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null)
        {
            _context.SystemSettings.Add(new SystemSetting
            {
                Key = key,
                Value = json,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            setting.Value = json;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}

public record EmpresaConfig(string Nombre, string? Rfc, string? Telefono, string? Direccion, string? Email);
public record PreferenciasConfig(string Timezone, string DateFormat, string Currency, int PageSize);
public record NotificacionesConfig(bool Stock, bool Sla, bool Pedido, bool Camara, bool Email);
public record SeguridadConfig(int PasswordMinLength, bool RequireUppercase, bool RequireDigit);
