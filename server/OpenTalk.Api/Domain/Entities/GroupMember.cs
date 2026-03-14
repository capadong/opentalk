namespace OpenTalk.Domain.Entities;

public class GroupMember
{
    public long Id { get; set; }
    public long GroupId { get; set; }
    public long UserId { get; set; }
    public DateTime JoinedAt { get; set; }
}
