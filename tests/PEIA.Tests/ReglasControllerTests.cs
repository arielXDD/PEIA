using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEIA.Modules.Automation.Controllers;
using PEIA.Shared.Infra.Automation;
using PEIA.Shared.Infra.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PEIA.Tests;

public class ReglasControllerTests
{
    private static PeiaDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<PeiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PeiaDbContext(options);
    }

    [Fact]
    public async Task CreateRegla_ShouldPersistInDatabase()
    {
        using var context = GetContext();
        var controller = new ReglasController(context);
        var request = new ReglaRequest("Alerta de Stock Crítico", "stock_critico", "Todos", "notificar", "Supervisor", true);

        var result = await controller.CreateRegla(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var regla = Assert.IsType<ReglaAutomatizacion>(createdResult.Value);
        Assert.Equal("Alerta de Stock Crítico", regla.Nombre);
        Assert.True(await context.ReglasAutomatizacion.AnyAsync(r => r.Nombre == "Alerta de Stock Crítico"));
    }

    [Fact]
    public async Task GetReglas_ShouldReturnAllReglas()
    {
        using var context = GetContext();
        context.ReglasAutomatizacion.Add(new ReglaAutomatizacion
        {
            Id = Guid.NewGuid(),
            Nombre = "Regla 1",
            EventoOrigen = "nuevo_pedido",
            Condicion = "Todos",
            Accion = "notificar",
            Responsable = "Admin",
            Activa = true
        });
        await context.SaveChangesAsync();

        var controller = new ReglasController(context);
        var result = await controller.GetReglas();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<ReglaAutomatizacion>>(okResult.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task UpdateRegla_ShouldModifyExistingRegla()
    {
        using var context = GetContext();
        var id = Guid.NewGuid();
        var regla = new ReglaAutomatizacion
        {
            Id = id,
            Nombre = "Regla Original",
            EventoOrigen = "nuevo_pedido",
            Condicion = "Todos",
            Accion = "notificar",
            Responsable = "Admin",
            Activa = true
        };
        context.ReglasAutomatizacion.Add(regla);
        await context.SaveChangesAsync();

        var controller = new ReglasController(context);
        var updateRequest = new ReglaRequest("Regla Modificada", "nuevo_pedido", "Cliente=XYZ", "email", "Gerente", false);

        var result = await controller.UpdateRegla(id, updateRequest);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var updated = Assert.IsType<ReglaAutomatizacion>(okResult.Value);
        Assert.Equal("Regla Modificada", updated.Nombre);
        Assert.Equal("Cliente=XYZ", updated.Condicion);
        Assert.Equal("email", updated.Accion);
        Assert.False(updated.Activa);
    }

    [Fact]
    public async Task DeleteRegla_ShouldRemoveRegla()
    {
        using var context = GetContext();
        var id = Guid.NewGuid();
        var regla = new ReglaAutomatizacion
        {
            Id = id,
            Nombre = "Regla a borrar",
            EventoOrigen = "sla_vencido",
            Condicion = "Todos",
            Accion = "notificar",
            Responsable = "Admin",
            Activa = true
        };
        context.ReglasAutomatizacion.Add(regla);
        await context.SaveChangesAsync();

        var controller = new ReglasController(context);
        var result = await controller.DeleteRegla(id);

        Assert.IsType<OkObjectResult>(result);
        Assert.False(await context.ReglasAutomatizacion.AnyAsync(r => r.Id == id));
    }
}
