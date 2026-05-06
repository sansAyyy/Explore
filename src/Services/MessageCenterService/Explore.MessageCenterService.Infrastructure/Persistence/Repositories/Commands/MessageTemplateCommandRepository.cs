using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.MessageCenterService.Application.Abstractions.Persistence;
using Explore.MessageCenterService.Domain.MessageTemplates;
using Microsoft.EntityFrameworkCore;

namespace Explore.MessageCenterService.Infrastructure.Persistence.Repositories.Commands;

public sealed class MessageTemplateCommandRepository : IMessageTemplateCommandRepository, IScopeDependency
{
    private readonly MessageCenterDbContext _dbContext;

    public MessageTemplateCommandRepository(MessageCenterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MessageTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.MessageTemplates.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<MessageTemplate?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return _dbContext.MessageTemplates.SingleOrDefaultAsync(x => x.Code == code, cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(string code, Guid? excludedId, CancellationToken cancellationToken)
    {
        return _dbContext.MessageTemplates.AnyAsync(
            x => x.Code == code && (!excludedId.HasValue || x.Id != excludedId.Value),
            cancellationToken);
    }

    public Task AddAsync(MessageTemplate messageTemplate, CancellationToken cancellationToken)
    {
        return _dbContext.MessageTemplates.AddAsync(messageTemplate, cancellationToken).AsTask();
    }
}

