using PEIA.Modules.Prediction.Models;

namespace PEIA.Modules.Prediction.Services;

public interface IPredictionService
{
    PrediccionDemandaDto PredictDemand(int days = 14);
    HistoricoPrediccionDto GetHistorico();
    ResumenPrediccionDto GetResumen(string periodo = "Próximos 14 días");
    List<ProductPredictionDto> GetProductPredictions();
}
