# Docker

`docker/` 目录用于本地开发和测试基础设施编排。当前主要有两类 Compose 文件：一类启动整套开发环境，一类只启动数据库和中间件。

## 文件说明

| 文件 | 用途 |
| --- | --- |
| `compose.dev.yaml` | 启动前端、网关和 3 个后端服务，适合完整联调 |
| `compose.dev-infra.yaml` | 只启动 PostgreSQL、Redis、RabbitMQ，适合本地运行应用代码 |
| `caddy/Caddyfile` | Development 域名入口配置，负责 Host 转发和通过 Cloudflare DNS-01 自动 HTTPS |
| `caddy/Dockerfile` | 基于官方 Caddy 镜像构建，内置 `dns.providers.cloudflare` 插件 |
| `env/dev.domain.env.example` | Development 域名化部署示例变量，包含前端域名、API 域名、证书邮箱和 Cloudflare Token |
| `env/dev-infra.env.example` | 测试基础设施环境变量示例 |
| `postgres/init/01-init-multiple-databases.sh` | PostgreSQL 初始化多个数据库 |

## 推荐启动顺序

### 方案 1：只起基础设施，代码在本地跑

```bash
cp docker/env/dev-infra.env.example docker/env/dev-infra.env
docker compose --env-file docker/env/dev-infra.env -f docker/compose.dev-infra.yaml up -d
```

然后在本地分别运行服务、网关和前端。这是日常开发最灵活的方式。

### 方案 2：整套容器联调

```bash
docker compose -f docker/compose.dev.yaml up --build -d
```

适合快速验证整体链路、部署脚本或跨端联调。

### 方案 3：Development 域名化接入

对外统一使用以下域名：

- `https://admin.example.com`：管理后台
- `https://app.example.com`：客户侧 H5
- `https://api.example.com`：统一 API 网关

前置条件：

- `admin.example.com`、`app.example.com`、`api.example.com` 只是示例域名；你需要替换成自己控制的真实域名，并把 A 记录指向 Development 服务器公网 IP。
- Cloudflare 中管理这些域名的 Zone。
- 一个 Cloudflare API Token，至少具备对应 Zone 的 `Zone.Zone:Read` 和 `Zone.DNS:Edit` 权限。
- 可以提供一个可接收证书通知的邮箱，填入 `CADDY_EMAIL`。
- 服务器对外提供 `80/443`。其中 `443` 用于 HTTPS 服务，`80` 主要用于 HTTP 跳转到 HTTPS；证书签发本身改走 DNS-01，不再依赖 HTTP-01。

推荐做法：

```bash
cp docker/env/dev.domain.env.example docker/env/dev.domain.env
docker compose --env-file docker/env/dev.domain.env -f docker/compose.dev.yaml up --build -d
```

该编排会启动 `edge-proxy`，由 Caddy 负责：

- 按 Host 转发到 `admin-web`、`customer-miniapp`、`gateway`
- 通过 Cloudflare DNS-01 自动签发与续期 HTTPS 证书
- 自动将 HTTP 跳转到 HTTPS
- 站点配置文件位于 `docker/caddy/Caddyfile`
- Caddy 镜像由 `docker/caddy/Dockerfile` 本地构建，内置 Cloudflare DNS provider

## 环境变量分组

文档里只保留变量名与示例，不直接复用仓库中的敏感默认值。

### 前端与网关

```bash
VITE_GATEWAY_BASE_URL=https://api.example.com
ADMIN_WEB_PUBLIC_URL=https://admin.example.com
CUSTOMER_MINIAPP_PUBLIC_URL=https://app.example.com
ADMIN_WEB_DOMAIN=admin.example.com
CUSTOMER_MINIAPP_DOMAIN=app.example.com
GATEWAY_DOMAIN=api.example.com
CADDY_EMAIL=ops@example.com
CLOUDFLARE_API_TOKEN=replace-with-your-cloudflare-api-token
ADMIN_WEB_PORT=8080
CUSTOMER_MINIAPP_PORT=8081
GATEWAY_PORT=5001
```

说明：

- `VITE_GATEWAY_BASE_URL` 会在前端镜像构建时注入，修改后需要重建前端镜像。
- `CLOUDFLARE_API_TOKEN` 由 Caddy 的 Cloudflare DNS 插件读取，用于创建 `_acme-challenge` TXT 记录。
- `ADMIN_WEB_PORT`、`CUSTOMER_MINIAPP_PORT`、`GATEWAY_PORT` 仍保留，方便迁移期继续通过 `IP:端口` 调试。
- 服务间调用继续走 Docker 内网，不应把 `MessageCenter__BaseUrl` 或 `RecipientDirectoryOptions__BaseUrl` 改成公网域名。

### JWT

```bash
JWT_ISSUER=Explore
JWT_AUDIENCE=Explore.Clients
JWT_SIGNING_KEY=change-this-local-jwt-signing-key
JWT_ACCESS_TOKEN_EXPIRATION_MINUTES=120
JWT_REFRESH_TOKEN_EXPIRATION_DAYS=7
```

### PostgreSQL / Redis / RabbitMQ

```bash
POSTGRES_USER=postgres
POSTGRES_PASSWORD=change-this-dev-password
POSTGRES_PORT=5432
REDIS_PASSWORD=change-this-dev-password
REDIS_PORT=6379
RABBITMQ_USERNAME=admin
RABBITMQ_PASSWORD=change-this-dev-password
RABBITMQ_PORT=5672
```

### 服务连接串示例

```bash
ADMIN_IDENTITY_DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=explore_admin_identity_dev;Username=postgres;Password=change-this-dev-password
CUSTOMER_ACCOUNT_DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=explore_customer_account_dev;Username=postgres;Password=change-this-dev-password
MESSAGE_CENTER_DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=explore_message_center_dev;Username=postgres;Password=change-this-dev-password
```

## 常用命令

启动整套环境：

```bash
docker compose -f docker/compose.dev.yaml up --build -d
```

按域名示例变量启动：

```bash
docker compose --env-file docker/env/dev.domain.env -f docker/compose.dev.yaml up --build -d
```

只重建单个服务：

```bash
docker compose -f docker/compose.dev.yaml up --build -d message-center-service
```

启动测试基础设施：

```bash
docker compose --env-file docker/env/dev-infra.env -f docker/compose.dev-infra.yaml up -d
```

停止并清理：

```bash
docker compose -f docker/compose.dev.yaml down
docker compose -f docker/compose.dev-infra.yaml down
```

查看 Caddy 日志：

```bash
docker compose -f docker/compose.dev.yaml logs -f edge-proxy
```

## 使用建议

- 日常开发优先用 `compose.dev-infra.yaml`，把应用跑在本地，调试成本更低。
- 需要验证容器网络、端口映射或前后端联调链路时，再使用 `compose.dev.yaml`。
- 域名化 Development 环境先复制 `env/dev.domain.env.example`，再通过 `edge-proxy` 暴露 `80/443`。
- Caddy 证书和运行配置保存在 Compose 命名卷 `caddy_data`、`caddy_config` 中，重建容器不会丢失已签发证书。
- 如果 Cloudflare Token 权限不对，或环境变量未注入，Caddy 会在证书申请阶段失败，先检查 `edge-proxy` 日志。
- 如果修改了前端构建参数，例如 `VITE_GATEWAY_BASE_URL`，需要重新构建对应镜像。

## 关联文档

- [../README.md](../README.md)
- [../apps/README.md](../apps/README.md)
- [../src/Services/README.md](../src/Services/README.md)
