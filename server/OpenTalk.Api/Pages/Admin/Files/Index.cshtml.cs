using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenTalk.Application.Storage;
using OpenTalk.Domain.Entities;
using OpenTalk.Infrastructure.Repositories;

namespace OpenTalk.Pages.Admin.Files;

public class IndexModel(
    IFileRepository fileRepo,
    IStorageProvider storage) : PageModel
{
    public IEnumerable<FileRecord> Files { get; set; } = [];
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 20;
    public bool HasMore { get; set; }

    public async Task OnGetAsync([FromQuery] int page = 0)
    {
        PageIndex = page;
        Files = await fileRepo.ListAsync(PageSize + 1, page * PageSize);
        HasMore = Files.Count() > PageSize;
        Files = Files.Take(PageSize);
    }

    public async Task<IActionResult> OnPostDeleteAsync(long id)
    {
        var record = await fileRepo.GetByIdAsync(id);
        if (record != null)
        {
            await storage.DeleteAsync(record.FilePath);
            await fileRepo.DeleteAsync(id);
        }
        return RedirectToPage();
    }
}
