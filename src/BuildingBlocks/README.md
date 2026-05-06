# BuildingBlocks

`src/BuildingBlocks/` 存放 Explore 的横切基础模块。这里不仅有通用工具，还包含认证与安全、Correlation / CurrentUser、自动依赖注入、领域建模、持久化审计、OpenAPI、消息抽象，以及按“抽象 + provider / inbox / outbox”拆分的基础能力。

下表中的“直接消费方”仅按当前 `src/Gateways` 与 `src/Services` 目录下 `.csproj` 的 `ProjectReference` 统计，不包含 `BuildingBlocks` 模块之间的内部引用。

## 总矩阵

| 模块 | 提供能力 | 显式第三方依赖 | 直接消费方 |
| --- | --- | --- | --- |
| `BuildingBlocks.Caching.Abstractions` | `ICacheService` 缓存抽象 | 无显式 PackageReference | `Explore.AdminIdentityService.Application`；`Explore.CustomerAccountService.Application` |
| `BuildingBlocks.Caching.Redis` | Redis 缓存实现、`AddRedisCaching` 注册扩展 | `FreeRedis 1.5.5`；`Microsoft.Extensions.Configuration.Abstractions 10.0.4`；`Microsoft.Extensions.DependencyInjection.Abstractions 10.0.4` | `Explore.AdminIdentityService.Infrastructure`；`Explore.CustomerAccountService.Infrastructure` |
| `BuildingBlocks.Common` | `Result` / `Error`、分页模型、Typed HttpClient、resilience、CorrelationId 透传 | `Microsoft.Extensions.Http.Resilience 10.4.0` | `Explore.AdminIdentityService.Application`；`Explore.CustomerAccountService.Application`；`Explore.MessageCenterService.Application` |
| `BuildingBlocks.Correlation.Abstractions` | CorrelationId 常量与上下文访问器抽象 | 无显式 PackageReference | 无直接 Gateway / Service 引用 |
| `BuildingBlocks.Correlation.AspNetCore` | 请求级 CorrelationId 中间件、访问器注册、日志上下文注入 | `Microsoft.AspNetCore.Http.Abstractions 2.3.9`；`Microsoft.Extensions.DependencyInjection.Abstractions 10.0.4`；`Serilog 4.3.0` | `Explore.Gateway.Api`；3 个服务的 `Api` 项目 |
| `BuildingBlocks.CurrentUser` | 当前用户解析、审计主体提取 | 无显式 PackageReference | `Explore.AdminIdentityService.Application`；`Explore.AdminIdentityService.Infrastructure`；`Explore.CustomerAccountService.Application`；`Explore.CustomerAccountService.Infrastructure`；`Explore.MessageCenterService.Infrastructure` |
| `BuildingBlocks.DependencyInjection` | 模块发现、自动注册、依赖注入约定 | `Microsoft.Extensions.Configuration.Abstractions 10.0.3`；`Microsoft.Extensions.DependencyInjection.Abstractions 10.0.3`；`Microsoft.Extensions.DependencyModel 10.0.3` | 3 个服务的 `Api` / `Application` / `Infrastructure` 项目 |
| `BuildingBlocks.DistributedLocking.Abstractions` | 分布式锁抽象、锁获取参数模型 | 无显式 PackageReference | 无直接 Gateway / Service 引用 |
| `BuildingBlocks.DistributedLocking.Redis` | Redis 分布式锁实现、`AddRedisDistributedLocking` 注册扩展 | `FreeRedis 1.5.5`；`Microsoft.Extensions.Configuration.Abstractions 10.0.4`；`Microsoft.Extensions.DependencyInjection.Abstractions 10.0.4` | `Explore.AdminIdentityService.Infrastructure`；`Explore.CustomerAccountService.Infrastructure` |
| `BuildingBlocks.Domain` | `Entity`、`ValueObject`、`AuditableEntity`、领域异常 | 无显式 PackageReference | 3 个服务的 `Domain` 项目 |
| `BuildingBlocks.Logging` | Serilog 宿主配置、ServiceName enrich、异步文件 / 控制台输出 | `Serilog.AspNetCore 10.0.0`；`Serilog.Enrichers.Environment 3.0.1`；`Serilog.Enrichers.Thread 4.0.0`；`Serilog.Sinks.Async 2.1.0` | `Explore.Gateway.Api`；3 个服务的 `Api` 项目 |
| `BuildingBlocks.Messaging.Abstractions` | 事件发布抽象、Envelope 抽象 | 无显式 PackageReference | 无直接 Gateway / Service 引用 |
| `BuildingBlocks.Messaging.Outbox.Abstractions` | Outbox 写入抽象 | 无显式 PackageReference | `Explore.AdminIdentityService.Infrastructure` |
| `BuildingBlocks.Messaging.Outbox.EntityFrameworkCore` | 基于 EF Core 的 Outbox 模型、写入器、Dispatcher、HostedService | `Microsoft.EntityFrameworkCore 10.0.4`；`Microsoft.EntityFrameworkCore.Relational 10.0.4`；`Microsoft.Extensions.Hosting.Abstractions 10.0.0`；`Microsoft.Extensions.Logging.Abstractions 10.0.4`；`Microsoft.Extensions.Options 10.0.4` | `Explore.AdminIdentityService.Infrastructure` |
| `BuildingBlocks.Messaging.Inbox.Abstractions` | Inbox 幂等处理抽象 | 无显式 PackageReference | `Explore.MessageCenterService.Infrastructure` |
| `BuildingBlocks.Messaging.Inbox.EntityFrameworkCore` | 基于 EF Core 的 Inbox 模型、处理器与注册扩展 | `Microsoft.EntityFrameworkCore 10.0.4`；`Microsoft.EntityFrameworkCore.Relational 10.0.4` | `Explore.MessageCenterService.Infrastructure` |
| `BuildingBlocks.Messaging.RabbitMQ` | MassTransit + RabbitMQ 集成、消费者发现、Correlation Filter、发布器实现 | `MassTransit 8.5.9`；`MassTransit.RabbitMQ 8.5.9`；`Microsoft.Extensions.Configuration.Binder 10.0.0` | `Explore.AdminIdentityService.Infrastructure`；`Explore.MessageCenterService.Infrastructure` |
| `BuildingBlocks.OpenApi` | 提供统一的 OpenAPI / Scalar 注册与映射扩展 | `Microsoft.AspNetCore.OpenApi 10.0.3`；`Scalar.AspNetCore 2.13.22` | 3 个服务的 `Api` 项目 |
| `BuildingBlocks.Persistence.EntityFrameworkCore` | 审计型 `DbContext`、软删除、并发版本、查询过滤 | `Microsoft.EntityFrameworkCore 10.0.4` | `Explore.AdminIdentityService.Infrastructure`；`Explore.CustomerAccountService.Infrastructure`；`Explore.MessageCenterService.Infrastructure` |
| `BuildingBlocks.Security` | JWT 鉴权、Token 生成、授权策略、密码哈希、手机号验证码 | `Microsoft.AspNetCore.Authentication.JwtBearer 10.0.3`；`System.IdentityModel.Tokens.Jwt 8.14.0` | `Explore.Gateway.Api`；`Explore.AdminIdentityService.Application`；`Explore.AdminIdentityService.Infrastructure`；`Explore.CustomerAccountService.Application`；`Explore.CustomerAccountService.Infrastructure`；`Explore.MessageCenterService.Infrastructure` |

## 模块说明

### BuildingBlocks.Caching.Abstractions

- 职责：提供统一缓存抽象 `ICacheService`，供 `Application` 与需要缓存抽象的其他模块依赖。
- 关键代码：`BuildingBlocks.Caching.Abstractions/ICacheService.cs`
- PackageReference：无显式 PackageReference
- FrameworkReference：无
- ProjectReference：无
- 直接消费方：`Explore.AdminIdentityService.Application`、`Explore.CustomerAccountService.Application`

### BuildingBlocks.Caching.Redis

- 职责：提供基于 Redis 的 `ICacheService` 实现，以及 `AddRedisCaching` 注册扩展。
- 关键代码：`BuildingBlocks.Caching.Redis/Services/RedisCacheService.cs`、`BuildingBlocks.Caching.Redis/Extensions/ServiceCollectionExtensions.cs`
- PackageReference：`FreeRedis 1.5.5`、`Microsoft.Extensions.Configuration.Abstractions 10.0.4`、`Microsoft.Extensions.DependencyInjection.Abstractions 10.0.4`
- FrameworkReference：无
- ProjectReference：`BuildingBlocks.Caching.Abstractions`
- 直接消费方：`Explore.AdminIdentityService.Infrastructure`、`Explore.CustomerAccountService.Infrastructure`

### BuildingBlocks.DistributedLocking.Abstractions

- 职责：提供分布式锁抽象 `IDistributedLockService` / `IDistributedLockLease`，以及 `DistributedLockAcquireOptions`、`DistributedLockDefaults` 模型。
- 关键代码：`BuildingBlocks.DistributedLocking.Abstractions/Abstractions/IDistributedLockService.cs`、`BuildingBlocks.DistributedLocking.Abstractions/Abstractions/IDistributedLockLease.cs`、`BuildingBlocks.DistributedLocking.Abstractions/Models/*.cs`
- PackageReference：无显式 PackageReference
- FrameworkReference：无
- ProjectReference：无
- 直接消费方：无直接 Gateway / Service 引用

### BuildingBlocks.DistributedLocking.Redis

- 职责：提供基于 Redis 的分布式锁实现，以及 `AddRedisDistributedLocking` 注册扩展。
- 关键代码：`BuildingBlocks.DistributedLocking.Redis/Services/RedisDistributedLockService.cs`、`BuildingBlocks.DistributedLocking.Redis/Services/RedisDistributedLockLease.cs`、`BuildingBlocks.DistributedLocking.Redis/Extensions/ServiceCollectionExtensions.cs`
- PackageReference：`FreeRedis 1.5.5`、`Microsoft.Extensions.Configuration.Abstractions 10.0.4`、`Microsoft.Extensions.DependencyInjection.Abstractions 10.0.4`
- FrameworkReference：无
- ProjectReference：`BuildingBlocks.DistributedLocking.Abstractions`
- 直接消费方：`Explore.AdminIdentityService.Infrastructure`、`Explore.CustomerAccountService.Infrastructure`
- 分布式锁使用方式：业务方按调用点显式传入 `DistributedLockAcquireOptions`；基础层仅提供默认值兜底，不维护按业务/按锁名的统一策略配置。

```csharp
await using var lease = await distributedLock.TryAcquireAsync(
    "admin-auth:refresh:user:123",
    new DistributedLockAcquireOptions
    {
        LeaseTtl = TimeSpan.FromSeconds(10),
        WaitTimeout = TimeSpan.FromSeconds(2)
    },
    cancellationToken);

if (lease is null)
{
    return;
}
```

### BuildingBlocks.Common

- 职责：提供统一结果模型、错误模型、分页模型，以及带有 resilience 和 CorrelationId 透传能力的 Typed HttpClient 注册扩展。
- 关键代码：`Results/Result.cs`、`Results/Error.cs`、`Results/ActionResultExtensions.cs`、`Pagination/PagedRequest.cs`、`Pagination/PagedResult.cs`、`Http/OutboundHttpClientRegistrationExtensions.cs`
- PackageReference：`Microsoft.Extensions.Http.Resilience 10.4.0`
- FrameworkReference：`Microsoft.AspNetCore.App`
- ProjectReference：`BuildingBlocks.Correlation.Abstractions`
- 直接消费方：`Explore.AdminIdentityService.Application`、`Explore.CustomerAccountService.Application`、`Explore.MessageCenterService.Application`

### BuildingBlocks.Correlation.Abstractions

- 职责：定义 CorrelationId 常量和上下文访问器抽象，供 ASP.NET Core 中间件、消息模块和 HTTP 客户端共享。
- 关键代码：`Constants/CorrelationIdConstants.cs`、`ContextAccessors/ICorrelationContextAccessor.cs`
- PackageReference：无显式 PackageReference
- FrameworkReference：无
- ProjectReference：无
- 直接消费方：无直接 Gateway / Service 引用；当前主要被其他 `BuildingBlocks` 模块复用

### BuildingBlocks.Correlation.AspNetCore

- 职责：在 ASP.NET Core 请求管道中生成 / 透传 CorrelationId，写入响应头，并推入 Serilog `LogContext`。
- 关键代码：`Middlewares/CorrelationIdMiddleware.cs`、`ContextAccessors/CorrelationContextAccessor.cs`、`Extensions/ServiceCollectionExtensions.cs`
- PackageReference：`Microsoft.AspNetCore.Http.Abstractions 2.3.9`、`Microsoft.Extensions.DependencyInjection.Abstractions 10.0.4`、`Serilog 4.3.0`
- FrameworkReference：无
- ProjectReference：`BuildingBlocks.Correlation.Abstractions`
- 直接消费方：`Explore.Gateway.Api`、`Explore.AdminIdentityService.Api`、`Explore.CustomerAccountService.Api`、`Explore.MessageCenterService.Api`

### BuildingBlocks.CurrentUser

- 职责：从 `HttpContext.User` 中提取当前用户标识，并为审计提供 `AuditActor`。
- 关键代码：`Abstractions/ICurrentUser.cs`、`Services/CurrentUser.cs`、`Claims/Extensions/ClaimsPrincipalExtensions.cs`、`Extensions/ServiceCollectionExtensions.cs`
- PackageReference：无显式 PackageReference
- FrameworkReference：`Microsoft.AspNetCore.App`
- ProjectReference：无
- 直接消费方：`Explore.AdminIdentityService.Application`、`Explore.AdminIdentityService.Infrastructure`、`Explore.CustomerAccountService.Application`、`Explore.CustomerAccountService.Infrastructure`、`Explore.MessageCenterService.Infrastructure`

### BuildingBlocks.DependencyInjection

- 职责：通过 `IModuleDependencyRegistrar` 与标记接口约定，按程序集发现并执行模块注册和约定注入。
- 关键代码：`BuildingBlocks.DependencyInjection.Abstractions/Abstractions/IModuleDependencyRegistrar.cs`、`BuildingBlocks.DependencyInjection.Abstractions/Abstractions/ITransientDependency.cs`、`BuildingBlocks.DependencyInjection.Abstractions/Abstractions/IScopeDependency.cs`、`BuildingBlocks.DependencyInjection.Abstractions/Abstractions/ISingletonDependency.cs`、`BuildingBlocks.DependencyInjection.Conventional/Extensions/ServiceCollectionExtensions.cs`
- PackageReference：`Microsoft.Extensions.Configuration.Abstractions 10.0.3`、`Microsoft.Extensions.DependencyInjection.Abstractions 10.0.3`、`Microsoft.Extensions.DependencyModel 10.0.3`
- FrameworkReference：无
- ProjectReference：无
- 直接消费方：3 个服务的 `Api`、`Application`、`Infrastructure` 项目

### BuildingBlocks.Domain

- 职责：提供领域建模基础抽象，包括实体、值对象、可审计实体和领域异常。
- 关键代码：`Abstractions/Entity.cs`、`Abstractions/ValueObject.cs`、`Abstractions/AuditableEntity.cs`、`Exceptions/DomainException.cs`
- PackageReference：无显式 PackageReference
- FrameworkReference：无
- ProjectReference：无
- 直接消费方：`Explore.AdminIdentityService.Domain`、`Explore.CustomerAccountService.Domain`、`Explore.MessageCenterService.Domain`

### BuildingBlocks.Logging

- 职责：统一宿主日志初始化，配置 ServiceName enrich、控制台输出和异步文件输出。
- 关键代码：`Extensions/HostBuilderExtensions.cs`、`Options/LoggingOptions.cs`、`Enrichers/ServiceNameEnricher.cs`
- PackageReference：`Serilog.AspNetCore 10.0.0`、`Serilog.Enrichers.Environment 3.0.1`、`Serilog.Enrichers.Thread 4.0.0`、`Serilog.Sinks.Async 2.1.0`
- FrameworkReference：无
- ProjectReference：无
- 直接消费方：`Explore.Gateway.Api`、`Explore.AdminIdentityService.Api`、`Explore.CustomerAccountService.Api`、`Explore.MessageCenterService.Api`

### BuildingBlocks.Messaging.Abstractions

- 职责：定义事件发布接口、Envelope 发布接口和标准消息 Envelope 模型。
- 关键代码：`Abstractions/IEventPublisher.cs`、`Abstractions/IEnvelopePublisher.cs`、`Envelope/MessageEnvelope.cs`
- PackageReference：无显式 PackageReference
- FrameworkReference：无
- ProjectReference：无
- 直接消费方：无直接 Gateway / Service 引用；当前主要被 `Messaging.Outbox.*`、`Messaging.Inbox.*`、`Messaging.RabbitMQ` 复用

### BuildingBlocks.Messaging.Outbox.Abstractions

- 职责：定义 Outbox 写入抽象 `IOutboxMessageWriter`，供业务服务在本地事务中写入待发送消息。
- 关键代码：`BuildingBlocks.Messaging.Outbox.Abstractions/IOutboxMessageWriter.cs`
- PackageReference：无显式 PackageReference
- FrameworkReference：无
- ProjectReference：`BuildingBlocks.Messaging.Abstractions`
- 直接消费方：`Explore.AdminIdentityService.Infrastructure`

### BuildingBlocks.Messaging.Outbox.EntityFrameworkCore

- 职责：提供基于 EF Core 的 Outbox 能力，包括消息模型、模型配置、写入器、批量 Dispatcher 和 HostedService。
- 关键代码：`OutboxMessage.cs`、`OutboxMessageConfiguration.cs`、`Writers/EntityFrameworkOutboxMessageWriter.cs`、`Dispatchers/EntityFrameworkOutboxDispatcher.cs`、`Dispatchers/OutboxDispatcherHostedService.cs`、`Extensions/ServiceCollectionExtensions.cs`
- PackageReference：`Microsoft.EntityFrameworkCore 10.0.4`、`Microsoft.EntityFrameworkCore.Relational 10.0.4`、`Microsoft.Extensions.Hosting.Abstractions 10.0.0`、`Microsoft.Extensions.Logging.Abstractions 10.0.4`、`Microsoft.Extensions.Options 10.0.4`
- FrameworkReference：无
- ProjectReference：`BuildingBlocks.Correlation.Abstractions`、`BuildingBlocks.Messaging.Abstractions`、`BuildingBlocks.Messaging.Outbox.Abstractions`
- 直接消费方：`Explore.AdminIdentityService.Infrastructure`

### BuildingBlocks.Messaging.Inbox.Abstractions

- 职责：定义 Inbox 幂等处理抽象 `IInboxMessageProcessor`，供消费者在处理消息时包裹重复消费保护。
- 关键代码：`BuildingBlocks.Messaging.Inbox.Abstractions/IInboxMessageProcessor.cs`
- PackageReference：无显式 PackageReference
- FrameworkReference：无
- ProjectReference：`BuildingBlocks.Messaging.Abstractions`
- 直接消费方：`Explore.MessageCenterService.Infrastructure`

### BuildingBlocks.Messaging.Inbox.EntityFrameworkCore

- 职责：提供基于 EF Core 的 Inbox 能力，包括消息模型、模型配置、处理器与注册扩展。
- 关键代码：`InboxMessage.cs`、`InboxMessageConfiguration.cs`、`Processors/EntityFrameworkInboxMessageProcessor.cs`、`Extensions/ServiceCollectionExtensions.cs`
- PackageReference：`Microsoft.EntityFrameworkCore 10.0.4`、`Microsoft.EntityFrameworkCore.Relational 10.0.4`
- FrameworkReference：无
- ProjectReference：`BuildingBlocks.Messaging.Abstractions`、`BuildingBlocks.Messaging.Inbox.Abstractions`
- 直接消费方：`Explore.MessageCenterService.Infrastructure`

### BuildingBlocks.Messaging.RabbitMQ

- 职责：基于 MassTransit 提供 RabbitMQ 集成，支持消费者自动发现、CorrelationId Consume / Publish Filter，以及统一发布器实现。
- 关键代码：`Extensions/ServiceCollectionExtensions.cs`、`Options/RabbitMqOptions.cs`、`Filters/CorrelationConsumeFilter.cs`、`Filters/CorrelationPublishFilter.cs`、`Publishers/MassTransitEventPublisher.cs`、`Publishers/MassTransitEnvelopePublisher.cs`
- PackageReference：`MassTransit 8.5.9`、`MassTransit.RabbitMQ 8.5.9`、`Microsoft.Extensions.Configuration.Binder 10.0.0`
- FrameworkReference：无
- ProjectReference：`BuildingBlocks.Correlation.Abstractions`、`BuildingBlocks.Messaging.Abstractions`
- 直接消费方：`Explore.AdminIdentityService.Infrastructure`、`Explore.MessageCenterService.Infrastructure`

### BuildingBlocks.OpenApi

- 职责：提供统一的 OpenAPI / Scalar 注册与映射扩展，支持服务级文档元信息配置和可选 Bearer 鉴权声明。
- 关键代码：`Extensions/ServiceOpenApiExtensions.cs`、`Options/ServiceOpenApiOptions.cs`
- PackageReference：`Microsoft.AspNetCore.OpenApi 10.0.3`、`Scalar.AspNetCore 2.13.22`
- FrameworkReference：`Microsoft.AspNetCore.App`
- ProjectReference：无
- 直接消费方：`Explore.AdminIdentityService.Api`、`Explore.CustomerAccountService.Api`、`Explore.MessageCenterService.Api`

### BuildingBlocks.Persistence.EntityFrameworkCore

- 职责：提供带审计字段、软删除和并发版本控制的 `AuditableDbContext` 基类，并统一可审计实体的模型配置。
- 关键代码：`Auditing/AuditableDbContext.cs`
- PackageReference：`Microsoft.EntityFrameworkCore 10.0.4`
- FrameworkReference：无
- ProjectReference：`BuildingBlocks.CurrentUser`、`BuildingBlocks.Domain`
- 直接消费方：`Explore.AdminIdentityService.Infrastructure`、`Explore.CustomerAccountService.Infrastructure`、`Explore.MessageCenterService.Infrastructure`

### BuildingBlocks.Security

- 职责：提供 JWT 认证与授权策略、访问令牌生成、密码哈希、手机号验证码服务。
- 关键代码：`Authentication/Jwt/Extensions/JwtBearerExtensions.cs`、`Authentication/Jwt/Services/JwtTokenService.cs`、`Hashing/PasswordHashService.cs`、`Hashing/Extensions/ServiceCollectionExtensions.cs`、`PhoneVerification/Services/PhoneVerificationCodeService.cs`
- PackageReference：`Microsoft.AspNetCore.Authentication.JwtBearer 10.0.3`、`System.IdentityModel.Tokens.Jwt 8.14.0`
- FrameworkReference：无
- ProjectReference：`BuildingBlocks.Caching.Abstractions`、`BuildingBlocks.Common`
- 直接消费方：`Explore.Gateway.Api`、`Explore.AdminIdentityService.Application`、`Explore.AdminIdentityService.Infrastructure`、`Explore.CustomerAccountService.Application`、`Explore.CustomerAccountService.Infrastructure`、`Explore.MessageCenterService.Infrastructure`

## 继续阅读

- 根目录导航： [../../README.md](../../README.md)
- 后端总览： [../README.md](../README.md)
- 服务总览： [../Services/README.md](../Services/README.md)
