namespace OpenTalk.Domain.Entities;

public class ChatGroup
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public long OwnerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
