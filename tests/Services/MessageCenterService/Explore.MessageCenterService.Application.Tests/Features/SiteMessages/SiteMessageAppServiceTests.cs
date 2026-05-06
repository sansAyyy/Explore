using BuildingBlocks.Common.Pagination;
using Explore.MessageCenterService.Application.Abstractions.Persistence;
using Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Responses;
using Explore.MessageCenterService.Application.Features.SiteMessages.Services;
using Explore.MessageCenterService.Domain.SiteMessages;

namespace Explore.MessageCenterService.Application.Tests.Features.SiteMessages;

public sealed class SiteMessageAppServiceTests
{
    [Fact]
    public async Task GetPagedAsync_ShouldReturnOnlySpecifiedUserMessages()
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var anotherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var repository = new FakeSiteMessageCommandRepository(
        [
            SiteMessage.Create(Guid.NewGuid(), Guid.NewGuid(), userId, "A", "Content A"),
            SiteMessage.Create(Guid.NewGuid(), Guid.NewGuid(), anotherUserId, "B", "Content B")
        ]);
        var unitOfWork = new FakeMessageCenterUnitOfWork();
        var service = new SiteMessageAppService(repository, new FakeSiteMessageQueryRepository(repository), unitOfWork);

        var result = await service.GetPagedAsync(new GetPagedSiteMessagesRequest
        {
            UserId = userId,
            PageIndex = 1,
            PageSize = 10
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(userId, result.Value.Items.Single().UserId);
    }

    [Fact]
    public async Task MarkReadAsync_ShouldUpdateReadFlags()
    {
        var siteMessage = SiteMessage.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "A",
            "Content A");
        var repository = new FakeSiteMessageCommandRepository([siteMessage]);
        var unitOfWork = new FakeMessageCenterUnitOfWork();
        var service = new SiteMessageAppService(repository, new FakeSiteMessageQueryRepository(repository), unitOfWork);

        var result = await service.MarkReadAsync(siteMessage.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(siteMessage.IsRead);
        Assert.NotNull(siteMessage.ReadAt);
        Assert.Equal(1, unitOfWork.CommitCount);
    }

    private sealed class FakeSiteMessageCommandRepository : ISiteMessageCommandRepository
    {
        public FakeSiteMessageCommandRepository(IEnumerable<SiteMessage> messages)
        {
            Messages = messages.ToList();
        }

        public List<SiteMessage> Messages { get; }

        public Task<SiteMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Messages.SingleOrDefault(x => x.Id == id));
        }

        public Task<IReadOnlyCollection<SiteMessage>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<SiteMessage>>(Messages.Where(x => x.UserId == userId && !x.IsRead).ToList());
        }

        public Task AddAsync(SiteMessage siteMessage, CancellationToken cancellationToken)
        {
            Messages.Add(siteMessage);
            return Task.CompletedTask;
        }

    }

    private sealed class FakeSiteMessageQueryRepository : ISiteMessageQueryRepository
    {
        private readonly FakeSiteMessageCommandRepository _repository;

        public FakeSiteMessageQueryRepository(FakeSiteMessageCommandRepository repository)
        {
            _repository = repository;
        }

        public Task<PagedResult<SiteMessageBasicResponse>> GetPagedAsync(GetPagedSiteMessagesRequest request, CancellationToken cancellationToken)
        {
            var items = _repository.Messages
                .Where(x => x.UserId == request.UserId)
                .Select(x => new SiteMessageBasicResponse(
                    x.Id,
                    x.UserId,
                    x.Title,
                    x.Content,
                    x.IsRead,
                    x.CreatedAt,
                    x.ReadAt))
                .ToList();

            return Task.FromResult(new PagedResult<SiteMessageBasicResponse>(items.Count, items));
        }

        public Task<SiteMessageDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var siteMessage = _repository.Messages.SingleOrDefault(x => x.Id == id);
            return Task.FromResult(siteMessage is null
                ? null
                : new SiteMessageDetailResponse(
                    siteMessage.Id,
                    siteMessage.DispatchId,
                    siteMessage.UserId,
                    siteMessage.Title,
                    siteMessage.Content,
                    siteMessage.IsRead,
                    siteMessage.CreatedAt,
                    siteMessage.ReadAt));
        }
    }

    private sealed class FakeMessageCenterUnitOfWork : IMessageCenterUnitOfWork
    {
        public int CommitCount { get; private set; }

        public Task<int> CommitAsync(CancellationToken cancellationToken)
        {
            CommitCount++;
            return Task.FromResult(1);
        }
    }
}

