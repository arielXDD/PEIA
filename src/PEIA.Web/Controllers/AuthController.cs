using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PEIA.Shared.Infra.Configuration;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Identity;

namespace PEIA.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly PeiaDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        IConfiguration configuration,
        PeiaDbContext context,
        IWebHostEnvironment environment,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmailOrUsername) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Usuario y contraseña son requeridos." });
        }

        // Buscar por email o username
        var user = await _userManager.FindByEmailAsync(request.EmailOrUsername);
        if (user == null)
        {
            user = await _userManager.FindByNameAsync(request.EmailOrUsername);
        }

        if (user == null || !user.Activo)
        {
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });
        }

        // Verificar contraseña
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return StatusCode(StatusCodes.Status423Locked, new { message = "La cuenta está bloqueada temporalmente por intentos fallidos." });
        }

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });
        }

        // Obtener roles del usuario
        var roles = await _userManager.GetRolesAsync(user);

        // Obtener centros asignados al usuario
        var centros = await _context.UsuarioCentros
            .Where(uc => uc.UsuarioId == user.Id && uc.Activo)
            .Select(uc => new
            {
                uc.Centro.Id,
                uc.Centro.Nombre,
                uc.Centro.Codigo
            })
            .ToListAsync();

        // Generar token JWT
        var token = await GenerateJwtTokenAsync(user, roles);
        await RegisterSessionAsync(user.Id, token.JwtId, token.ExpiresAt);

        return Ok(new
        {
            token = token.Value,
            user = new
            {
                id = user.Id,
                userName = user.UserName,
                email = user.Email,
                nombreCompleto = user.NombreCompleto,
                roles = roles,
                centros = centros
            }
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmailOrUsername))
        {
            return BadRequest(new { message = "Indica tu usuario o correo." });
        }

        var user = await _userManager.FindByEmailAsync(request.EmailOrUsername.Trim())
            ?? await _userManager.FindByNameAsync(request.EmailOrUsername.Trim());

        const string genericMessage = "Si la cuenta existe, se generó una solicitud para restablecer la contraseña.";
        if (user is null || !user.Activo)
        {
            return Ok(new { message = genericMessage });
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        _logger.LogInformation("Token de recuperación generado para {UserId}.", user.Id);

        if (!_environment.IsDevelopment())
        {
            return Ok(new { message = genericMessage });
        }

        var encodedToken = WebUtility.UrlEncode(token);
        var resetUrl = $"/Login?reset=true&user={WebUtility.UrlEncode(user.Email ?? user.UserName)}&token={encodedToken}";
        return Ok(new
        {
            message = genericMessage,
            resetToken = token,
            resetUrl
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmailOrUsername) ||
            string.IsNullOrWhiteSpace(request.Token) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { message = "Usuario, token y nueva contraseña son obligatorios." });
        }

        var user = await _userManager.FindByEmailAsync(request.EmailOrUsername.Trim())
            ?? await _userManager.FindByNameAsync(request.EmailOrUsername.Trim());

        if (user is null || !user.Activo)
        {
            return BadRequest(new { message = "No se pudo restablecer la contraseña." });
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "No se pudo restablecer la contraseña.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        return Ok(new { message = "Contraseña restablecida correctamente. Ya puedes iniciar sesión." });
    }

    [Authorize]
    [HttpGet("sesiones")]
    public async Task<IActionResult> GetSesiones()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var currentJwtId = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var now = DateTime.UtcNow;
        var sesiones = await _context.UserSessions
            .AsNoTracking()
            .Where(s => s.UsuarioId == userId.Value && !s.Revocada && s.FechaExpiracion > now)
            .OrderByDescending(s => s.FechaInicio)
            .Select(s => new
            {
                s.Id,
                s.FechaInicio,
                s.FechaExpiracion,
                s.IpAddress,
                s.UserAgent,
                actual = s.JwtId == currentJwtId
            })
            .ToListAsync();

        return Ok(sesiones);
    }

    [Authorize]
    [HttpDelete("sesiones/{id:guid}")]
    public async Task<IActionResult> CerrarSesion(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.Id == id && s.UsuarioId == userId.Value && !s.Revocada);

        if (session is null)
        {
            return NotFound(new { message = "Sesión no encontrada." });
        }

        session.Revocada = true;
        session.FechaRevocacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpDelete("sesiones")]
    public async Task<IActionResult> CerrarOtrasSesiones()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var currentJwtId = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var sessions = await _context.UserSessions
            .Where(s => s.UsuarioId == userId.Value && !s.Revocada && s.JwtId != currentJwtId)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var session in sessions)
        {
            session.Revocada = true;
            session.FechaRevocacion = now;
        }

        await _context.SaveChangesAsync();
        return Ok(new { closed = sessions.Count });
    }

    private async Task<JwtTokenResult> GenerateJwtTokenAsync(Usuario user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);
        var expiryMinutes = await GetSessionExpiryMinutesAsync();
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
        var jwtId = Guid.NewGuid().ToString("N");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jwtId),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim("nombreCompleto", user.NombreCompleto)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new JwtTokenResult(tokenHandler.WriteToken(token), jwtId, expiresAt);
    }

    private async Task RegisterSessionAsync(Guid userId, string jwtId, DateTime expiresAt)
    {
        var ipAddress = Truncate(HttpContext.Connection.RemoteIpAddress?.ToString(), 80);
        var userAgent = Truncate(Request.Headers.UserAgent.ToString(), 500);

        _context.UserSessions.Add(new UserSession
        {
            UsuarioId = userId,
            JwtId = jwtId,
            FechaInicio = DateTime.UtcNow,
            FechaExpiracion = expiresAt,
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        await _context.SaveChangesAsync();
    }

    private Guid? GetCurrentUserId()
    {
        var rawId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(rawId, out var userId) ? userId : null;
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private async Task<double> GetSessionExpiryMinutesAsync()
    {
        var fallback = double.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "180");
        var setting = await _context.SystemSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == "seguridad");
        if (setting is null)
        {
            return fallback;
        }

        try
        {
            var seguridad = JsonSerializer.Deserialize<SecurityTokenSettings>(setting.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var minutes = seguridad?.SessionTimeoutMinutes ?? (int)fallback;
            return minutes <= 0 ? 43200 : Math.Clamp(minutes, 5, 43200);
        }
        catch (JsonException)
        {
            return fallback;
        }
    }
}

public class LoginRequest
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    public string EmailOrUsername { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public record SecurityTokenSettings(
    int PasswordMinLength,
    bool RequireUppercase,
    bool RequireDigit,
    bool RequireTwoFactor,
    int SessionTimeoutMinutes);

public record JwtTokenResult(string Value, string JwtId, DateTime ExpiresAt);
