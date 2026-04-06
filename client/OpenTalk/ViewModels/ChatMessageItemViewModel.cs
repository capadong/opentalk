using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using OpenTalk.Models;
using OpenTalk.Services;

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
    public string? CachedFilePath { get; init; }
    public DateTime CreatedAt { get; init; }

    // Image support
    public bool IsText => Type == 1;
    public bool IsImage => Type == 2;
    public bool IsFile => Type == 3;

    public Bitmap? ImageBitmap
    {
        get
        {
            if (!IsImage)
            {
                return null;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(CachedFilePath))
                {
                    return new Bitmap(CachedFilePath);
                }

                if (string.IsNullOrWhiteSpace(FileUrl))
                {
                    return null;
                }

                if (Uri.TryCreate(FileUrl, UriKind.Absolute, out var uri))
                {
                    if (uri.IsFile && !string.IsNullOrWhiteSpace(uri.LocalPath))
                    {
                        return new Bitmap(uri.LocalPath);
                    }

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

    public static ChatMessageItemViewModel FromMessage(ChatMessage message, long currentUserId, IEnumerable<GroupMember> members, string? cachedFilePath = null)
    {
        var member = members is null
            ? null
            : System.Linq.Enumerable.FirstOrDefault(members, m => m.UserId == message.SenderId);

        var label = member?.DisplayName ?? LocalizationService.Instance.Format("Main.UserLabel", message.SenderId);

        return new ChatMessageItemViewModel
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = label,
            IsSelf = message.SenderId == currentUserId,
            Type = message.Type,
            Content = message.Content,
            FileUrl = message.FileUrl,
            CachedFilePath = cachedFilePath,
            CreatedAt = message.CreatedAt
        };
    }
}
