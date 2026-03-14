using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenTalk.Application.Services;
using OpenTalk.Domain.Entities;
using OpenTalk.Infrastructure.Repositories;

namespace OpenTalk.Pages.Admin.Users;

public class IndexModel(UserService userService, IUserRepository users) : PageModel
{
    [BindProperty]
    public string Username { get; set; } = "";
    [BindProperty]
    public string Password { get; set; } = "";
    [BindProperty]
    public string? Nickname { get; set; }
    public IEnumerable<User> Users { get; set; } = [];

    public async Task OnGet()
    {
        Users = await users.ListAsync(200, 0);
    }

    public async Task<IActionResult> OnPost()
    {
        await userService.RegisterAsync(Username, Password, Nickname);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDelete(long id)
    {
        await users.DeleteAsync(id);
        return new OkResult();
    }
}
