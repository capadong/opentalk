using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
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

    // Image support
    public bool IsText => Type == 1;
    public bool IsImage => Type == 2;
    public bool IsFile => Type == 3;

    public Bitmap? ImageBitmap
    {
        get
        {
            if (!IsImage || string.IsNullOrWhiteSpace(FileUrl))
            {
                return null;
            }

            try
            {
                // Avalonia Bitmap can load from file path or stream.
                // Backend returns a URL; in this demo client we also support local paths.
                if (Uri.TryCreate(FileUrl, UriKind.Absolute, out var uri))
                {
                    if (uri.IsFile)
                    {
                        return new Bitmap(uri.LocalPath);
                    }

                    // For http(s) we fall back to showing the link; downloading is handled elsewhere.
                    return null;
                }

                return new Bitmap(FileUrl);
            }
            catch
            {
                return null;
            }
        }
    }

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
