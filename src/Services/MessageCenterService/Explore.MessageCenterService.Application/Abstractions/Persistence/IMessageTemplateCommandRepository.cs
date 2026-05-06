using Explore.MessageCenterService.Domain.MessageTemplates;

namespace Explore.MessageCenterService.Application.Abstractions.Persistence;

public interface IMessageTemplateCommandRepository
{
    Task<MessageTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<MessageTemplate?> GetByCodeAsync(string code, CancellationToken cancellationToken);

    Task<bool> ExistsByCodeAsync(string code, Guid? excludedId, CancellationToken cancellationToken);

    Task AddAsync(MessageTemplate messageTemplate, CancellationToken cancellationToken);
}

