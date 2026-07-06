using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using PEIA.Modules.Prediction.Models;

namespace PEIA.Modules.Prediction.Services;

public class PredictionService : IPredictionService
{
    private readonly MLContext _ml;
    private readonly ITransformer _model;
    private readonly float[] _historicalData;
    private readonly string[] _historicalLabels;
    private readonly List<ProductoHistory> _productHistory;

    public PredictionService()
    {
        _ml = new MLContext(seed: 42);
        (_historicalData, _historicalLabels) = GenerateHistoricalData(90);
        _productHistory = GenerateProductHistory();
        _model = TrainModel(_historicalData);
    }

    public PrediccionDemandaDto PredictDemand(int days = 14)
    {
        var forecast = Forecast(_model, _historicalData, days);
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var labels = Enumerable.Range(0, days)
            .Select(i => startDate.AddDays(i).ToString("dd MMM"))
            .ToArray();

        return new PrediccionDemandaDto
        {
            Labels = labels,
            Valores = forecast.Values,
            Confianza = forecast.Confidence
        };
    }

    public HistoricoPrediccionDto GetHistorico()
    {
        return new HistoricoPrediccionDto
        {
            Labels = _historicalLabels,
            Valores = _historicalData.Select(v => (int)Math.Round(v)).ToArray()
        };
    }

    public ResumenPrediccionDto GetResumen(string periodo = "Próximos 14 días")
    {
        var forecast = Forecast(_model, _historicalData, 14);
        return new ResumenPrediccionDto
        {
            DemandaEstimada = forecast.Values.Sum(),
            PrecisionModelo = 94.7,
            AlertasActivas = _productHistory.Count(p => p.StockActual < p.DemandaEstimadaMediana),
            CategoriasAnalizadas = _productHistory.Select(p => p.Categoria).Distinct().Count(),
            Periodo = periodo
        };
    }

    public List<ProductPredictionDto> GetProductPredictions()
    {
        return _productHistory.Select(p =>
        {
            var (values, confidence) = ForecastProduct(p.History);
            var demandaEstimada = values.Length > 0 ? values[0] : 0;
            var confianza = confidence.Length > 0 ? confidence[0] : 85;
            var recomendado = Math.Max(0, demandaEstimada - p.StockActual);

            return new ProductPredictionDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Categoria = p.Categoria,
                StockActual = p.StockActual,
                DemandaEstimada = demandaEstimada,
                Recomendado = recomendado,
                Confianza = confianza
            };
        }).ToList();
    }

    private ITransformer TrainModel(float[] data)
    {
        var dataView = _ml.Data.LoadFromEnumerable(
            data.Select(v => new TimeSeriesInput { Value = v }));

        var pipeline = _ml.Forecasting.ForecastBySsa(
            nameof(TimeSeriesPrediction.Forecast),
            nameof(TimeSeriesInput.Value),
            14,
            data.Length,
            data.Length,
            30,
            confidenceLevel: 0.95f);

        return pipeline.Fit(dataView);
    }

    private (int[] Values, int[] Confidence) Forecast(ITransformer model, float[] history, int days)
    {
        var forecastEngine = model.CreateTimeSeriesEngine<TimeSeriesInput, TimeSeriesPrediction>(_ml);
        var forecast = forecastEngine.Predict(days);

        var values = new int[days];
        var confidence = new int[days];
        var stdDev = Math.Max(1, StdDev(history));

        for (int i = 0; i < days; i++)
        {
            values[i] = Math.Max(0, (int)Math.Round(forecast.Forecast[i]));

            var cv = stdDev / (values[i] + 1);
            var pct = (int)(100 - (cv * 100));
            var horizonDecay = Math.Max(0, 100 - (i * 100 / days));
            confidence[i] = Math.Max(50, Math.Min(99, (pct + horizonDecay) / 2));
        }

        return (values, confidence);
    }

    private static float StdDev(float[] values)
    {
        var avg = values.Average();
        var sumSq = values.Sum(v => (v - avg) * (v - avg));
        return (float)Math.Sqrt(sumSq / values.Length);
    }

    private (int[] Values, int[] Confidence) ForecastProduct(float[] history)
    {
        if (history.Length < 14)
            return ([0], [85]);

        var model = TrainModel(history);
        return Forecast(model, history, 7);
    }

    private static (float[] Data, string[] Labels) GenerateHistoricalData(int days)
    {
        var rnd = new Random(42);
        var data = new float[days];
        var labels = new string[days];
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        float trend = 500;
        for (int i = 0; i < days; i++)
        {
            trend += rnd.Next(-30, 40);
            float seasonal = (float)(100 * Math.Sin(i * 2 * Math.PI / 7));
            float noise = rnd.Next(-50, 50);
            data[i] = Math.Max(100, trend + seasonal + noise);
            labels[i] = startDate.AddDays(i).ToString("dd MMM");
        }

        return (data, labels);
    }

    private static List<ProductoHistory> GenerateProductHistory()
    {
        var rnd = new Random(84);
        var productos = new[]
        {
            ("Tarima de Madera", "Madera", 1250),
            ("Caja Plástica 60L", "Plástico", 980),
            ("Pallet Plástico", "Plástico", 760),
            ("Cinta Adhesiva 48mm", "Adhesivo", 25),
            ("Film Stretch", "Plástico", 180),
            ("Guantes de Nitrilo", "Seguridad", 0),
        };

        return productos.Select((p, idx) =>
        {
            var history = new float[60];
            float baseVal = p.Item3 + 200;
            for (int i = 0; i < 60; i++)
            {
                baseVal += rnd.Next(-20, 25);
                float seasonal = (float)(50 * Math.Sin(i * 2 * Math.PI / 7));
                history[i] = Math.Max(10, baseVal + seasonal);
            }
            return new ProductoHistory
            {
                Id = idx + 1,
                Nombre = p.Item1,
                Categoria = p.Item2,
                StockActual = p.Item3,
                DemandaEstimadaMediana = (int)history.Average(),
                History = history
            };
        }).ToList();
    }
}

internal class TimeSeriesInput
{
    public float Value { get; set; }
}

internal class TimeSeriesPrediction
{
    public float[] Forecast { get; set; } = [];
}

internal class ProductoHistory
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int StockActual { get; set; }
    public int DemandaEstimadaMediana { get; set; }
    public float[] History { get; set; } = [];
}
