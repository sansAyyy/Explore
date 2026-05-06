# 请参阅 https://aka.ms/customizecontainer 以了解如何自定义调试容器，以及 Visual Studio 如何使用此 Dockerfile 生成映像以更快地进行调试。

# 此阶段用于在快速模式(默认为调试配置)下从 VS 运行时
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# 此阶段用于生成服务项目
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["src/Gateways/Explore.Gateway.Api/Explore.Gateway.Api.csproj", "src/Gateways/Explore.Gateway.Api/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Correlation/BuildingBlocks.Correlation.AspNetCore/BuildingBlocks.Correlation.AspNetCore.csproj", "src/BuildingBlocks/BuildingBlocks.Correlation/BuildingBlocks.Correlation.AspNetCore/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Correlation/BuildingBlocks.Correlation.Abstractions/BuildingBlocks.Correlation.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.Correlation/BuildingBlocks.Correlation.Abstractions/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Logging/BuildingBlocks.Logging.Serilog/BuildingBlocks.Logging.Serilog.csproj", "src/BuildingBlocks/BuildingBlocks.Logging/BuildingBlocks.Logging.Serilog/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authentication/BuildingBlocks.Security.Authentication.Abstractions/BuildingBlocks.Security.Authentication.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authentication/BuildingBlocks.Security.Authentication.Abstractions/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authentication/BuildingBlocks.Security.Authentication.Jwt/BuildingBlocks.Security.Authentication.Jwt.csproj", "src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authentication/BuildingBlocks.Security.Authentication.Jwt/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authorization/BuildingBlocks.Security.Authorization.AspNetCore/BuildingBlocks.Security.Authorization.AspNetCore.csproj", "src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authorization/BuildingBlocks.Security.Authorization.AspNetCore/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authorization/BuildingBlocks.Security.Authorization.Abstractions/BuildingBlocks.Security.Authorization.Abstractions.csproj", "src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.Authorization/BuildingBlocks.Security.Authorization.Abstractions/"]
RUN dotnet restore "./src/Gateways/Explore.Gateway.Api/Explore.Gateway.Api.csproj"
COPY . .
WORKDIR "/src/src/Gateways/Explore.Gateway.Api"
RUN dotnet build "./Explore.Gateway.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 此阶段用于发布要复制到最终阶段的服务项目
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Explore.Gateway.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 此阶段在生产中使用，或在常规模式下从 VS 运行时使用(在不使用调试配置时为默认值)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Explore.Gateway.Api.dll"]