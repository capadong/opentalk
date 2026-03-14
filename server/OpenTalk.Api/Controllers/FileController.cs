using Microsoft.AspNetCore.Mvc;
using OpenTalk.Application.Storage;
using OpenTalk.Domain.Entities;
using OpenTalk.Infrastructure.Repositories;

namespace OpenTalk.Controllers;

[ApiController]
[Route("api/v1/files")]
public class FileController(
    IStorageProvider storage,
    IFileRepository fileRepo) : ControllerBase
{
    [HttpPost("upload")]
    [RequestSizeLimit(104_857_600)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] long? uploaderId)
    {
        if (file == null || file.Length == 0) return BadRequest("No file");

        await using var stream = file.OpenReadStream();
        var url = await storage.UploadAsync(stream, file.FileName, file.ContentType);

        var record = new FileRecord
        {
            FileName = file.FileName,
            FilePath = url,
            FileType = file.ContentType,
            Size = file.Length,
            UploaderId = uploaderId,
            CreatedAt = DateTime.UtcNow
        };
        await fileRepo.InsertAsync(record);

        return Ok(new { url, size = file.Length, name = file.FileName, id = record.Id });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var record = await fileRepo.GetByIdAsync(id);
        if (record == null) return NotFound();
        await storage.DeleteAsync(record.FilePath);
        await fileRepo.DeleteAsync(id);
        return Ok(new { deleted = true });
    }
}
