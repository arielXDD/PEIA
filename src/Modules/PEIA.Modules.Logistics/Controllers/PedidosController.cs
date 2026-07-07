using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediatR;
using PEIA.Modules.Logistics.Notifications;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Identity;
using PEIA.Shared.Infra.Logistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PEIA.Modules.Logistics.Controllers;

[ApiController]
[Route("api/pedidos")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly PeiaDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly IMediator? _mediator;

    public PedidosController(PeiaDbContext context, UserManager<Usuario> userManager, IMediator? mediator = null)
    {
        _context = context;
        _userManager = userManager;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPedidos([FromQuery] Guid? centroId)
    {
        if (centroId == null || centroId == Guid.Empty)
        {
            return BadRequest(new { message = "El parámetro centroId es obligatorio." });
        }

        // Obtener todos los pedidos asociados a este centro con sus relaciones
        var pedidos = await _context.Pedidos
            .Where(p => p.CentroId == centroId)
            .Include(p => p.Ruta)
            .Include(p => p.Transportista)
            .Include(p => p.SLA)
            .OrderByDescending(p => p.FechaPedido)
            .Select(p => new
            {
                p.Id,
                p.Codigo,
                p.Cliente,
                p.DireccionEntrega,
                p.FechaPedido,
                p.FechaEstimadaEntrega,
                p.Estado,
                Ruta = p.Ruta != null ? new { p.Ruta.Id, p.Ruta.Nombre } : null,
                Transportista = p.Transportista != null ? new { p.Transportista.Id, p.Transportista.NombreCompleto } : null,
                SLA = p.SLA != null ? new { p.SLA.Id, p.SLA.TiempoLimite, p.SLA.EstadoSLA, p.SLA.FechaResolucion } : null
            })
            .ToListAsync();

        return Ok(pedidos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPedidoDetalle(Guid id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Ruta)
            .Include(p => p.Transportista)
            .Include(p => p.SLA)
            .Include(p => p.EntregaEstados)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
        {
            return NotFound(new { message = "Pedido no encontrado." });
        }

        var res = new
        {
            pedido.Id,
            pedido.Codigo,
            pedido.Cliente,
            pedido.DireccionEntrega,
            pedido.FechaPedido,
            pedido.FechaEstimadaEntrega,
            pedido.Estado,
            Ruta = pedido.Ruta != null ? new { pedido.Ruta.Id, pedido.Ruta.Nombre, pedido.Ruta.Origen, pedido.Ruta.Destino, pedido.Ruta.DistanciaKm } : null,
            Transportista = pedido.Transportista != null ? new { pedido.Transportista.Id, pedido.Transportista.NombreCompleto, pedido.Transportista.Email } : null,
            SLA = pedido.SLA != null ? new { pedido.SLA.Id, pedido.SLA.TiempoLimite, pedido.SLA.EstadoSLA, pedido.SLA.FechaResolucion } : null,
            EstadosRastreo = pedido.EntregaEstados
                .OrderByDescending(ee => ee.FechaActualizacion)
                .Select(ee => new
                {
                    ee.Id,
                    ee.Estado,
                    ee.Descripcion,
                    ee.Latitud,
                    ee.Longitud,
                    ee.FechaActualizacion,
                    ActualizadoPor = ee.ActualizadoPor != null ? ee.ActualizadoPor.NombreCompleto : "Sistema"
                })
        };

        return Ok(res);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePedido([FromBody] CreatePedidoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Codigo) ||
            string.IsNullOrWhiteSpace(request.Cliente) ||
            string.IsNullOrWhiteSpace(request.DireccionEntrega) ||
            request.CentroId == Guid.Empty ||
            request.FechaEstimadaEntrega <= DateTime.UtcNow)
        {
            return BadRequest(new { message = "Datos del pedido inválidos o incompletos." });
        }

        var existe = await _context.Pedidos.AnyAsync(p => p.Codigo == request.Codigo);
        if (existe)
        {
            return BadRequest(new { message = $"Ya existe un pedido con el código '{request.Codigo}'." });
        }

        var centroExiste = await _context.Centros.AnyAsync(c => c.Id == request.CentroId);
        if (!centroExiste)
        {
            return BadRequest(new { message = "El centro seleccionado no existe." });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var pedido = new Pedido
            {
                Id = Guid.NewGuid(),
                Codigo = request.Codigo,
                Cliente = request.Cliente,
                DireccionEntrega = request.DireccionEntrega,
                CentroId = request.CentroId,
                FechaPedido = DateTime.UtcNow,
                FechaEstimadaEntrega = request.FechaEstimadaEntrega,
                Estado = "Creado"
            };

            // Crear SLA
            var sla = new SLA
            {
                Id = Guid.NewGuid(),
                PedidoId = pedido.Id,
                TiempoLimite = request.FechaEstimadaEntrega,
                EstadoSLA = "EnRiesgo"
            };

            // Estado de entrega inicial
            var estadoInicial = new EntregaEstado
            {
                Id = Guid.NewGuid(),
                PedidoId = pedido.Id,
                Estado = "Creado",
                Descripcion = "Pedido registrado en el sistema.",
                FechaActualizacion = DateTime.UtcNow
            };

            _context.Pedidos.Add(pedido);
            _context.SLAs.Add(sla);
            _context.EntregaEstados.Add(estadoInicial);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            if (_mediator is not null)
            {
                await _mediator.Publish(new PedidoCreadoNotification(
                    pedido.Id,
                    pedido.Codigo,
                    pedido.Cliente,
                    pedido.CentroId,
                    pedido.FechaEstimadaEntrega));
            }

            return CreatedAtAction(nameof(GetPedidoDetalle), new { id = pedido.Id }, pedido);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Ocurrió un error al procesar la solicitud.", error = ex.Message });
        }
    }

    [HttpPut("{id}/asignar")]
    public async Task<IActionResult> AsignarPedido(Guid id, [FromBody] AsignarPedidoRequest request)
    {
        var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.Id == id);
        if (pedido == null)
        {
            return NotFound(new { message = "Pedido no encontrado." });
        }

        // Validar ruta
        var rutaExiste = await _context.Rutas.AnyAsync(r => r.Id == request.RutaId);
        if (!rutaExiste)
        {
            return BadRequest(new { message = "La ruta seleccionada no existe." });
        }

        // Validar transportista
        var transportista = await _userManager.FindByIdAsync(request.TransportistaId.ToString());
        if (transportista == null)
        {
            return BadRequest(new { message = "El transportista seleccionado no existe." });
        }

        // Actualizar datos del pedido
        pedido.RutaId = request.RutaId;
        pedido.TransportistaId = request.TransportistaId;
        pedido.Estado = "Asignado";

        // Registrar estado
        var userId = GetCurrentUserId();
        var nuevoEstado = new EntregaEstado
        {
            Id = Guid.NewGuid(),
            PedidoId = pedido.Id,
            Estado = "Asignado",
            Descripcion = $"Pedido asignado al transportista {transportista.NombreCompleto}.",
            FechaActualizacion = DateTime.UtcNow,
            ActualizadoPorId = userId
        };

        _context.EntregaEstados.Add(nuevoEstado);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Ruta y transportista asignados correctamente." });
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> ActualizarEstado(Guid id, [FromBody] ActualizarEstadoRequest request)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.SLA)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
        {
            return NotFound(new { message = "Pedido no encontrado." });
        }

        pedido.Estado = request.Estado;

        // Si se entrega, resolvemos el SLA
        if (request.Estado.Equals("Entregado", StringComparison.OrdinalIgnoreCase) && pedido.SLA != null)
        {
            pedido.SLA.FechaResolucion = DateTime.UtcNow;
            pedido.SLA.EstadoSLA = DateTime.UtcNow <= pedido.SLA.TiempoLimite ? "Cumplido" : "Incumplido";
        }
        else if (request.Estado.Equals("Cancelado", StringComparison.OrdinalIgnoreCase) && pedido.SLA != null)
        {
            pedido.SLA.EstadoSLA = "Cancelado";
        }

        // Registrar nuevo hito de rastreo
        var userId = GetCurrentUserId();
        var nuevoEstado = new EntregaEstado
        {
            Id = Guid.NewGuid(),
            PedidoId = pedido.Id,
            Estado = request.Estado,
            Descripcion = request.Descripcion,
            Latitud = request.Latitud,
            Longitud = request.Longitud,
            FechaActualizacion = DateTime.UtcNow,
            ActualizadoPorId = userId
        };

        _context.EntregaEstados.Add(nuevoEstado);
        await _context.SaveChangesAsync();

        // Verificar si hay alerta vencida al momento (opcional para SLAs activos en background)
        if (pedido.SLA != null && DateTime.UtcNow > pedido.SLA.TiempoLimite && pedido.SLA.EstadoSLA == "EnRiesgo")
        {
            pedido.SLA.EstadoSLA = "Incumplido";
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "Estado actualizado correctamente." });
    }

    [HttpGet("slas")]
    public async Task<IActionResult> GetSlas([FromQuery] Guid? centroId)
    {
        if (centroId == null || centroId == Guid.Empty)
        {
            return BadRequest(new { message = "El parámetro centroId es obligatorio." });
        }

        var query = _context.SLAs
            .Include(s => s.Pedido)
            .Where(s => s.Pedido != null && s.Pedido.CentroId == centroId);

        var slas = await query
            .Select(s => new
            {
                s.Id,
                s.PedidoId,
                PedidoCodigo = s.Pedido != null ? s.Pedido.Codigo : string.Empty,
                PedidoCliente = s.Pedido != null ? s.Pedido.Cliente : string.Empty,
                s.TiempoLimite,
                s.EstadoSLA,
                s.FechaResolucion
            })
            .ToListAsync();

        var total = slas.Count;
        var cumplidos = slas.Count(s => s.EstadoSLA.Equals("Cumplido", StringComparison.OrdinalIgnoreCase));
        var enRiesgo = slas.Count(s => s.EstadoSLA.Equals("EnRiesgo", StringComparison.OrdinalIgnoreCase));
        var incumplidos = slas.Count(s => s.EstadoSLA.Equals("Incumplido", StringComparison.OrdinalIgnoreCase));
        var cancelados = slas.Count(s => s.EstadoSLA.Equals("Cancelado", StringComparison.OrdinalIgnoreCase));

        return Ok(new
        {
            Metricas = new { Total = total, Cumplidos = cumplidos, EnRiesgo = enRiesgo, Incumplidos = incumplidos, Cancelados = cancelados },
            Detalle = slas
        });
    }

    [HttpGet("transportistas")]
    public async Task<IActionResult> GetTransportistas()
    {
        // Obtener usuarios con rol de Logistica
        var transportistas = await _userManager.GetUsersInRoleAsync("Logistica");
        
        var result = transportistas.Select(u => new
        {
            u.Id,
            u.NombreCompleto,
            u.Email
        }).ToList();

        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value 
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (Guid.TryParse(userIdClaim, out var guid))
        {
            return guid;
        }
        return null;
    }
}

public class CreatePedidoRequest
{
    public string Codigo { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string DireccionEntrega { get; set; } = string.Empty;
    public DateTime FechaEstimadaEntrega { get; set; }
    public Guid CentroId { get; set; }
}

public class AsignarPedidoRequest
{
    public Guid RutaId { get; set; }
    public Guid TransportistaId { get; set; }
}

public class ActualizarEstadoRequest
{
    public string Estado { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
}
