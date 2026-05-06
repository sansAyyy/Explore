using BuildingBlocks.Common.Results;

namespace Explore.AdminIdentityService.Application.Abstractions.Notifications;

public interface IAdminSiteMessageSender
{
    Task<Result> SendAsync(AdminSiteMessageRequest request, CancellationToken cancellationToken);
}

