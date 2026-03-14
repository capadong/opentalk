using Microsoft.AspNetCore.SignalR;
using OpenTalk.Application.Services;
using OpenTalk.Application.DTOs;

namespace OpenTalk.Hubs;

public class ChatHub(MessageService messageService, GroupService groupService) : Hub
{
    public async Task Register(long userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
    }

    public async Task JoinGroup(long groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
    }

    public async Task LeaveGroup(long groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
    }

    public async Task SendMessage(SendMessageDto dto)
    {
        var saved = await messageService.SaveMessageAsync(dto);
        await Clients.Group(dto.GroupId.ToString()).SendAsync("ReceiveMessage", saved);
    }

    public async Task SendDirectMessage(SendDirectMessageDto dto)
    {
        var saved = await messageService.SaveDirectMessageAsync(dto);
        await Clients.Group($"user:{dto.SenderId}").SendAsync("ReceiveDirectMessage", saved);
        await Clients.Group($"user:{dto.ReceiverId}").SendAsync("ReceiveDirectMessage", saved);
    }
}
