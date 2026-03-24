using Microsoft.AspNetCore.SignalR;
using AspireApp.BedRock.SonetOps.MCP.Models;

namespace AspireApp.BedRock.SonetOps.MCP.Hubs;

public class ResponseHub : Hub
{
    public async Task SendResponse(UIResponse response)
    {
        await Clients.All.SendAsync("ReceiveResponse", response);
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}