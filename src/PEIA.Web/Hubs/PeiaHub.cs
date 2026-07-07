using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PEIA.Web.Hubs;

[Authorize]
public class PeiaHub : Hub
{
    public async Task JoinCentro(Guid centroId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetCentroGroup(centroId));
    }

    public async Task LeaveCentro(Guid centroId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetCentroGroup(centroId));
    }

    public static string GetCentroGroup(Guid centroId) => $"centro:{centroId:N}";
}
