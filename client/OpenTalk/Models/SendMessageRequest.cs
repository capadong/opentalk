namespace OpenTalk.Models;

public class SendMessageRequest
{
    public long GroupId { get; set; }
    public long SenderId { get; set; }
    public int Type { get; set; } = 1;
    public string Content { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
}
