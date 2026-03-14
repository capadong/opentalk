using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace OpenTalk.Controllers;

[ApiController]
[Route("api/v1/files")]
public class FileController(IConfiguration configuration, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("upload")]
    [RequestSizeLimit(104_857_600)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file");
        var root = configuration.GetSection("Storage")["UploadsRoot"] ?? "wwwroot/uploads";
        var now = DateTime.UtcNow;
        var relPath = $"{now:yyyy/MM}";
        var dir = Path.Combine(env.ContentRootPath, root, relPath);
        Directory.CreateDirectory(dir);
        var fname = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var full = Path.Combine(dir, fname);
        await using var fs = new FileStream(full, FileMode.Create);
        await file.CopyToAsync(fs);
        var url = $"/uploads/{relPath.Replace("\\", "/")}/{fname}";
        return Ok(new { url, size = file.Length, name = file.FileName });
    }
}
