using System;

namespace OpenTalk.Models;

public class ChatMessage
{
    public long Id { get; set; }
    public long? GroupId { get; set; }
    public long SenderId { get; set; }
    public long? ReceiverId { get; set; }
    public int Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
