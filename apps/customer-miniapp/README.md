# customer-miniapp

`customer-miniapp` 是 Explore 的客户侧 uni-app 应用，当前重点覆盖手机号登录、个人资料查看与资料编辑能力。它既可以作为 H5 应用调试，也保留了面向小程序平台的开发命令。

## 当前功能范围

基于 `src/pages.json`，当前主要页面包括：

- 首页：`pages/home/index`
- 手机号登录：`features/customer-account/pages/login/index`
- 个人资料：`features/customer-account/pages/profile/index`
- 编辑资料：`features/customer-account/pages/edit/index`

## 常用命令

在 `apps/customer-miniapp` 目录下执行：

```bash
npm install
npm run dev:h5
```

```bash
npm run build:h5
```

```bash
npm run dev:mp-weixin
```

```bash
npm run build:mp-weixin
```

```bash
npm run type-check
```

## 环境变量

主要环境变量：

```bash
VITE_GATEWAY_BASE_URL=http://localhost:5203
```

说明：

- 代码未配置时会默认回退到 `http://localhost:5203`。
- 建议通过网关访问后端，而不是直接连具体服务。
- 构建 H5 或容器镜像时，需要确认网关地址与部署环境一致。
- 建议从 `apps/customer-miniapp/.env.example` 或 `apps/customer-miniapp/.env.development.example` 复制出本地文件，再填写自己的环境地址。

## 运行方式

### H5 本地调试

```bash
cd apps/customer-miniapp
npm install
npm run dev:h5
```

### 微信小程序方向调试

```bash
npm run dev:mp-weixin
```

### Compose 联调

如需通过容器暴露 H5 版本，可使用：

```bash
docker compose -f docker/compose.dev.yaml up --build -d customer-miniapp gateway
```

## 依赖的后端能力

- `Explore.Gateway.Api`
- `CustomerAccountService`
- `MessageCenterService`

其中登录、资料查询和资料更新主要依赖账户服务，短信 / 通知相关能力会间接依赖消息中心。

## 构建产物预期

- `build:h5`：生成 H5 站点产物，适合浏览器或静态部署场景。
- `build:mp-weixin`：生成微信小程序方向产物，供后续在开发者工具中继续集成。

## 关联文档

- [../../README.md](../../README.md)
- [../README.md](../README.md)
- [../../docker/README.md](../../docker/README.md)
