# Services

`src/Services/` 存放按业务边界拆分的服务实现。每个服务内部继续按 `Api / Application / Domain / Infrastructure` 分层，测试则放在 `tests/Services/` 下按服务对应组织。

## 服务矩阵

| 服务 | 核心职责 | 主要运行依赖 | 主要 BuildingBlocks | 测试位置 | 文档 |
| --- | --- | --- | --- | --- | --- |
| `AdminIdentityService` | 管理员登录、授权上下文、角色权限 | PostgreSQL、Redis、RabbitMQ、MessageCenter HTTP API | `Correlation.AspNetCore`、`Logging`、`OpenApi`、`DependencyInjection`、`Caching.*`、`CurrentUser`、`DistributedLocking.Redis`、`Common.*`、`Security.*`、`Messaging.Outbox.*`、`Messaging.RabbitMQ`、`Persistence.*`、`Domain` | `tests/Services/AdminIdentityService` | [AdminIdentityService/README.md](AdminIdentityService/README.md) |
| `CustomerAccountService` | 客户账户、手机号登录、资料维护 | PostgreSQL、Redis、MessageCenter HTTP API | `Correlation.AspNetCore`、`Logging`、`OpenApi`、`DependencyInjection`、`Caching.*`、`CurrentUser`、`DistributedLocking.Redis`、`Common.*`、`Security.*`、`Persistence.*`、`Domain` | `tests/Services/CustomerAccountService` | [CustomerAccountService/README.md](CustomerAccountService/README.md) |
| `MessageCenterService` | 消息模板、通知投递、收件人目录对接 | PostgreSQL、RabbitMQ、RecipientDirectory HTTP API | `Correlation.AspNetCore`、`Logging`、`OpenApi`、`DependencyInjection`、`Common.*`、`CurrentUser`、`Security.*`、`Messaging.Inbox.*`、`Messaging.RabbitMQ`、`Persistence.*`、`Domain` | `tests/Services/MessageCenterService` | [MessageCenterService/README.md](MessageCenterService/README.md) |

## BuildingBlocks 消费关系

- API 层普遍依赖 `Correlation.AspNetCore`、`Logging`、`OpenApi`，并通过 `AddModules(builder.Configuration)` 完成模块发现与注册。
- Application / Infrastructure 中实现 `IScopeDependency`、`ITransientDependency`、`ISingletonDependency` 的服务会被自动注入；`MessageCenterService.Application.DependencyInjection` 当前保持空实现就是因为应用服务已经走约定注册。
- `AdminIdentityService` 的消息能力已分成两条链路：短信验证码通过 Typed HttpClient 直连 `MessageCenter`，管理员站内信通过 EF Core Outbox + RabbitMQ 异步投递。
- `MessageCenterService` 通过 RabbitMQ 消费 `SendNotificationByTemplateMessage`，并使用 EF Core Inbox 做幂等处理。
- `CustomerAccountService` 当前不直接接入 RabbitMQ，消息中心相关交互仍以 HTTP 能力为主。
- 想看每个 `BuildingBlocks.*` 模块的职责、NuGet 版本和直接消费方，进入 [../BuildingBlocks/README.md](../BuildingBlocks/README.md)。

## 依赖关系说明

- 前端优先访问网关，不直接调用服务地址。
- `AdminIdentityService` 与 `CustomerAccountService` 都会消费消息中心能力。
- `AdminIdentityService` 当前由自身写入业务数据和 Outbox 记录，再由 `MessageCenterService` 消费并落库站内信。
- `MessageCenterService` 会按配置访问收件人目录，默认场景下通常对接客户账户服务。
- 各服务复用 `BuildingBlocks` 提供的日志、认证、缓存、分布式锁、消息与持久化能力。

## 本地开发入口

```bash
dotnet run --project src/Services/AdminIdentityService/Explore.AdminIdentityService.Api
dotnet run --project src/Services/CustomerAccountService/Explore.CustomerAccountService.Api
dotnet run --project src/Services/MessageCenterService/Explore.MessageCenterService.Api
```

如需统一入口，再启动：

```bash
dotnet run --project src/Gateways/Explore.Gateway.Api
```

## 继续阅读

- 根目录导航： [../../README.md](../../README.md)
- 后端总览： [../README.md](../README.md)
- BuildingBlocks 详细说明： [../BuildingBlocks/README.md](../BuildingBlocks/README.md)
- 测试说明： [../../tests/README.md](../../tests/README.md)
