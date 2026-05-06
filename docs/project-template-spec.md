这套规范是为 **“一个 Solution + 多项目 + 多微服务”** 量身定制的，偏向**工程化、可维护、团队协作**。

---

# 一、总体原则（先记住这 5 条） 
1. **Building Block ≠ 业务代码**
2. **Building Block 只做横切关注点**
3. **抽象与实现分离**
4. **服务只依赖抽象，不依赖实现**
5. **文件夹 = 意图，而不是技术**

---

# 二、Solution 结构规范 
```plain
src/
├── BuildingBlocks/          # ✅ 所有基础模块
├── Services/                # ✅ 所有微服务
├── Shared/                  # ⚠️ 仅用于跨服务契约（慎用）
├── Tests/                   # ✅ 测试
├── Samples/                 # ✅ 示例 / Demo
└── YourSolution.slnx
```

### ✅ 禁止项 
+ ❌ 根目录直接放 Service 项目
+ ❌ 根目录放 Utils / Common / Helpers 项目

---

# 三、Building Blocks 项目命名规范 
### ✅ 命名格式 
```plain
BuildingBlocks.[能力]
```

### ✅ 示例 
```plain
BuildingBlocks.Common
BuildingBlocks.CurrentUser
BuildingBlocks.Messaging.Abstractions
BuildingBlocks.Messaging.RabbitMQ
BuildingBlocks.Security
BuildingBlocks.Caching
BuildingBlocks.Logging
BuildingBlocks.Telemetry
BuildingBlocks.Http
BuildingBlocks.Api
BuildingBlocks.HealthChecks
```

### ❌ 禁止命名 
+ ❌ CommonLibrary
+ ❌ SharedKernel
+ ❌ Utils
+ ❌ Helpers

---

# 四、Building Block 内部文件夹规范（强制） 
**所有 Building Block 必须遵循此结构**

```plain
BuildingBlocks.Xxx/
├── Abstractions/        # ✅ 接口、抽象类（对外暴露）
├── Options/             # ✅ 强类型配置
├── Extensions/          # ✅ DI / 中间件 / 注册入口
├── Middlewares/         # ✅ HTTP 中间件（如有）
├── Exceptions/          # ✅ 自定义异常 / 错误
├── Internal/            # ❌ 实现细节（internal）
├── [Models|Events]      # ✅ 只读模型 / 事件
└── BuildingBlocks.Xxx.csproj
```

### ✅ 文件夹含义速查表 
| 文件夹 | 是否对外 | 用途 |
| --- | --- | --- |
| Abstractions | ✅ | 对外契约 |
| Options | ✅ | IOptions |
| Extensions | ✅ | 服务注册 |
| Middlewares | ✅ | 管道行为 |
| Exceptions | ✅ | 错误定义 |
| Internal | ❌ | 实现细节 |
| Models / Events | ✅ | 只读数据结构 |


### ✅ 示例 
以 `BuildingBlocks.CurrentUser`为例：

```plain
BuildingBlocks.CurrentUser/
├── Abstractions/
│   └── ICurrentUser.cs
├── Options/
│   └── CurrentUserOptions.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── Middlewares/
│   └── CurrentUserMiddleware.cs
├── Exceptions/
│   └── MissingUserClaimException.cs
├── Internal/
│   └── CurrentUserAccessor.cs
├── CurrentUser.cs
└── BuildingBlocks.CurrentUser.csproj
```

---

## ✅ Messaging 这种“抽象 + 实现”的拆分方式 
```plain
BuildingBlocks.Messaging/
├── Abstractions/
│   ├── IEventBus.cs
│   ├── IIntegrationEvent.cs
│   └── IEventSubscriber.cs
├── Options/
│   └── EventBusOptions.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── BuildingBlocks.Messaging.Abstractions.csproj
```

```plain
BuildingBlocks.Messaging.RabbitMQ/
├── RabbitMqEventBus.cs
├── RabbitMqOptions.cs
├── Internal/
│   └── ConnectionFactoryBuilder.cs
└── BuildingBlocks.Messaging.RabbitMQ.csproj
```

✅ **服务只引用 Abstractions**

✅ **实现可替换**

---

# 五、Building Block 代码规范 
### ✅ 1️⃣ 所有对外能力必须通过接口 
```plain
public interface ICurrentUser { }
public interface IEventBus { }
public interface IDistributedCacheService { }
```

### ✅ 2️⃣ 所有配置必须使用 Options 模式 
```plain
public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
```

### ✅ 3️⃣ 所有注册必须通过 Extensions 
```plain
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCurrentUser(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        return services;
    }
}
```

### ✅ 4️⃣ 实现类必须 internal（如可能） 
```plain
internal class RabbitMqEventBus : IEventBus
{
}
```

---

# 六、Services（微服务）结构规范 
### 1 Domain
推荐按业务对象组织：

```latex
Domain/
  AdminUsers/
    AdminUser.cs  ✅充血模型
    ValueObjects/
    Events/
  Shared/
```

### 2 Application
推荐按功能组织，但由 `Application Service` 统一编排：

```latex
Application/
  Features/
    AdminUsers/
      Services/
      Dtos/
        Requests/
        Responses/
      Validators/
  Abstractions/
  Contracts/
  DependencyInjection.cs
```

### 3 Infrastructure
```latex
Infrastructure/
  Persistence/
    Configurations/
    Repositories/
      Commands/
      Queries/
    Migrations/
  Messaging/
  Integrations/
  Services/
  DependencyInjection.cs
```

### 4 Api
```latex
Api/
  Controllers/
```

### ✅ 引用规则 
+ ✅ 只引用 BuildingBlocks.Abstractions
+ ❌ 不引用 BuildingBlocks.RabbitMQ
+ ❌ 不在 Domain 中引用 BuildingBlocks

---

# 七、Shared 文件夹使用规范（严格限制） 
```plain
Shared/
└── Contracts/
    └── IntegrationEvents/
```

### ✅ 允许 
+ 跨服务 Integration Event
+ 枚举
+ 公共 DTO（只读）

### ❌ 禁止 
+ EF Core 实体
+ 业务逻辑
+ 服务特有配置

**Shared 是“最后的妥协”，不是设计目标**

---

# 八、依赖关系规范（非常重要） 
```plain
Service
  ↓（只依赖）
BuildingBlocks.Abstractions
  ↓
BuildingBlocks.Implementation（通过 DI 注入）
```

### ✅ 正确 
```plain
builder.Services.AddRabbitMqEventBus();
```

### ❌ 错误 
```plain
builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();
```

---

# 九、禁止事项（红线） 
❌ 在 Building Blocks 中写业务规则

❌ 在 Building Blocks 中引用 EF Core

❌ 在 Building Blocks 中放 Controller

❌ 使用 Utils / Helpers / Common 文件夹

❌ 让服务直接依赖具体实现

❌ 在 Building Block 中放 Swagger / Scalar（除非是 OpenApi 扩展包）

---

# 十、一句话总结 
**Building Blocks 只做“基础设施和横切关注点”，

按职责划分、接口对外、实现隐藏、服务只依赖抽象；

微服务按 Api / Application / Domain / Infrastructure 分层，

不依赖实现，不乱用 Shared。**