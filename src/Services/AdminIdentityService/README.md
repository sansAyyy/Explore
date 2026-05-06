# AdminIdentityService

`AdminIdentityService` 负责管理员身份体系，包括登录、授权上下文、角色、权限及相关认证支撑能力。管理端的 RBAC 能力主要依赖这个服务。

## 目录结构

- `Explore.AdminIdentityService.Api`：HTTP 接口、OpenAPI、宿主配置
- `Explore.AdminIdentityService.Application`：应用服务、用例编排
- `Explore.AdminIdentityService.Domain`：领域模型与核心业务规则
- `Explore.AdminIdentityService.Infrastructure`：数据库、缓存、消息（Outbox / RabbitMQ）与外部集成实现

## BuildingBlocks 依赖约定

- `Application` 只依赖抽象型模块，例如 `BuildingBlocks.Caching.Abstractions`。
- `Infrastructure` 负责接入实现型模块，例如 `BuildingBlocks.Caching.Redis`、`BuildingBlocks.DistributedLocking.Redis`、`BuildingBlocks.Messaging.*`。
- `Security` 内部复用 `ICacheService`，但不直接绑定 Redis provider。

## 当前通知链路

- 短信验证码由 `AdminAuthenticationAppService` 通过 `IAdminMessageCenterClient` 调用 `MessageCenterService` 的 HTTP 接口发送。
- 管理员创建、启停、改密等站内通知由 `AdminUserAppService` 通过 `IAdminSiteMessageSender` 写入 `SendNotificationByTemplateMessage` 到本地 Outbox。
- `AdminIdentityUnitOfWork` 提交管理员业务变更时，会与 Outbox 记录共用同一个 `AdminIdentityDbContext` 事务边界。
- Outbox Dispatcher 再经 RabbitMQ 投递消息，由 `MessageCenterService` 消费后生成站内信并提供查询 / 已读接口。

## 依赖注入约定

- `Program.cs` 通过 `AddModules(builder.Configuration)` 统一加载 `Application`、`Infrastructure` 的模块注册。
- 常规应用服务和仓储实现通过 `IScopeDependency` 自动注册，不需要在 `Application.DependencyInjection` 里手写 `AddScoped`。
- `Infrastructure.DependencyInjection` 主要保留数据库、RabbitMQ、Outbox、Redis 和 Typed HttpClient 这类需要显式参数的注册。

## 运行依赖

- PostgreSQL
- Redis
- RabbitMQ（管理员站内信异步投递）
- Message Center HTTP 接口（管理员短信验证码）

## 核心配置

重点关注以下配置项：

- `ConnectionStrings:AdminIdentityDatabase`
- `ConnectionStrings:Redis`
- `RabbitMqOptions:*`
- `MessageCenter:*`
- `JwtOptions:*`
- `PhoneVerificationCode:*`
- `LoggingOptions:*`

其中 `MessageCenter:*` 当前用于短信验证码场景下的 Typed HttpClient 配置；站内信异步投递依赖 `RabbitMqOptions:*` 和 Outbox。

README 中不直接提供敏感连接串默认值，建议通过本地环境变量或 `appsettings.Development.json` 覆盖。

## 开发入口

```bash
dotnet run --project src/Services/AdminIdentityService/Explore.AdminIdentityService.Api
```

默认本地 HTTP 地址：

```text
http://localhost:5229
```

开发环境下可用：

- 健康检查：`/health`
- OpenAPI / Scalar：随开发环境启动一起暴露

## 测试入口

```bash
dotnet test tests/Services/AdminIdentityService/Explore.AdminIdentityService.Application.Tests/Explore.AdminIdentityService.Application.Tests.csproj
```

## 关联文档

- [../../../README.md](../../../README.md)
- [../README.md](../README.md)
- [../../../tests/README.md](../../../tests/README.md)
- [../../../docs/admin-identity-rbac-frontend-integration.md](../../../docs/admin-identity-rbac-frontend-integration.md)
