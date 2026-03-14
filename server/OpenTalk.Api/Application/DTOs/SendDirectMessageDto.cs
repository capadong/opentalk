namespace OpenTalk.Application.DTOs;

public class SendDirectMessageDto
{
    public long SenderId { get; set; }
    public long ReceiverId { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public string Content { get; set; } = "";
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

