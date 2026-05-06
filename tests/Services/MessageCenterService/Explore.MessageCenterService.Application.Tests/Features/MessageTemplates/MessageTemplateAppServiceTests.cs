using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using Explore.MessageCenterService.Application.Abstractions.Persistence;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Responses;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Services;
using Explore.MessageCenterService.Domain.MessageTemplates;

namespace Explore.MessageCenterService.Application.Tests.Features.MessageTemplates;

public sealed class MessageTemplateAppServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateTemplate_WhenRequestIsValid()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.CreateAsync(new CreateMessageTemplateRequest
        {
            Code = "order.pay_success.sms",
            Name = "Order Pay Success SMS",
            Description = "Notify payment success by sms",
            ChannelType = NotificationChannelType.Sms,
            BodyTemplate = "Order {{orderNo}} paid successfully."
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("order.pay_success.sms", result.Value!.Code);
        Assert.Equal(NotificationChannelType.Sms, result.Value.ChannelType);
        Assert.Single(fixture.CommandRepository.Templates);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnValidation_WhenChannelTypeIsInvalid()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.CreateAsync(new CreateMessageTemplateRequest
        {
            Code = "order.pay_success.sms",
            Name = "Order Pay Success SMS",
            ChannelType = 0,
            BodyTemplate = "Body"
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.ValidationFailed, result.Error.Code);
    }

    private static Fixture CreateFixture()
    {
        var commandRepository = new FakeMessageTemplateCommandRepository();
        var queryRepository = new FakeMessageTemplateQueryRepository(commandRepository);
        var unitOfWork = new FakeMessageCenterUnitOfWork();
        var service = new MessageTemplateAppService(commandRepository, queryRepository, unitOfWork);
        return new Fixture(service, commandRepository, unitOfWork);
    }

    private sealed record Fixture(
        MessageTemplateAppService Service,
        FakeMessageTemplateCommandRepository CommandRepository,
        FakeMessageCenterUnitOfWork UnitOfWork);

    private sealed class FakeMessageTemplateCommandRepository : IMessageTemplateCommandRepository
    {
        private readonly List<MessageTemplate> _templates = [];

        public IReadOnlyList<MessageTemplate> Templates => _templates;

        public Task<MessageTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_templates.SingleOrDefault(x => x.Id == id));
        }

        public Task<MessageTemplate?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        {
            return Task.FromResult(_templates.SingleOrDefault(x => x.Code == code));
        }

        public Task<bool> ExistsByCodeAsync(string code, Guid? excludedId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_templates.Any(x => x.Code == code && (!excludedId.HasValue || x.Id != excludedId.Value)));
        }

        public Task AddAsync(MessageTemplate messageTemplate, CancellationToken cancellationToken)
        {
            _templates.Add(messageTemplate);
            return Task.CompletedTask;
        }

    }

    private sealed class FakeMessageTemplateQueryRepository : IMessageTemplateQueryRepository
    {
        private readonly FakeMessageTemplateCommandRepository _commandRepository;

        public FakeMessageTemplateQueryRepository(FakeMessageTemplateCommandRepository commandRepository)
        {
            _commandRepository = commandRepository;
        }

        public Task<PagedResult<MessageTemplateBasicResponse>> GetPagedAsync(GetPagedMessageTemplatesRequest request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<MessageTemplateDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var template = _commandRepository.Templates.SingleOrDefault(x => x.Id == id);
            if (template is null)
            {
                return Task.FromResult<MessageTemplateDetailResponse?>(null);
            }

            return Task.FromResult<MessageTemplateDetailResponse?>(new MessageTemplateDetailResponse(
                template.Id,
                template.Code,
                template.Name,
                template.Description,
                template.IsEnabled,
                template.ChannelType,
                template.TitleTemplate,
                template.BodyTemplate));
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

