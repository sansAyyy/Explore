using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Security.Authorization.Constants;
using Explore.MessageCenterService.Api.Controllers;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Abstractions;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Responses;
using Explore.MessageCenterService.Application.Features.Notifications.Abstractions;
using Explore.MessageCenterService.Application.Features.Notifications.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.Notifications.Dtos.Responses;
using Explore.MessageCenterService.Application.Features.SiteMessages.Abstractions;
using Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Responses;
using Explore.MessageCenterService.Domain.MessageTemplates;
using Explore.MessageCenterService.Domain.NotificationDispatches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explore.MessageCenterService.Application.Tests.Controllers;

public sealed class MessageCenterControllerTests
{
    [Fact]
    public async Task MessageTemplatesController_CreateAsync_ShouldReturnCreatedAtAction()
    {
        var controller = new MessageTemplatesController(new FakeMessageTemplateAppService());

        var result = await controller.CreateAsync(new CreateMessageTemplateRequest
        {
            Code = "order.pay_success.sms",
            Name = "Order Pay Success SMS",
            ChannelType = NotificationChannelType.Sms,
            BodyTemplate = "Body"
        }, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task NotificationsController_SendAsync_ShouldReturnOk()
    {
        var controller = new NotificationsController(new FakeNotificationAppService());

        var result = await controller.SendAsync(new SendNotificationRequest
        {
            TemplateCode = "order.pay_success.sms",
            Channel = NotificationChannelType.Sms,
            Recipient = new NotificationRecipientRequest
            {
                PhoneNumber = "13800138000"
            }
        }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SiteMessagesController_MarkReadAsync_ShouldReturnNoContent()
    {
        var controller = new SiteMessagesController(new FakeSiteMessageAppService());

        var result = await controller.MarkReadAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Theory]
    [InlineData(typeof(MessageTemplatesController))]
    [InlineData(typeof(SiteMessagesController))]
    public void Controllers_ShouldRequireAdminPolicy(Type controllerType)
    {
        var attribute = controllerType
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.NotNull(attribute);
        Assert.Equal(AuthorizationPolicies.AdminOnly, attribute!.Policy);
    }

    private sealed class FakeMessageTemplateAppService : IMessageTemplateAppService
    {
        public Task<Result<PagedResult<MessageTemplateBasicResponse>>> GetPagedAsync(GetPagedMessageTemplatesRequest request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<Result<MessageTemplateDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<Result<MessageTemplateDetailResponse>> CreateAsync(CreateMessageTemplateRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success(new MessageTemplateDetailResponse(
                Guid.NewGuid(),
                request.Code,
                request.Name,
                request.Description,
                true,
                request.ChannelType,
                request.TitleTemplate,
                request.BodyTemplate)));
        }

        public Task<Result<MessageTemplateDetailResponse>> UpdateAsync(Guid id, UpdateMessageTemplateRequest request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<Result> EnableAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<Result> DisableAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeNotificationAppService : INotificationAppService
    {
        public Task<Result<SendNotificationResponse>> SendByTemplateAsync(SendNotificationRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success(new SendNotificationResponse(
                Guid.NewGuid(),
                new NotificationDispatchResponse(Guid.NewGuid(), request.Channel, NotificationDispatchStatus.Delivered, null))));
        }
    }

    private sealed class FakeSiteMessageAppService : ISiteMessageAppService
    {
        public Task<Result<PagedResult<SiteMessageBasicResponse>>> GetPagedAsync(GetPagedSiteMessagesRequest request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<Result<SiteMessageDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<Result> MarkReadAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }

        public Task<Result> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}

