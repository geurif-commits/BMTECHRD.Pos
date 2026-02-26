namespace BMTECHRD.Pos.Application.Abstractions.Security;
public interface IPasswordHasher
{
    string Hash(string plain);
    bool Verify(string plain, string hash);
}