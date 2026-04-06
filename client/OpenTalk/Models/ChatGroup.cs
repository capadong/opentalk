using System;

namespace OpenTalk.Models;

public class ChatGroup
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }

    public override string ToString() => Name;
}
