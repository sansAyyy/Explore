using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Abstractions;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Services;
using Explore.MessageCenterService.Application.Features.Notifications.Abstractions;
using Explore.MessageCenterService.Application.Features.Notifications.Services;
using Explore.MessageCenterService.Application.Features.SiteMessages.Abstractions;
using Explore.MessageCenterService.Application.Features.SiteMessages.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Explore.MessageCenterService.Application;

public class DependencyInjection : IModuleDependencyRegistrar
{
    public IServiceCollection ModuleDependencyInjection(IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}

