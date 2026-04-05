using System;
using System.Collections.Generic;
using OpenTalk.Models;

namespace OpenTalk.ViewModels;

public sealed class ChatMessageItemViewModel
{
    public long Id { get; init; }
    public long SenderId { get; init; }

    // Used by axaml bindings
    public string SenderName { get; init; } = string.Empty;
    public bool IsSelf { get; init; }
    public bool IsOther => !IsSelf;
    public string TimeText => CreatedAt.ToLocalTime().ToString("HH:mm");

    public int Type { get; init; }
    public string Content { get; init; } = string.Empty;
    public string? FileUrl { get; init; }
    public DateTime CreatedAt { get; init; }

    public static ChatMessageItemViewModel FromMessage(ChatMessage message, long currentUserId, IEnumerable<GroupMember> members)
    {
        var member = members is null
            ? null
            : System.Linq.Enumerable.FirstOrDefault(members, m => m.UserId == message.SenderId);

        var label = member?.DisplayName ?? $"用户 #{message.SenderId}";

        return new ChatMessageItemViewModel
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = label,
            IsSelf = message.SenderId == currentUserId,
            Type = message.Type,
            Content = message.Content,
            FileUrl = message.FileUrl,
            CreatedAt = message.CreatedAt
        };
    }
}
