using BuildingBlocks.CurrentUser.Abstractions;
using BuildingBlocks.CurrentUser.AspNetCore.Claims.Extensions;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.CurrentUser.AspNetCore.Services
{
    public class CurrentUser : ICurrentUser
    {
        private const string SystemActor = "System";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId => _httpContextAccessor.HttpContext?.User.GetSubjectId();

        public string AuditActor => UserId?.ToString() ?? SystemActor;
    }
}

