using OpenTalk.Application.DTOs;

namespace OpenTalk.Domain.Entities;

public class ChatMessage
{
    public long Id { get; set; }
    public long GroupId { get; set; }
    public long SenderId { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public string Content { get; set; } = "";
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
