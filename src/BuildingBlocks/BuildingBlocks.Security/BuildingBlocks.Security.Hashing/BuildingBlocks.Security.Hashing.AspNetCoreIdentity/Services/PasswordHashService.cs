using BuildingBlocks.Security.Hashing.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace BuildingBlocks.Security.Hashing.AspNetCoreIdentity.Services;

public sealed class PasswordHashService : IPasswordHashService
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(new object(), password);
    }

    public bool VerifyHashedPassword(string passwordHash, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(new object(), passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}

