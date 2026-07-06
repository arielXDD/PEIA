using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using PEIA.Modules.Logistics.Controllers;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Identity;
using PEIA.Shared.Infra.Logistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace PEIA.Tests;

public class LogisticsTests
{
    private PeiaDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<PeiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new PeiaDbContext(options);
    }

    private Mock<UserManager<Usuario>> GetMockUserManager()
    {
        var store = new Mock<IUserStore<Usuario>>();
        return new Mock<UserManager<Usuario>>(
            store.Object, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task CreatePedido_ShouldCreatePedidoAndSla_WhenDataIsValid()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var userManagerMock = GetMockUserManager();

        // Agregar bodega/centro dummy
        var centroId = Guid.NewGuid();
        context.Centros.Add(new Centro { Id = centroId, Nombre = "Bodega Norte", Codigo = "BN-01" });
        await context.SaveChangesAsync();

        var controller = new PedidosController(context, userManagerMock.Object);

        var request = new CreatePedidoRequest
        {
            Codigo = "PED-TEST-100",
            Cliente = "Cliente Test S.A.",
            DireccionEntrega = "Calle Falsa 123",
            CentroId = centroId,
            FechaEstimadaEntrega = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var result = await controller.CreatePedido(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var pedido = Assert.IsType<Pedido>(createdResult.Value);

        Assert.Equal("PED-TEST-100", pedido.Codigo);
        Assert.Equal("Creado", pedido.Estado);

        // Verificar que el SLA se creó
        var sla = await context.SLAs.FirstOrDefaultAsync(s => s.PedidoId == pedido.Id);
        Assert.NotNull(sla);
        Assert.Equal("EnRiesgo", sla.EstadoSLA);

        // Verificar que el estado inicial se creó
        var estado = await context.EntregaEstados.FirstOrDefaultAsync(e => e.PedidoId == pedido.Id);
        Assert.NotNull(estado);
        Assert.Equal("Creado", estado.Estado);
    }

    [Fact]
    public async Task AsignarPedido_ShouldUpdateRutaAndDriver_WhenValid()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var userManagerMock = GetMockUserManager();

        var centroId = Guid.NewGuid();
        var pedidoId = Guid.NewGuid();
        var rutaId = Guid.NewGuid();
        var transportistaId = Guid.NewGuid();

        context.Centros.Add(new Centro { Id = centroId, Nombre = "Bodega Norte", Codigo = "BN-01" });
        context.Rutas.Add(new Ruta { Id = rutaId, Nombre = "Ruta 1", Origen = "BN-01", Destino = "Sur", DistanciaKm = 50 });
        context.Pedidos.Add(new Pedido { Id = pedidoId, Codigo = "PED-101", Cliente = "Test", DireccionEntrega = "Dir", CentroId = centroId, Estado = "Creado" });
        await context.SaveChangesAsync();

        var transportista = new Usuario { Id = transportistaId, UserName = "chofer", NombreCompleto = "Juan Chofer" };
        userManagerMock.Setup(um => um.FindByIdAsync(transportistaId.ToString())).ReturnsAsync(transportista);

        var controller = new PedidosController(context, userManagerMock.Object);

        // Mock ClaimsPrincipal para simular usuario autenticado
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }, "mock"));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        var request = new AsignarPedidoRequest
        {
            RutaId = rutaId,
            TransportistaId = transportistaId
        };

        // Act
        var result = await controller.AsignarPedido(pedidoId, request);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        var dbPedido = await context.Pedidos.FindAsync(pedidoId);
        Assert.Equal("Asignado", dbPedido.Estado);
        Assert.Equal(rutaId, dbPedido.RutaId);
        Assert.Equal(transportistaId, dbPedido.TransportistaId);

        // Verificar hito de rastreo
        var tracking = await context.EntregaEstados.Where(e => e.PedidoId == pedidoId).ToListAsync();
        Assert.Contains(tracking, e => e.Estado == "Asignado" && e.Descripcion.Contains("Juan Chofer"));
    }

    [Fact]
    public async Task ActualizarEstado_ShouldSetSlaToCumplido_WhenDeliveredOnTime()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var userManagerMock = GetMockUserManager();

        var pedidoId = Guid.NewGuid();
        var pedido = new Pedido { Id = pedidoId, Codigo = "PED-200", Cliente = "Test", DireccionEntrega = "Dir", Estado = "EnRuta" };
        var sla = new SLA { Id = Guid.NewGuid(), PedidoId = pedidoId, TiempoLimite = DateTime.UtcNow.AddHours(2), EstadoSLA = "EnRiesgo" };
        
        context.Pedidos.Add(pedido);
        context.SLAs.Add(sla);
        await context.SaveChangesAsync();

        var controller = new PedidosController(context, userManagerMock.Object);

        // Mock ClaimsPrincipal para simular usuario autenticado
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }, "mock"));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        var request = new ActualizarEstadoRequest
        {
            Estado = "Entregado",
            Descripcion = "Paquete entregado a tiempo en puerta"
        };

        // Act
        var result = await controller.ActualizarEstado(pedidoId, request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var dbSla = await context.SLAs.FirstOrDefaultAsync(s => s.PedidoId == pedidoId);
        Assert.Equal("Cumplido", dbSla.EstadoSLA);
        Assert.NotNull(dbSla.FechaResolucion);
    }

    [Fact]
    public async Task ActualizarEstado_ShouldSetSlaToIncumplido_WhenDeliveredLate()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var userManagerMock = GetMockUserManager();

        var pedidoId = Guid.NewGuid();
        var pedido = new Pedido { Id = pedidoId, Codigo = "PED-300", Cliente = "Test", DireccionEntrega = "Dir", Estado = "EnRuta" };
        // SLA vencido hace 1 hora
        var sla = new SLA { Id = Guid.NewGuid(), PedidoId = pedidoId, TiempoLimite = DateTime.UtcNow.AddHours(-1), EstadoSLA = "EnRiesgo" };

        context.Pedidos.Add(pedido);
        context.SLAs.Add(sla);
        await context.SaveChangesAsync();

        var controller = new PedidosController(context, userManagerMock.Object);

        // Mock ClaimsPrincipal para simular usuario autenticado
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }, "mock"));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        var request = new ActualizarEstadoRequest
        {
            Estado = "Entregado",
            Descripcion = "Entregado tarde"
        };

        // Act
        var result = await controller.ActualizarEstado(pedidoId, request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var dbSla = await context.SLAs.FirstOrDefaultAsync(s => s.PedidoId == pedidoId);
        Assert.Equal("Incumplido", dbSla.EstadoSLA);
    }
}
