using Microsoft.AspNetCore.Mvc;
using OpenTalk.Application.Services;

namespace OpenTalk.Controllers;

[ApiController]
[Route("api/v1/groups")]
public class GroupController(GroupService groups) : ControllerBase
{
    [HttpGet("{userId:long}")]
    public async Task<IActionResult> List(long userId)
    {
        var list = await groups.ListAsync(userId);
        return Ok(list);
    }
}
