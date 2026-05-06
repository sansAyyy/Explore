# AdminIdentityService 前端权限对接说明

本文档约定管理端前端如何对接 `AdminIdentityService` 的权限模型，以及哪些页面只依赖登录态、不参与 RBAC 页面权限判断。

当前权限模型包含三类资源：

- `Page = 1`：页面权限
- `Button = 2`：按钮权限
- `Group = 3`：分组节点，仅用于组织权限树结构，不参与页面或按钮授权判断

前端基于页面权限和按钮权限做界面控制；分组节点由后端显式返回，不再需要前端维护分组权限 code 白名单。

## 1. 基本原则

1. 登录成功后，只保存 token 和基础用户信息，不假设登录响应里包含完整权限。
2. 登录成功后，立即调用 `GET /api/admin-authorization/current` 拉取当前管理员授权上下文。
3. 页面可见、路由可进入、按钮显隐，统一基于权限码集合判断。
4. 按钮权限仅用于前端交互控制，不是实际安全边界。
5. 当前登录管理员的个人中心是免权限页面，只要登录态有效即可访问。
6. 角色或权限被后端修改后，不要求重新登录；前端重新拉取授权上下文即可拿到最新权限。

## 2. 鉴权相关接口

### `POST /api/admin-auth/login`

登录成功后返回 token 信息。

### `GET /api/admin-authorization/current`

返回当前管理员授权上下文：

```json
{
  "userId": "00000000-0000-0000-0000-000000000001",
  "userName": "bootstrap-admin",
  "displayName": "Bootstrap Admin",
  "roleCodes": [
    "super_admin"
  ],
  "permissionCodes": [
    "admin_permissions.create",
    "admin_permissions.page",
    "admin_roles.page"
  ],
  "pagePermissionCodes": [
    "admin_permissions.page",
    "admin_roles.page"
  ],
  "buttonPermissionCodes": [
    "admin_permissions.create"
  ]
}
```

字段约定：

- `permissionCodes`：当前管理员拥有的全部权限码集合，包含页面权限和按钮权限
- `pagePermissionCodes`：页面权限码集合，前端路由和菜单主要使用它
- `buttonPermissionCodes`：按钮权限码集合，前端按钮显隐主要使用它

### `GET /api/admin-current-user`

返回当前管理员基础资料，可用于个人中心展示。

### `PUT /api/admin-current-user`

更新当前管理员自己的基础资料。

### `PUT /api/admin-current-user/password`

更新当前管理员自己的登录密码。

## 3. 推荐前端流程

### 登录后

1. 调用登录接口获取 token。
2. 保存 `accessToken`、`refreshToken`、过期时间和基础用户身份。
3. 立刻调用 `GET /api/admin-authorization/current`。
4. 将 `roleCodes`、`permissionCodes`、`pagePermissionCodes`、`buttonPermissionCodes` 存入前端状态管理。
5. 根据页面权限初始化菜单、路由守卫和首页跳转。

### 刷新页面后

1. 先恢复本地 token。
2. 使用 token 调用 `GET /api/admin-authorization/current`。
3. 拉取成功后再渲染受权限控制的页面。

不要只信任本地缓存的权限集合。权限可能已经在服务端发生变化，重新拉取才能保证即时生效。

## 4. 页面权限接入方式

前端自己维护“页面路由 -> 页面权限码”的本地映射，例如：

```ts
const pagePermissionMap: Record<string, string> = {
  '/admin-permissions': 'admin_permissions.page',
  '/system/admin-roles': 'admin_roles.page',
}
```

约定：

- 没有页面权限时，不显示菜单入口。
- 没有页面权限时，即使用户手动输入路由，也不允许进入页面。
- 个人中心这类登录后即可访问的页面，不配置 `pagePermission`，只要求 `requiresAuth = true`。

示例判断：

```ts
function canAccessRoute(routePath: string, pagePermissionCodes: string[]) {
  const permissionCode = pagePermissionMap[routePath]
  if (!permissionCode) {
    return true
  }

  return pagePermissionCodes.includes(permissionCode)
}
```

## 5. 按钮权限接入方式

前端自己维护“按钮节点 -> 按钮权限码”的本地映射。

约定：

- 没有按钮权限时，不展示按钮。
- 没有按钮权限时，不开放对应的前端交互入口。
- 按钮权限只负责前端显隐，不代表拥有页面内全部操作能力。
- 当前管理员自己的资料维护与改密，不再依赖 `current_admin.*` 这类按钮权限码，统一按“登录即可访问”处理。

## 6. 免权限页面约定

以下页面属于“已登录即可访问”，不纳入 RBAC 页面权限控制：

- `/profile`：个人中心

这类页面仍然需要：

- `meta.requiresAuth = true`
- 登录失效时被路由守卫重定向到登录页

这类页面不需要：

- `meta.pagePermission`
- 菜单权限映射
- 按钮权限码映射

## 7. 刷新权限时机

建议在以下场景重新调用 `GET /api/admin-authorization/current`：

1. 登录成功后
2. 页面刷新并恢复会话后
3. 当前登录管理员的角色或权限发生变更后

## 8. 命名约定

页面权限码示例：

- `dashboard.view`
- `admin_permissions.page`

按钮权限码示例：

- `admin_permissions.create`
- `admin_roles.assign_permissions`

## 9. 不建议做法

1. 不要把角色直接映射成页面权限，尽量只认权限码。
2. 不要把菜单树写死成“角色 -> 菜单”，应改为“页面权限码 -> 菜单项”。
3. 不要把后端返回的 `permissionCodes` 当成菜单数据源，菜单仍由前端本地配置维护。
4. 不要假设拥有页面权限就一定拥有全部按钮权限。

## 10. 联调检查清单

1. 登录后能成功获取 token。
2. 使用 token 调用 `GET /api/admin-authorization/current`，确认能拿到角色和权限集合。
3. 没有页面权限的路由会被守卫拦截。
4. 没有按钮权限的交互入口不会显示。
5. 访问 `/profile` 时，只校验登录态，不校验页面权限。
6. 角色或权限变更后，重新调用 `GET /api/admin-authorization/current` 能立即生效。

## 11. 相关文件

- `src/Services/AdminIdentityService/Explore.AdminIdentityService.Api/Explore.AdminIdentityService.Api.http`
- `apps/admin-web/src/stores/auth.ts`
- `apps/admin-web/src/router/guards.ts`
- `apps/admin-web/src/features/current-admin/pages/ProfilePage.vue`
