using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OpenTalk.Pages.Admin;

public class LoginModel : PageModel
{
    [BindProperty]
    public string Username { get; set; } = "";
    [BindProperty]
    public string Password { get; set; } = "";
    public string? Error { get; set; }

    private readonly IConfiguration _configuration;

    public LoginModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void OnGet(string? returnUrl = null) { }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        var adminUser = _configuration["Admin:Username"] ?? "";
        var adminHash = _configuration["Admin:PasswordHash"] ?? "";

        if (string.IsNullOrWhiteSpace(adminUser) || string.IsNullOrWhiteSpace(adminHash))
        {
            Error = "管理员未配置";
            return Page();
        }

        if (Username == adminUser && Hash(Password).Equals(adminHash, StringComparison.OrdinalIgnoreCase))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, Username),
                new Claim(ClaimTypes.Role, "admin")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return Redirect(returnUrl ?? "/Admin");
        }

        Error = "用户名或密码错误";
        return Page();
    }

    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
