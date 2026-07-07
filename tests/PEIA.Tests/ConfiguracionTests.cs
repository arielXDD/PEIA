using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEIA.Shared.Infra.Data;
using PEIA.Web.Controllers;

namespace PEIA.Tests;

public class ConfiguracionTests
{
    private static PeiaDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<PeiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PeiaDbContext(options);
    }

    [Fact]
    public async Task Empresa_ShouldPersistInSystemSettings()
    {
        using var context = GetContext();
        var controller = new ConfiguracionController(context);
        var request = new EmpresaConfig("PEIA", "PEIA123456ABC", "+52 55 0000 0000", "Ciudad de México", "admin@peia.com");

        var putResult = await controller.UpdateEmpresa(request);
        var getResult = await controller.GetEmpresa();

        Assert.IsType<OkObjectResult>(putResult);
        var ok = Assert.IsType<OkObjectResult>(getResult);
        var empresa = Assert.IsType<EmpresaConfig>(ok.Value);
        Assert.Equal("PEIA123456ABC", empresa.Rfc);
        Assert.True(await context.SystemSettings.AnyAsync(s => s.Key == "empresa"));
    }

    [Fact]
    public async Task Seguridad_ShouldRejectShortPasswordPolicy()
    {
        using var context = GetContext();
        var controller = new ConfiguracionController(context);

        var result = await controller.UpdateSeguridad(new SeguridadConfig(4, true, true));

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.False(await context.SystemSettings.AnyAsync(s => s.Key == "seguridad"));
    }
}
