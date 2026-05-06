using BuildingBlocks.Security.PhoneVerification.Abstractions;
using BuildingBlocks.Security.PhoneVerification.Options;
using BuildingBlocks.Security.PhoneVerification.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Security.PhoneVerification.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPhoneVerificationCode(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = PhoneVerificationCodeOptions.SectionName)
    {
        services.Configure<PhoneVerificationCodeOptions>(configuration.GetSection(sectionName));
        services.AddScoped<IPhoneVerificationCodeService, PhoneVerificationCodeService>();
        return services;
    }
}

