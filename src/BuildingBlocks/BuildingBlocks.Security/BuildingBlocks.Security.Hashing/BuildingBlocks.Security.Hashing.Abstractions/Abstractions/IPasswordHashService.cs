namespace BuildingBlocks.Security.Hashing.Abstractions;

public interface IPasswordHashService
{
    string HashPassword(string password);

    bool VerifyHashedPassword(string passwordHash, string password);
}

