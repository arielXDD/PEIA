using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PEIA.Modules.Inventory.Controllers;
using PEIA.Modules.Inventory.Notifications;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Identity;
using PEIA.Shared.Infra.Inventory;

namespace PEIA.Tests;

public class InventoryTests
{
    private static PeiaDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<PeiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PeiaDbContext(options);
    }

    [Fact]
    public async Task RegistrarMovimiento_ShouldRejectSalida_WhenStockWouldBeNegative()
    {
        using var context = GetContext();
        var mediator = new Mock<IMediator>();
        var centroId = Guid.NewGuid();
        var categoriaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();

        context.Centros.Add(new Centro { Id = centroId, Nombre = "Bodega Norte", Codigo = "BN-01" });
        context.Categorias.Add(new Categoria { Id = categoriaId, Nombre = "Consumibles" });
        context.Productos.Add(new Producto
        {
            Id = productoId,
            Sku = "SKU-001",
            Nombre = "Cinta",
            CategoriaId = categoriaId,
            StockMinimo = 5
        });
        context.Stocks.Add(new Stock { Id = Guid.NewGuid(), ProductoId = productoId, CentroId = centroId, Cantidad = 3 });
        await context.SaveChangesAsync();

        var controller = new InventarioController(context, mediator.Object);

        var result = await controller.RegistrarMovimiento(new MovimientoRequest(
            productoId,
            centroId,
            "Salida",
            4,
            null,
            "Venta",
            null,
            null,
            null));

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(context.Movimientos);
        Assert.Equal(3, (await context.Stocks.FirstAsync()).Cantidad);
    }

    [Fact]
    public async Task RegistrarMovimiento_ShouldPublishStockMinimo_WhenStockFallsBelowMinimum()
    {
        using var context = GetContext();
        var mediator = new Mock<IMediator>();
        var centroId = Guid.NewGuid();
        var categoriaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();

        context.Centros.Add(new Centro { Id = centroId, Nombre = "Bodega Norte", Codigo = "BN-01" });
        context.Categorias.Add(new Categoria { Id = categoriaId, Nombre = "Consumibles" });
        context.Productos.Add(new Producto
        {
            Id = productoId,
            Sku = "SKU-002",
            Nombre = "Guantes",
            CategoriaId = categoriaId,
            StockMinimo = 10
        });
        context.Stocks.Add(new Stock { Id = Guid.NewGuid(), ProductoId = productoId, CentroId = centroId, Cantidad = 12 });
        await context.SaveChangesAsync();

        var controller = new InventarioController(context, mediator.Object);

        var result = await controller.RegistrarMovimiento(new MovimientoRequest(
            productoId,
            centroId,
            "Salida",
            3,
            "A-01",
            "Pedido",
            "PED-1",
            null,
            null));

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(9, (await context.Stocks.FirstAsync()).Cantidad);
        mediator.Verify(m => m.Publish(
            It.Is<StockMinimoAlcanzadoNotification>(n =>
                n.ProductoId == productoId &&
                n.CentroId == centroId &&
                n.StockActual == 9 &&
                n.StockMinimo == 10),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
