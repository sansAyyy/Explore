# Explore.Gateway.Api

`Explore.Gateway.Api` 是 Explore 的统一 API Gateway，前端默认通过它访问后端服务。它负责把外部请求转发到内部服务，并在入口层完成通用控制。

## 职责

- 基于 YARP 做反向代理
- 统一 JWT 鉴权与授权策略
- 统一 CORS 配置
- 暴露健康检查
- 在开发环境下暴露 OpenAPI

## 路由概览

当前主要代理以下服务前缀：

- `/admin-identity/*`
- `/customer-account/*`
- `/message-center/*`

实际目标地址由 `ReverseProxy` 配置决定，本地开发默认指向各服务的本机端口。

## 本地开发入口

```bash
dotnet run --project src/Gateways/Explore.Gateway.Api
```

默认本地 HTTP 地址来自 `launchSettings.json`：

```text
http://localhost:5203
```

## 核心配置

重点关注以下配置段：

- `Cors:AllowedOrigins`
- `JwtOptions`
- `ReverseProxy:Routes`
- `ReverseProxy:Clusters`
- `LoggingOptions`

建议在本地环境中通过 `appsettings.Development.json`、环境变量或容器变量覆盖，而不是直接修改通用默认值。

## 开发期可观测入口

- 健康检查：`/health`
- 开发环境 OpenAPI：`/openapi/*`

## 关联文档

- [../../../README.md](../../../README.md)
- [../../README.md](../../README.md)
- [../../Services/README.md](../../Services/README.md)
