using System.Security.Cryptography;
using System.Text;
using OpenTalk.Domain.Entities;
using OpenTalk.Infrastructure.Repositories;

namespace OpenTalk.Application.Services;

public class UserService(IUserRepository users)
{
    public async Task<User> RegisterAsync(string username, string password, string? nickname = null)
    {
        var existing = await users.GetByUsernameAsync(username);
        if (existing is not null) throw new InvalidOperationException("Username exists");
        var hash = Hash(password);
        var user = new User { Username = username, PasswordHash = hash, Nickname = nickname, CreatedAt = DateTime.UtcNow };
        return await users.InsertAsync(user);
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await users.GetByUsernameAsync(username);
        if (user is null) return null;
        return user.PasswordHash == Hash(password) ? user : null;
    }

    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
