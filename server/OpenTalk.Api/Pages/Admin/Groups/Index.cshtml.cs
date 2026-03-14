using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenTalk.Domain.Entities;
using OpenTalk.Infrastructure.Repositories;

namespace OpenTalk.Pages.Admin.Groups;

public class IndexModel(IGroupRepository groups) : PageModel
{
    [BindProperty]
    public string Name { get; set; } = "";
    [BindProperty]
    public long OwnerId { get; set; }
    public IEnumerable<ChatGroup> Groups { get; set; } = [];
    public Dictionary<long, List<GroupMember>> Members { get; set; } = new();

    public async Task OnGet()
    {
        Groups = await groups.ListAsync(OwnerId == 0 ? 1 : OwnerId);
        Members = new Dictionary<long, List<GroupMember>>();
        foreach (var g in Groups)
        {
            var ms = await groups.ListMembersAsync(g.Id);
            Members[g.Id] = ms.ToList();
        }
    }

    public async Task<IActionResult> OnPost()
    {
        var g = new ChatGroup { Name = Name, OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        await groups.CreateAsync(g);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteGroup(long id)
    {
        await groups.DeleteAsync(id);
        return new OkResult();
    }

    public async Task<IActionResult> OnPostAddMember(long gid, long uid)
    {
        await groups.AddMemberAsync(gid, uid, DateTime.UtcNow);
        return new OkResult();
    }

    public async Task<IActionResult> OnPostRemoveMember(long gid, long uid)
    {
        await groups.RemoveMemberAsync(gid, uid);
        return new OkResult();
    }
}
