namespace OpenTalk.Application.DTOs;

public enum MessageType
{
    Text = 1,
    Image = 2,
    File = 3
}

public class SendMessageDto
{
    public long GroupId { get; set; }
    public long SenderId { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public string Content { get; set; } = "";
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
