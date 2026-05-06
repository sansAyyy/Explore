# MessageCenterService

`MessageCenterService` 负责消息模板与通知发送相关能力，并通过收件人目录配置决定如何解析通知目标。它既可以被其他服务直接调用，也可以消费异步通知消息；当前站内信通道已经落库，短信与小程序通道仍保留 placeholder 发送器。

## 目录结构

- `Explore.MessageCenterService.Api`：HTTP 接口、OpenAPI、宿主配置
- `Explore.MessageCenterService.Application`：应用层用例、通知编排
- `Explore.MessageCenterService.Domain`：领域模型与业务规则
- `Explore.MessageCenterService.Infrastructure`：数据库、Inbox、RabbitMQ 消费者、收件人目录与通道发送器实现

## 当前通知入口

- 同步入口：`POST /api/notifications/send`，适合被其他服务通过 HTTP 直接调用。
- 异步入口：消费 `SendNotificationByTemplateMessage`，并用 `IInboxMessageProcessor` 做幂等处理。
- 站内信能力：通过 `/api/site-messages` 提供当前登录管理员自己的站内信分页查询、详情、单条已读和全部已读接口。
- 模板管理：通过 `/api/message-templates` 管理模板及启停状态。
- 当前通道实现状态：`SiteMessage` 会落库；`Sms` 与 `MiniProgram` 发送器当前返回 placeholder 状态，等待真实 provider 接入。

## 依赖注入约定

- `Program.cs` 只调用一次 `AddModules(builder.Configuration)`，统一完成模块发现与服务注册。
- `Application` 层的应用服务实现 `IScopeDependency` 后会自动注入，因此 `Explore.MessageCenterService.Application.DependencyInjection` 当前不需要手写注册逻辑。
- `Infrastructure.DependencyInjection` 只保留 `DbContext`、RabbitMQ、Inbox、JWT 和 Recipient Directory Typed HttpClient 这类需要显式参数的注册。

## 运行依赖

- PostgreSQL
- RabbitMQ
- Recipient Directory HTTP 接口或 Stub 配置

## 核心配置

重点关注以下配置项：

- `ConnectionStrings:MessageCenterDatabase`
- `RabbitMqOptions:*`
- `RecipientDirectoryOptions:*`
- `JwtOptions:*`
- `LoggingOptions:*`

其中 `RecipientDirectoryOptions` 主要用于短信 / 小程序这类需要补全接收地址的场景；站内信通道只要求能解析出 `RecipientUserId`。

## 开发入口

```bash
dotnet run --project src/Services/MessageCenterService/Explore.MessageCenterService.Api
```

默认本地 HTTP 地址：

```text
http://localhost:5013
```

开发环境下可用：

- 健康检查：`/health`
- OpenAPI / Scalar：随开发环境启动一起暴露

## 测试入口

```bash
dotnet test tests/Services/MessageCenterService/Explore.MessageCenterService.Application.Tests/Explore.MessageCenterService.Application.Tests.csproj
```

## 关联文档

- [../../../README.md](../../../README.md)
- [../README.md](../README.md)
- [../../../tests/README.md](../../../tests/README.md)
