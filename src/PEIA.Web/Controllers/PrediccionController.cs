using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PEIA.Modules.Prediction.Services;

namespace PEIA.Web.Controllers;

[ApiController]
[Route("api/predicciones")]
public class PrediccionController : ControllerBase
{
    private readonly IPredictionService _predictionService;

    public PrediccionController(IPredictionService predictionService)
    {
        _predictionService = predictionService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_predictionService.GetProductPredictions());
    }

    [AllowAnonymous]
    [HttpGet("resumen")]
    public IActionResult Resumen()
    {
        var resumen = _predictionService.GetResumen();
        return Ok(new
        {
            demandaEstimada = resumen.DemandaEstimada,
            precisionModelo = resumen.PrecisionModelo,
            alertasActivas = resumen.AlertasActivas,
            categoriasAnalizadas = resumen.CategoriasAnalizadas,
            periodo = resumen.Periodo
        });
    }

    [AllowAnonymous]
    [HttpGet("historico")]
    public IActionResult Historico()
    {
        var historico = _predictionService.GetHistorico();
        return Ok(new { labels = historico.Labels, valores = historico.Valores });
    }

    [AllowAnonymous]
    [HttpGet("prediccion")]
    public IActionResult Prediccion([FromQuery] int dias = 14)
    {
        var prediccion = _predictionService.PredictDemand(dias);
        return Ok(new { labels = prediccion.Labels, valores = prediccion.Valores, confianza = prediccion.Confianza });
    }
}
