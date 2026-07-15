using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEIA.Modules.ERP.Controllers;
using PEIA.Modules.ERP.Handlers;
using PEIA.Shared.Infra.Data;
using PEIA.Shared.Infra.Identity;

namespace PEIA.Tests;

public class ErpTests
{
    private static PeiaDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<PeiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PeiaDbContext(options);
    }

    [Fact]
    public async Task CreateCentro_ShouldRejectDuplicateCodigo()
    {
        using var context = GetContext();
        context.Centros.Add(new Centro { Id = Guid.NewGuid(), Nombre = "Bodega Norte", Codigo = "BN-01" });
        await context.SaveChangesAsync();

        var handler = new CreateCentroCommandHandler(context);

        var result = await handler.Handle(new CreateCentroCommand(new CentroRequest("Otra Bodega", "bn-01", "Calle 1")), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("Ya existe", result.Error);
        Assert.Equal(1, await context.Centros.CountAsync());
    }

    [Fact]
    public async Task DeleteCentro_ShouldSoftDelete()
    {
        using var context = GetContext();
        var centroId = Guid.NewGuid();
        context.Centros.Add(new Centro { Id = centroId, Nombre = "Bodega Norte", Codigo = "BN-01", Activo = true });
        await context.SaveChangesAsync();

        var handler = new DeleteCentroCommandHandler(context);

        var result = await handler.Handle(new DeleteCentroCommand(centroId), CancellationToken.None);

        Assert.True(result.Success);
        Assert.False((await context.Centros.FindAsync(centroId))!.Activo);
    }
}
