namespace PEIA.Modules.Prediction.Models;

public class HistoricalDemand
{
    public DateTime Date { get; set; }
    public float Demand { get; set; }
}

public class ProductPredictionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int StockActual { get; set; }
    public int DemandaEstimada { get; set; }
    public int Recomendado { get; set; }
    public int Confianza { get; set; }
}

public class ResumenPrediccionDto
{
    public int DemandaEstimada { get; set; }
    public double PrecisionModelo { get; set; }
    public int AlertasActivas { get; set; }
    public int CategoriasAnalizadas { get; set; }
    public string Periodo { get; set; } = string.Empty;
}

public class HistoricoPrediccionDto
{
    public string[] Labels { get; set; } = [];
    public int[] Valores { get; set; } = [];
}

public class PrediccionDemandaDto
{
    public string[] Labels { get; set; } = [];
    public int[] Valores { get; set; } = [];
    public int[] Confianza { get; set; } = [];
}
