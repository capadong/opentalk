using Microsoft.AspNetCore.Mvc;
using OpenTalk.Application.Services;

namespace OpenTalk.Controllers;

[ApiController]
[Route("api/v1/stats")]
public class StatsController(StatsService stats, ConnectionTracker tracker) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = new
        {
            onlineConnections = tracker.OnlineCount,
            todayMessages     = await stats.GetTodayMessageCountAsync(),
            userCount         = await stats.GetUserCountAsync(),
            groupCount        = await stats.GetGroupCountAsync()
        };
        return Ok(result);
    }
}
