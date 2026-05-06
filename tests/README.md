# Tests

`tests/` 目录主要存放后端测试项目，按基础模块和业务服务分组。前端测试不放在这里，而是跟随各自应用放在 `apps/` 目录内。

## 目录组织

| 路径 | 说明 |
| --- | --- |
| `tests/BuildingBlocks` | 基础模块测试 |
| `tests/Services/AdminIdentityService` | 管理员身份服务测试 |
| `tests/Services/CustomerAccountService` | 客户账户服务测试 |
| `tests/Services/MessageCenterService` | 消息中心服务测试 |

## 常用命令

执行整个 .NET 测试集：

```bash
dotnet test Explore.slnx
```

执行单个服务测试：

```bash
dotnet test tests/Services/AdminIdentityService/Explore.AdminIdentityService.Application.Tests/Explore.AdminIdentityService.Application.Tests.csproj
dotnet test tests/Services/CustomerAccountService/Explore.CustomerAccountService.Application.Tests/Explore.CustomerAccountService.Application.Tests.csproj
dotnet test tests/Services/MessageCenterService/Explore.MessageCenterService.Application.Tests/Explore.MessageCenterService.Application.Tests.csproj
```

执行基础模块测试：

```bash
dotnet test tests/BuildingBlocks/Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests/Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests.csproj
```

前端相关检查请在应用目录执行：

```bash
cd apps/admin-web
npm test
```

```bash
cd apps/customer-miniapp
npm run type-check
```

## 外部依赖说明

- 部分测试只依赖内存或轻量级本地组件。
- 涉及数据库、Redis、RabbitMQ 的联调或集成验证时，建议先参考 [../docker/README.md](../docker/README.md) 启动基础设施。
- 仓库当前提供的是基础设施编排说明，不假定所有测试都自动拉起外部依赖。

## 关联文档

- [../README.md](../README.md)
- [../src/Services/README.md](../src/Services/README.md)
- [../docker/README.md](../docker/README.md)
