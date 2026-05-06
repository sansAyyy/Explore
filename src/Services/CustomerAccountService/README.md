# CustomerAccountService

`CustomerAccountService` 负责客户账户相关能力，包括手机号登录、账户资料读取与更新，以及客户端登录态依赖的基础账户接口。

## 目录结构

- `Explore.CustomerAccountService.Api`：HTTP 接口、OpenAPI、宿主配置
- `Explore.CustomerAccountService.Application`：应用层用例与编排逻辑
- `Explore.CustomerAccountService.Domain`：领域模型与业务规则
- `Explore.CustomerAccountService.Infrastructure`：持久化、缓存及外部依赖实现

## BuildingBlocks 依赖约定

- `Application` 只依赖抽象型模块，例如 `BuildingBlocks.Caching.Abstractions`。
- `Infrastructure` 负责接入实现型模块，例如 `BuildingBlocks.Caching.Redis`、`BuildingBlocks.DistributedLocking.Redis`。
- 手机号验证码等安全能力继续经由 `BuildingBlocks.Security` 复用缓存抽象。

## 运行依赖

- PostgreSQL
- Redis
- Message Center HTTP 接口

## 核心配置

重点关注以下配置项：

- `ConnectionStrings:CustomerAccountDatabase`
- `ConnectionStrings:Redis`
- `MessageCenter:*`
- `JwtOptions:*`
- `PhoneVerificationCode:*`
- `LoggingOptions:*`

## 开发入口

```bash
dotnet run --project src/Services/CustomerAccountService/Explore.CustomerAccountService.Api
```

默认本地地址来自 `launchSettings.json`：

```text
http://localhost:5230
```

开发环境下可用：

- 健康检查：`/health`
- OpenAPI / Scalar：随开发环境启动一起暴露

## 测试入口

```bash
dotnet test tests/Services/CustomerAccountService/Explore.CustomerAccountService.Application.Tests/Explore.CustomerAccountService.Application.Tests.csproj
```

## 关联文档

- [../../../README.md](../../../README.md)
- [../README.md](../README.md)
- [../../../tests/README.md](../../../tests/README.md)
- [../../../apps/customer-miniapp/README.md](../../../apps/customer-miniapp/README.md)
