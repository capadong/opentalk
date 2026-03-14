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
    [BindProperty]
    public string? Avatar { get; set; }
    public IEnumerable<User> Users { get; set; } = [];

    public async Task OnGet()
    {
        Users = await users.ListAsync(200, 0);
    }

    public async Task<IActionResult> OnPost()
    {
        var user = await userService.RegisterAsync(Username, Password, Nickname);
        if (!string.IsNullOrEmpty(Avatar))
        {
            user.Avatar = Avatar;
            await users.UpdateAsync(user);
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDelete(long id)
    {
        await users.DeleteAsync(id);
        return new OkResult();
    }

    public async Task<IActionResult> OnPostEdit(long id, string username, string? nickname, string? avatar)
    {
        var user = new User
        {
            Id = id,
            Username = username,
            Nickname = nickname,
            Avatar = avatar
        };
        await users.UpdateAsync(user);
        return RedirectToPage();
    }
}
