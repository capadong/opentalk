namespace OpenTalk.Models;

public class SendDirectMessageRequest
{
    public long SenderId { get; set; }
    public long ReceiverId { get; set; }
    public int Type { get; set; } = 1;
    public string Content { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
}
