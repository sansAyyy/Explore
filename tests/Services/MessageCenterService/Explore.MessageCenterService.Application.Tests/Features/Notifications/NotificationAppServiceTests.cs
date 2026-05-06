using BuildingBlocks.Common.Results;
using Explore.MessageCenterService.Application.Abstractions.External;
using Explore.MessageCenterService.Application.Abstractions.Notifications;
using Explore.MessageCenterService.Application.Abstractions.Persistence;
using Explore.MessageCenterService.Application.Features.Notifications.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.Notifications.Services;
using Explore.MessageCenterService.Domain.MessageTemplates;
using Explore.MessageCenterService.Domain.NotificationDispatches;
using Explore.MessageCenterService.Domain.SiteMessages;

namespace Explore.MessageCenterService.Application.Tests.Features.Notifications;

public sealed class NotificationAppServiceTests
{
    [Fact]
    public async Task SendByTemplateAsync_ShouldSendSmsWithoutRecipientDirectory_WhenPhoneNumberIsProvided()
    {
        var fixture = CreateFixture(NotificationChannelType.Sms);

        var result = await fixture.Service.SendByTemplateAsync(new SendNotificationRequest
        {
            TemplateCode = fixture.Template.Code,
            Channel = NotificationChannelType.Sms,
            Recipient = new NotificationRecipientRequest
            {
                PhoneNumber = "13900139000"
            },
            Parameters = new Dictionary<string, string>
            {
                ["orderNo"] = "SO001",
                ["amount"] = "88.00"
            }
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(NotificationDispatchStatus.Placeholder, result.Value!.Dispatch.Status);
        Assert.Equal(0, fixture.DirectoryClient.CallCount);
        Assert.Equal("13900139000", fixture.DispatchRepository.Dispatches.Single().RecipientAddressSnapshot);
        Assert.Null(fixture.DispatchRepository.Dispatches.Single().RecipientUserId);
    }

    [Fact]
    public async Task SendByTemplateAsync_ShouldUseRecipientDirectory_WhenSmsOnlyHasUserId()
    {
        var fixture = CreateFixture(NotificationChannelType.Sms);

        var result = await fixture.Service.SendByTemplateAsync(new SendNotificationRequest
        {
            TemplateCode = fixture.Template.Code,
            Channel = NotificationChannelType.Sms,
            Recipient = new NotificationRecipientRequest
            {
                UserId = fixture.Profile.UserId
            },
            Parameters = new Dictionary<string, string>
            {
                ["orderNo"] = "SO001",
                ["amount"] = "88.00"
            }
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, fixture.DirectoryClient.CallCount);
        Assert.Equal(fixture.Profile.PhoneNumber, fixture.DispatchRepository.Dispatches.Single().RecipientAddressSnapshot);
    }

    [Fact]
    public async Task SendByTemplateAsync_ShouldSendSiteMessage_WhenUserIdIsProvided()
    {
        var fixture = CreateFixture(NotificationChannelType.SiteMessage);

        var result = await fixture.Service.SendByTemplateAsync(new SendNotificationRequest
        {
            TemplateCode = fixture.Template.Code,
            Channel = NotificationChannelType.SiteMessage,
            Recipient = new NotificationRecipientRequest
            {
                UserId = fixture.Profile.UserId
            },
            Parameters = new Dictionary<string, string>
            {
                ["orderNo"] = "SO001",
                ["amount"] = "88.00"
            }
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(NotificationDispatchStatus.Delivered, result.Value!.Dispatch.Status);
        Assert.Single(fixture.SiteMessageRepository.Messages);
        Assert.Equal(fixture.Profile.UserId, fixture.SiteMessageRepository.Messages.Single().UserId);
    }

    [Fact]
    public async Task SendByTemplateAsync_ShouldReturnValidation_WhenChannelDoesNotMatchTemplate()
    {
        var fixture = CreateFixture(NotificationChannelType.SiteMessage);

        var result = await fixture.Service.SendByTemplateAsync(new SendNotificationRequest
        {
            TemplateCode = fixture.Template.Code,
            Channel = NotificationChannelType.Sms,
            RecipientUserId = fixture.Profile.UserId,
            Parameters = new Dictionary<string, string>
            {
                ["orderNo"] = "SO001",
                ["amount"] = "88.00"
            }
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.ValidationFailed, result.Error.Code);
    }

    [Fact]
    public async Task SendByTemplateAsync_ShouldReturnValidation_WhenSiteMessageRecipientHasNoUserId()
    {
        var fixture = CreateFixture(NotificationChannelType.SiteMessage);

        var result = await fixture.Service.SendByTemplateAsync(new SendNotificationRequest
        {
            TemplateCode = fixture.Template.Code,
            Channel = NotificationChannelType.SiteMessage,
            Recipient = new NotificationRecipientRequest
            {
                PhoneNumber = "13900139000"
            },
            Parameters = new Dictionary<string, string>
            {
                ["orderNo"] = "SO001",
                ["amount"] = "88.00"
            }
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.ValidationFailed, result.Error.Code);
    }

    [Fact]
    public async Task SendByTemplateAsync_ShouldMarkSmsAsFailed_WhenDirectoryHasNoPhone()
    {
        var fixture = CreateFixture(
            NotificationChannelType.Sms,
            profile: new RecipientProfileDto(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                null,
                "open-id"));

        var result = await fixture.Service.SendByTemplateAsync(new SendNotificationRequest
        {
            TemplateCode = fixture.Template.Code,
            Channel = NotificationChannelType.Sms,
            RecipientUserId = fixture.Profile.UserId,
            Parameters = new Dictionary<string, string>
            {
                ["orderNo"] = "SO001",
                ["amount"] = "88.00"
            }
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(NotificationDispatchStatus.Failed, result.Value!.Dispatch.Status);
        Assert.Equal(1, fixture.DirectoryClient.CallCount);
    }

    [Fact]
    public async Task SendByTemplateAsync_ShouldRemainCompatible_WithLegacyRecipientUserId()
    {
        var fixture = CreateFixture(NotificationChannelType.Sms);

        var result = await fixture.Service.SendByTemplateAsync(new SendNotificationRequest
        {
            TemplateCode = fixture.Template.Code,
            Channel = NotificationChannelType.Sms,
            RecipientUserId = fixture.Profile.UserId,
            Parameters = new Dictionary<string, string>
            {
                ["orderNo"] = "SO001",
                ["amount"] = "88.00"
            }
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(fixture.Profile.UserId, fixture.DispatchRepository.Dispatches.Single().RecipientUserId);
    }

    private static Fixture CreateFixture(
        NotificationChannelType channelType,
        RecipientProfileDto? profile = null,
        Result<RecipientProfileDto>? directoryResult = null)
    {
        var effectiveProfile = profile ?? new RecipientProfileDto(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "13800138000",
            "open-id");

        var template = MessageTemplate.Create(
            Guid.NewGuid(),
            channelType switch
            {
                NotificationChannelType.Sms => "order.pay_success.sms",
                NotificationChannelType.MiniProgram => "order.pay_success.mini_program",
                NotificationChannelType.SiteMessage => "order.pay_success.site_message",
                _ => "order.pay_success.unknown"
            },
            $"Order Pay Success {channelType}",
            "Notify payment success",
            true,
            channelType,
            channelType == NotificationChannelType.Sms ? null : "Payment {{orderNo}}",
            channelType == NotificationChannelType.Sms ? "SMS {{orderNo}} {{amount}}" : "Paid {{amount}}");

        var commandRepository = new FakeMessageTemplateCommandRepository(template);
        var dispatchRepository = new FakeNotificationDispatchCommandRepository();
        var siteMessageRepository = new FakeSiteMessageCommandRepository();
        var directoryClient = new FakeRecipientDirectoryClient(directoryResult ?? Result.Success(effectiveProfile));
        var unitOfWork = new FakeMessageCenterUnitOfWork();
        var service = new NotificationAppService(
            commandRepository,
            dispatchRepository,
            siteMessageRepository,
            directoryClient,
            new FakeTemplateRenderer(),
            [new FakePlaceholderSender(NotificationChannelType.Sms), new FakePlaceholderSender(NotificationChannelType.MiniProgram)],
            unitOfWork);

        return new Fixture(service, template, effectiveProfile, siteMessageRepository, dispatchRepository, directoryClient, unitOfWork);
    }

    private sealed record Fixture(
        NotificationAppService Service,
        MessageTemplate Template,
        RecipientProfileDto Profile,
        FakeSiteMessageCommandRepository SiteMessageRepository,
        FakeNotificationDispatchCommandRepository DispatchRepository,
        FakeRecipientDirectoryClient DirectoryClient,
        FakeMessageCenterUnitOfWork UnitOfWork);

    private sealed class FakeMessageTemplateCommandRepository : IMessageTemplateCommandRepository
    {
        private readonly MessageTemplate _template;

        public FakeMessageTemplateCommandRepository(MessageTemplate template)
        {
            _template = template;
        }

        public Task<MessageTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_template.Id == id ? _template : null);
        }

        public Task<MessageTemplate?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        {
            return Task.FromResult(_template.Code == code ? _template : null);
        }

        public Task<bool> ExistsByCodeAsync(string code, Guid? excludedId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task AddAsync(MessageTemplate messageTemplate, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

    }

    private sealed class FakeNotificationDispatchCommandRepository : INotificationDispatchCommandRepository
    {
        public List<NotificationDispatch> Dispatches { get; } = [];

        public Task AddRangeAsync(IEnumerable<NotificationDispatch> dispatches, CancellationToken cancellationToken)
        {
            Dispatches.AddRange(dispatches);
            return Task.CompletedTask;
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

    private sealed class FakeSiteMessageCommandRepository : ISiteMessageCommandRepository
    {
        public List<SiteMessage> Messages { get; } = [];

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

    private sealed class FakeRecipientDirectoryClient : IRecipientDirectoryClient
    {
        private readonly Result<RecipientProfileDto> _result;

        public FakeRecipientDirectoryClient(Result<RecipientProfileDto> result)
        {
            _result = result;
        }

        public int CallCount { get; private set; }

        public Task<Result<RecipientProfileDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(_result);
        }
    }

    private sealed class FakeTemplateRenderer : ITemplateRenderer
    {
        public Result<RenderedTemplateResult> Render(string? titleTemplate, string bodyTemplate, IReadOnlyDictionary<string, string> parameters)
        {
            var missingKeys = new[] { titleTemplate, bodyTemplate }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .SelectMany(text => ExtractKeys(text!))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(key => !parameters.ContainsKey(key))
                .ToArray();

            if (missingKeys.Length > 0)
            {
                return Result.Failure<RenderedTemplateResult>(Error.Validation($"Missing parameters: {string.Join(", ", missingKeys)}"));
            }

            return Result.Success(new RenderedTemplateResult(
                Render(titleTemplate, parameters),
                Render(bodyTemplate, parameters)!));
        }

        private static IEnumerable<string> ExtractKeys(string template)
        {
            var start = 0;
            while (true)
            {
                var open = template.IndexOf("{{", start, StringComparison.Ordinal);
                if (open < 0)
                {
                    yield break;
                }

                var close = template.IndexOf("}}", open, StringComparison.Ordinal);
                if (close < 0)
                {
                    yield break;
                }

                yield return template.Substring(open + 2, close - open - 2).Trim();
                start = close + 2;
            }
        }

        private static string? Render(string? template, IReadOnlyDictionary<string, string> parameters)
        {
            if (template is null)
            {
                return null;
            }

            var rendered = template;
            foreach (var parameter in parameters)
            {
                rendered = rendered.Replace($"{{{{{parameter.Key}}}}}", parameter.Value, StringComparison.OrdinalIgnoreCase);
            }

            return rendered;
        }
    }

    private sealed class FakePlaceholderSender : INotificationChannelSender
    {
        public FakePlaceholderSender(NotificationChannelType channelType)
        {
            ChannelType = channelType;
        }

        public NotificationChannelType ChannelType { get; }

        public Task<ChannelSendResult> SendAsync(ChannelSendContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ChannelSendResult(NotificationDispatchStatus.Placeholder, "placeholder"));
        }
    }
}

