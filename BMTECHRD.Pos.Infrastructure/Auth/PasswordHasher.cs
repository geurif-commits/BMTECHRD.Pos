using BMTECHRD.Pos.Application.Abstractions.Security;
using BCrypt.Net;

namespace BMTECHRD.Pos.Infrastructure.Auth;
public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string plain) => BCrypt.Net.BCrypt.HashPassword(plain);
    public bool Verify(string plain, string hash) => BCrypt.Net.BCrypt.Verify(plain, hash);
}
