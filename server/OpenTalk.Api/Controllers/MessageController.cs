using Microsoft.AspNetCore.Mvc;
using OpenTalk.Application.Services;

namespace OpenTalk.Controllers;

[ApiController]
[Route("api/v1/messages")]
public class MessageController(MessageService messages) : ControllerBase
{
    /// <summary>获取群组消息，支持分页（beforeId 向前翻页）</summary>
    [HttpGet("{groupId:long}")]
    public async Task<IActionResult> Recent(
        long groupId,
        [FromQuery] int limit = 50,
        [FromQuery] long? beforeId = null)
    {
        var list = await messages.GetRecentAsync(groupId, limit, beforeId);
        return Ok(list);
    }

    /// <summary>获取私信，支持分页（beforeId 向前翻页）</summary>
    [HttpGet("direct/{userId:long}/{peerId:long}")]
    public async Task<IActionResult> Direct(
        long userId,
        long peerId,
        [FromQuery] int limit = 50,
        [FromQuery] long? beforeId = null)
    {
        var list = await messages.GetDirectRecentAsync(userId, peerId, limit, beforeId);
        return Ok(list);
    }
}
