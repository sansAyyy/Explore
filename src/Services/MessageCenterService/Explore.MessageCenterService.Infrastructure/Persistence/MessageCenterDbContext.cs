using BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Extensions;
using BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Models;
using BuildingBlocks.Persistence.Auditing.Abstractions;
using BuildingBlocks.Persistence.Auditing.EntityFrameworkCore.Auditing;
using Explore.MessageCenterService.Domain.MessageTemplates;
using Explore.MessageCenterService.Domain.NotificationDispatches;
using Explore.MessageCenterService.Domain.SiteMessages;
using Microsoft.EntityFrameworkCore;

namespace Explore.MessageCenterService.Infrastructure.Persistence;

public sealed class MessageCenterDbContext : AuditableDbContext
{
    public MessageCenterDbContext(
        DbContextOptions<MessageCenterDbContext> options,
        IAuditActorAccessor auditActorAccessor)
        : base(options, auditActorAccessor)
    {
    }

    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();

    public DbSet<NotificationDispatch> NotificationDispatches => Set<NotificationDispatch>();

    public DbSet<SiteMessage> SiteMessages => Set<SiteMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInboxMessageEntity();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MessageCenterDbContext).Assembly);
    }
}



