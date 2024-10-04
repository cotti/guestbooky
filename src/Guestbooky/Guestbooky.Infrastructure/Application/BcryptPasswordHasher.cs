using Guestbooky.Application.Interfaces;

namespace Guestbooky.Infrastructure.Application;

/// <summary>
/// Modern BCrypt is so <c>hard</c> to use. I love it.
/// Weird edge cases, though. Keep your stuff under 72 characters. It'll be fine.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
    }
}