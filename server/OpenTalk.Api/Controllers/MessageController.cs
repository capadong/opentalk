using Microsoft.AspNetCore.Mvc;
using OpenTalk.Application.Services;

namespace OpenTalk.Controllers;

[ApiController]
[Route("api/v1/messages")]
public class MessageController(MessageService messages) : ControllerBase
{
    [HttpGet("{groupId:long}")]
    public async Task<IActionResult> Recent(long groupId, [FromQuery] int limit = 50)
    {
        var list = await messages.GetRecentAsync(groupId, limit);
        return Ok(list);
    }
}
