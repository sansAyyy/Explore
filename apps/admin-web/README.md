# admin-web

`admin-web` 是 Explore 的管理后台前端，面向内部管理员使用，当前覆盖认证、权限、客户管理与消息模板等后台能力。

## 当前功能范围

- 登录与会话持久化
- 基于权限的路由守卫与菜单可见性控制
- 仪表盘入口
- 管理员、角色、权限管理页面
- 客户账户管理页面
- 消息模板管理页面

## 开发脚本

在 `apps/admin-web` 目录下执行：

```bash
npm install
npm run dev
```

```bash
npm run build
```

```bash
npm test
```

## 环境变量

当前主要使用以下前端环境变量：

```bash
VITE_GATEWAY_BASE_URL=http://localhost:5203
```

说明：

- 管理端通过网关访问后端，而不是直接访问各服务。
- 未配置时，代码默认回退到 `http://localhost:5203`。
- 若通过 Docker 构建镜像，`VITE_GATEWAY_BASE_URL` 属于构建时注入参数。
- 建议从 `apps/admin-web/.env.example` 或 `apps/admin-web/.env.development.example` 复制出本地文件，再填写自己的环境地址。

## 运行方式

### 本地开发

推荐搭配本地运行的网关：

```bash
cd apps/admin-web
npm install
npm run dev
```

如果网关运行在默认端口，请确保：

```bash
VITE_GATEWAY_BASE_URL=http://localhost:5203
```

### Docker 构建

从仓库根目录执行：

```bash
docker build -f apps/admin-web/docker/Dockerfile -t admin-web --build-arg VITE_GATEWAY_BASE_URL=http://localhost:5203 apps/admin-web
docker run --rm -p 8080:80 admin-web
```

### Compose 联调

推荐使用根目录的开发编排，而不是旧文档中的相对路径写法：

```bash
docker compose -f docker/compose.dev.yaml up --build -d admin-web gateway
```

如果要拉起整套联调环境：

```bash
docker compose -f docker/compose.dev.yaml up --build -d
```

## 依赖的后端能力

- `Explore.Gateway.Api`
- `AdminIdentityService`
- `CustomerAccountService`
- `MessageCenterService`

默认情况下，浏览器只需要访问网关地址，网关再将请求转发到各服务。

## 初始化管理员

仓库不再公开固定演示账号或口令。

如果你保留默认数据库迁移中的引导管理员，请在本地环境初始化后立即修改用户名、邮箱和密码；如果你已有自己的初始化方案，请以部署环境配置为准。

## 关联文档

- [../../README.md](../../README.md)
- [../README.md](../README.md)
- [../../docker/README.md](../../docker/README.md)
- [../../docs/admin-identity-rbac-frontend-integration.md](../../docs/admin-identity-rbac-frontend-integration.md)
