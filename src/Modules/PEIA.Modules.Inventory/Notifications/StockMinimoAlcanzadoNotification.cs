using MediatR;

namespace PEIA.Modules.Inventory.Notifications;

public record StockMinimoAlcanzadoNotification(
    Guid ProductoId,
    string Sku,
    string Producto,
    Guid CentroId,
    int StockActual,
    int StockMinimo) : INotification;
