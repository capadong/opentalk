using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenTalk.Application.Services;

namespace OpenTalk.Pages.Admin;

public class DashboardModel(StatsService stats, ConnectionTracker tracker) : PageModel
{
    public int OnlineConnections { get; set; }
    public long TodayMessages { get; set; }
    public int UserCount { get; set; }
    public int GroupCount { get; set; }

    public async Task OnGet()
    {
        OnlineConnections = tracker.OnlineCount;
        TodayMessages = await stats.GetTodayMessageCountAsync();
        UserCount = await stats.GetUserCountAsync();
        GroupCount = await stats.GetGroupCountAsync();
    }
}
