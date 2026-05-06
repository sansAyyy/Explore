# Src

`src/` 存放后端源码，按“基础能力、共享契约、接入层、业务服务”拆分，目标是让业务代码和横切能力边界清晰。

## 结构分层

| 路径 | 角色 |
| --- | --- |
| `BuildingBlocks/` | 通用基础能力，不只是工具类集合，还覆盖认证与安全、Correlation / CurrentUser、自动依赖注入、领域抽象、持久化审计、OpenAPI、消息抽象与 RabbitMQ / EF Core 集成 |
| `Contracts/` | 跨服务共享契约，避免业务实现直接互相依赖 |
| `Gateways/` | 外部接入层，当前为统一 API Gateway |
| `Services/` | 按业务边界拆分的微服务实现 |

## 当前后端组成

- `Gateways/Explore.Gateway.Api`：统一入口，负责路由转发与访问控制。
- `Services/AdminIdentityService`：管理员体系、认证授权。
- `Services/CustomerAccountService`：客户账户、登录、资料。
- `Services/MessageCenterService`：消息模板、通知能力。

## BuildingBlocks 概览

当前 `BuildingBlocks` 已拆分为 14 个模块，分别承担不同横切职责，例如：

- `BuildingBlocks.Common`：统一结果模型、分页模型、出站 HTTP resilience 与 CorrelationId 透传
- `BuildingBlocks.Security`：JWT 鉴权、Token 生成、密码哈希、手机号验证码
- `BuildingBlocks.Messaging.*`：消息抽象、RabbitMQ 集成、EF Core Inbox / Outbox
- `BuildingBlocks.Persistence.EntityFrameworkCore`：审计字段、软删除、并发版本
- `BuildingBlocks.DependencyInjection`：模块发现与自动注册

详细模块能力、显式 NuGet 依赖、内部引用关系和服务消费方见 [src/BuildingBlocks/README.md](BuildingBlocks/README.md)。

## 分层约定

- 业务服务目录通常采用 `Api`、`Application`、`Domain`、`Infrastructure` 四层。
- `BuildingBlocks` 只承载横切能力，不承载具体业务规则。
- 跨服务共享内容优先进入 `Contracts`，避免服务之间直接引用彼此实现。
- 网关负责统一暴露入口，前端原则上经由网关访问后端。

## 继续阅读

- BuildingBlocks 详细说明： [BuildingBlocks/README.md](BuildingBlocks/README.md)
- 服务边界与依赖关系： [Services/README.md](Services/README.md)
- 网关说明： [Gateways/Explore.Gateway.Api/README.md](Gateways/Explore.Gateway.Api/README.md)
- 根目录导航： [../README.md](../README.md)
