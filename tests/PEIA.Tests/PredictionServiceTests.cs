using Microsoft.EntityFrameworkCore;
using PEIA.Modules.Prediction.Services;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Identity;
using PEIA.Shared.Infra.Inventory;

namespace PEIA.Tests;

public class PredictionServiceTests
{
    private static PeiaDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<PeiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PeiaDbContext(options);
    }

    [Fact]
    public async Task GetHistorico_ShouldUseRealSalidaMovements_WhenEnoughDataExists()
    {
        using var context = GetContext();
        var centroId = Guid.NewGuid();
        var categoriaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();

        context.Centros.Add(new Centro { Id = centroId, Nombre = "Bodega Norte", Codigo = "BN-01" });
        context.Categorias.Add(new Categoria { Id = categoriaId, Nombre = "Consumibles" });
        context.Productos.Add(new Producto { Id = productoId, Sku = "SKU-1", Nombre = "Cinta", CategoriaId = categoriaId });
        context.Stocks.Add(new Stock { Id = Guid.NewGuid(), ProductoId = productoId, CentroId = centroId, Cantidad = 100 });

        var start = DateTime.UtcNow.Date.AddDays(-20);
        for (var i = 0; i < 20; i++)
        {
            context.Movimientos.Add(new Movimiento
            {
                Id = Guid.NewGuid(),
                Tipo = "Salida",
                Cantidad = i + 1,
                StockAnterior = 100 - i,
                StockNuevo = 99 - i,
                ProductoId = productoId,
                CentroId = centroId,
                FechaMovimiento = start.AddDays(i)
            });
        }

        await context.SaveChangesAsync();

        var service = new PredictionService(context);
        var historico = service.GetHistorico();

        Assert.Equal(90, historico.Valores.Length);
        Assert.Contains(20, historico.Valores);
        Assert.Contains(1, historico.Valores);
    }
}
