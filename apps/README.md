# Apps

`apps/` 存放所有前端应用。当前仓库包含一个管理后台和一个客户侧 uni-app 应用，它们共用网关作为后端统一入口，但开发节奏和运行方式不同。

## 应用列表

| 应用 | 目标用户 | 技术栈 | 文档 |
| --- | --- | --- | --- |
| `admin-web` | 管理员 / 运营 | Vue 3、Vite、Pinia、Element Plus | [apps/admin-web/README.md](admin-web/README.md) |
| `customer-miniapp` | 客户侧用户 | uni-app、Vue 3、TypeScript | [apps/customer-miniapp/README.md](customer-miniapp/README.md) |

## 共性约定

- 两个前端都通过 `VITE_GATEWAY_BASE_URL` 访问网关。
- 管理端更偏内部后台，直接使用浏览器开发体验。
- 客户端应用以 uni-app 为核心，除了 H5 调试，也保留小程序方向的命令入口。
- 前端各自独立管理依赖，没有在仓库根目录统一做 workspace 聚合。

## 快速选择

- 想做管理后台页面、权限菜单、运营功能，进入 [apps/admin-web/README.md](admin-web/README.md)。
- 想做登录、资料页、小程序端交互，进入 [apps/customer-miniapp/README.md](customer-miniapp/README.md)。
- 想先把后端和基础设施跑起来，先看 [../docker/README.md](../docker/README.md)。
