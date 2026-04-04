using System;

namespace OpenTalk.Models;

public class GroupMember
{
    public long UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public DateTime JoinedAt { get; set; }

    public string DisplayName => string.IsNullOrWhiteSpace(Nickname) ? Username : Nickname;
}
