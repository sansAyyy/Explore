# 请参阅 https://aka.ms/customizecontainer 以了解如何自定义调试容器，以及 Visual Studio 如何使用此 Dockerfile 生成映像以更快地进行调试。

# 此阶段用于在快速模式(默认为调试配置)下从 VS 运行时
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# 此阶段用于生成服务项目
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["src/Services/AdminIdentityService/Explore.AdminIdentityService.Api/Explore.AdminIdentityService.Api.csproj", "src/Services/AdminIdentityService/Explore.AdminIdentityService.Api/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.AspNetCore/BuildingBlocks.Common.AspNetCore.csproj", "src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.AspNetCore/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common/BuildingBlocks.Common.csproj", "src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Correlation/BuildingBlocks.Correlation.AspNetCore/BuildingBlocks.Correlation.AspNetCore.csproj", "src/BuildingBlocks/BuildingBlocks.Correlation/BuildingBlocks.Correlation.AspNetCore/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Correlation/BuildingBlocks.Correlation.Abstractions/BuildingBlocks.Correlation.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.Correlation/BuildingBlocks.Correlation.Abstractions/"]
COPY ["src/BuildingBlocks/BuildingBlocks.DependencyInjection/BuildingBlocks.DependencyInjection.Conventional/BuildingBlocks.DependencyInjection.Conventional.csproj", "src/BuildingBlocks/BuildingBlocks.DependencyInjection/BuildingBlocks.DependencyInjection.Conventional/"]
COPY ["src/BuildingBlocks/BuildingBlocks.DependencyInjection/BuildingBlocks.DependencyInjection.Abstractions/BuildingBlocks.DependencyInjection.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.DependencyInjection/BuildingBlocks.DependencyInjection.Abstractions/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Logging/BuildingBlocks.Logging.Serilog/BuildingBlocks.Logging.Serilog.csproj", "src/BuildingBlocks/BuildingBlocks.Logging/BuildingBlocks.Logging.Serilog/"]
COPY ["src/BuildingBlocks/BuildingBlocks.OpenApi/BuildingBlocks.OpenApi.AspNetCore/BuildingBlocks.OpenApi.AspNetCore.csproj", "src/BuildingBlocks/BuildingBlocks.OpenApi/BuildingBlocks.OpenApi.AspNetCore/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authorization/BuildingBlocks.Security.Authorization.Abstractions/BuildingBlocks.Security.Authorization.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authorization/BuildingBlocks.Security.Authorization.Abstractions/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authorization/BuildingBlocks.Security.Authorization.AspNetCore/BuildingBlocks.Security.Authorization.AspNetCore.csproj", "src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authorization/BuildingBlocks.Security.Authorization.AspNetCore/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authentication/BuildingBlocks.Security.Authentication.Abstractions/BuildingBlocks.Security.Authentication.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authentication/BuildingBlocks.Security.Authentication.Abstractions/"]
COPY ["src/Services/AdminIdentityService/Explore.AdminIdentityService.Application/Explore.AdminIdentityService.Application.csproj", "src/Services/AdminIdentityService/Explore.AdminIdentityService.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Caching/BuildingBlocks.Caching.Abstractions/BuildingBlocks.Caching.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.Caching/BuildingBlocks.Caching.Abstractions/"]
COPY ["src/BuildingBlocks/BuildingBlocks.CurrentUser/BuildingBlocks.CurrentUser.Abstractions/BuildingBlocks.CurrentUser.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.CurrentUser/BuildingBlocks.CurrentUser.Abstractions/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Hashing/BuildingBlocks.Security.Hashing.Abstractions/BuildingBlocks.Security.Hashing.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Hashing/BuildingBlocks.Security.Hashing.Abstractions/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.PhoneVerification/BuildingBlocks.Security.PhoneVerification.Abstractions/BuildingBlocks.Security.PhoneVerification.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.PhoneVerification/BuildingBlocks.Security.PhoneVerification.Abstractions/"]
COPY ["src/Services/AdminIdentityService/Explore.AdminIdentityService.Domain/Explore.AdminIdentityService.Domain.csproj", "src/Services/AdminIdentityService/Explore.AdminIdentityService.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.Core/BuildingBlocks.Domain.Core.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.Core/"]
COPY ["src/Services/AdminIdentityService/Explore.AdminIdentityService.Infrastructure/Explore.AdminIdentityService.Infrastructure.csproj", "src/Services/AdminIdentityService/Explore.AdminIdentityService.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Caching/BuildingBlocks.Caching.Redis/BuildingBlocks.Caching.Redis.csproj", "src/BuildingBlocks/BuildingBlocks.Caching/BuildingBlocks.Caching.Redis/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.Http/BuildingBlocks.Common.Http.csproj", "src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.Http/"]
COPY ["src/BuildingBlocks/BuildingBlocks.CurrentUser/BuildingBlocks.CurrentUser.AspNetCore/BuildingBlocks.CurrentUser.AspNetCore.csproj", "src/BuildingBlocks/BuildingBlocks.CurrentUser/BuildingBlocks.CurrentUser.AspNetCore/"]
COPY ["src/BuildingBlocks/BuildingBlocks.DistributedLocking/BuildingBlocks.DistributedLocking.Redis/BuildingBlocks.DistributedLocking.Redis.csproj", "src/BuildingBlocks/BuildingBlocks.DistributedLocking/BuildingBlocks.DistributedLocking.Redis/"]
COPY ["src/BuildingBlocks/BuildingBlocks.DistributedLocking/BuildingBlocks.DistributedLocking.Abstractions/BuildingBlocks.DistributedLocking.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.DistributedLocking/BuildingBlocks.DistributedLocking.Abstractions/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.Outbox/BuildingBlocks.Messaging.Outbox.Abstractions/BuildingBlocks.Messaging.Outbox.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.Outbox/BuildingBlocks.Messaging.Outbox.Abstractions/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.Abstractions/BuildingBlocks.Messaging.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.Abstractions/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.Outbox/BuildingBlocks.Messaging.Outbox.EntityFrameworkCore/BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.csproj", "src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.Outbox/BuildingBlocks.Messaging.Outbox.EntityFrameworkCore/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.RabbitMQ/BuildingBlocks.Messaging.RabbitMQ.csproj", "src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.RabbitMQ/"]
COPY ["src/Contracts/Explore.Contracts.Messaging/Explore.Contracts.Messaging.csproj", "src/Contracts/Explore.Contracts.Messaging/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Persistence/BuildingBlocks.Persistence.Auditing.CurrentUser/BuildingBlocks.Persistence.Auditing.CurrentUser.csproj", "src/BuildingBlocks/BuildingBlocks.Persistence/BuildingBlocks.Persistence.Auditing.CurrentUser/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Persistence/BuildingBlocks.Persistence.Auditing.Abstractions/BuildingBlocks.Persistence.Auditing.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.Persistence/BuildingBlocks.Persistence.Auditing.Abstractions/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Persistence/BuildingBlocks.Persistence.Auditing.EntityFrameworkCore/BuildingBlocks.Persistence.Auditing.EntityFrameworkCore.csproj", "src/BuildingBlocks/BuildingBlocks.Persistence/BuildingBlocks.Persistence.Auditing.EntityFrameworkCore/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authentication/BuildingBlocks.Security.Authentication.Jwt/BuildingBlocks.Security.Authentication.Jwt.csproj", "src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authentication/BuildingBlocks.Security.Authentication.Jwt/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Hashing/BuildingBlocks.Security.Hashing.AspNetCoreIdentity/BuildingBlocks.Security.Hashing.AspNetCoreIdentity.csproj", "src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Hashing/BuildingBlocks.Security.Hashing.AspNetCoreIdentity/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.PhoneVerification/BuildingBlocks.Security.PhoneVerification/BuildingBlocks.Security.PhoneVerification.csproj", "src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.PhoneVerification/BuildingBlocks.Security.PhoneVerification/"]
RUN dotnet restore "./src/Services/AdminIdentityService/Explore.AdminIdentityService.Api/Explore.AdminIdentityService.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/AdminIdentityService/Explore.AdminIdentityService.Api"
RUN dotnet build "./Explore.AdminIdentityService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 此阶段用于发布要复制到最终阶段的服务项目
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Explore.AdminIdentityService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 此阶段在生产中使用，或在常规模式下从 VS 运行时使用(在不使用调试配置时为默认值)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Explore.AdminIdentityService.Api.dll"]