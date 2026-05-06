using BuildingBlocks.Common.Results;

namespace Explore.MessageCenterService.Application.Abstractions.External;

public interface IRecipientDirectoryClient
{
    Task<Result<RecipientProfileDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}

