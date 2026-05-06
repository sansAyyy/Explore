using BuildingBlocks.Security.Authentication.Models;

namespace BuildingBlocks.Security.Authentication.Abstractions;

public interface IJwtTokenService
{
    AccessTokenResult CreateAccessToken(string subjectId, string userName, string tokenParty);
}

