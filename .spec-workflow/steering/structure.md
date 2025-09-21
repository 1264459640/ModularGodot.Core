# 项目结构文档 - ModularGodot.Core

## 📋 目录

- [项目概述](#项目概述)
- [目录结构](#目录结构)
- [分层架构](#分层架构)
- [文件组织原则](#文件组织原则)
- [命名约定](#命名约定)
- [配置管理](#配置管理)
- [文档结构](#文档结构)
- [构建输出](#构建输出)

## 项目概述

ModularGodot.Core 采用分层架构和模块化设计，项目结构清晰地反映了架构层次和职责分离。

### 设计原则

- **分层清晰**：每层有明确的职责和边界
- **依赖单向**：依赖关系向内流动，避免循环依赖
- **模块化**：功能按模块组织，便于维护和扩展
- **约定优于配置**：统一的命名和组织约定

## 目录结构

```
ModularGodot.Core/
├── .gitignore                          # Git忽略文件配置
├── README.md                           # 项目说明文档
├── .spec-workflow/                     # 规范工作流配置
│   ├── steering/                       # 指导文档
│   │   ├── product.md                  # 产品愿景文档
│   │   ├── tech.md                     # 技术架构文档
│   │   └── structure.md                # 项目结构文档
│   └── templates/                      # 文档模板
├── docs/                               # 项目文档
│   ├── Architecture.md                 # 架构设计文档
│   ├── API-Reference.md                # API参考文档
│   └── Examples.md                     # 使用示例文档
└── src/                                # 源代码目录
    ├── 0_Contracts/                    # 契约层
    │   ├── Abstractions/               # 抽象接口定义
    │   │   ├── Bases/                  # 基础类定义
    │   │   ├── Caching/                # 缓存服务接口
    │   │   ├── Logging/                # 日志服务接口
    │   │   ├── Messaging/              # 消息传递接口
    │   │   ├── Monitoring/             # 监控服务接口
    │   │   ├── ResourceLoading/        # 资源加载接口
    │   │   └── ResourceManagement/     # 资源管理接口
    │   ├── Attributes/                 # 自定义特性
    │   │   ├── InjectableAttribute.cs  # 可注入标记特性
    │   │   └── SkipRegistrationAttribute.cs # 跳过注册特性
    │   ├── Events/                     # 事件定义
    │   │   └── ResourceManagement/     # 资源管理事件
    │   ├── MF.Contracts.csproj         # 契约层项目文件
    │   └── Singleton.cs                # 单例基类
    ├── 1_Contexts/                     # 上下文层
    │   ├── Contexts.cs                 # 主要IoC容器配置
    │   ├── MediatorModule.cs           # MediatR模块配置
    │   ├── SingleModule.cs             # 单例服务模块
    │   └── MF.Context.csproj           # 上下文层项目文件
    ├── 2_Infrastructure/               # 基础设施层
    │   ├── Caching/                    # 缓存实现
    │   │   ├── CacheConfig.cs          # 缓存配置
    │   │   └── MemoryCacheService.cs   # 内存缓存服务
    │   ├── Logging/                    # 日志实现
    │   │   └── GodotGameLogger.cs      # Godot日志记录器
    │   ├── Messaging/                  # 消息传递实现
    │   │   ├── MediatRAdapter.cs       # MediatR适配器
    │   │   ├── MediatRHandlerAdapter.cs # 处理器适配器
    │   │   ├── MediatRRequestAdapter.cs # 请求适配器
    │   │   └── R3EventBus.cs           # R3事件总线
    │   ├── Monitoring/                 # 监控实现
    │   │   ├── MemoryMonitor.cs        # 内存监控器
    │   │   └── PerformanceMonitor.cs   # 性能监控器
    │   ├── ResourceLoading/            # 资源加载实现
    │   │   └── GodotResourceLoader.cs  # Godot资源加载器
    │   ├── ResourceManagement/         # 资源管理实现
    │   │   └── ResourceManager.cs      # 资源管理器
    │   └── MF.Infrastructure.csproj    # 基础设施层项目文件
    ├── 3_Repositories/                 # 仓储层
    │   └── MF.Repositories.csproj      # 仓储层项目文件
    └── ModularGodot.Core.sln           # 解决方案文件
```

## 分层架构

### 0_Contracts (契约层)

**职责**：定义系统的契约和数据传输对象

**特点**：
- 无外部依赖
- 纯接口和数据结构定义
- 其他层的依赖基础

**组织结构**：
```
0_Contracts/
├── Abstractions/           # 核心抽象接口
│   ├── Bases/             # 基础类和接口
│   ├── Caching/           # 缓存相关接口
│   ├── Logging/           # 日志相关接口
│   ├── Messaging/         # 消息传递接口
│   ├── Monitoring/        # 监控相关接口
│   ├── ResourceLoading/   # 资源加载接口
│   └── ResourceManagement/ # 资源管理接口
├── Attributes/            # 自定义特性
├── Events/               # 事件定义
└── *.cs                  # 共享类型定义
```

### 1_Contexts (上下文层)

**职责**：依赖注入配置和模块管理

**特点**：
- 使用Autofac作为IoC容器
- 集成MediatR进行命令/查询处理
- 自动服务发现和注册

**组织结构**：
```
1_Contexts/
├── Contexts.cs           # 主要IoC容器配置
├── MediatorModule.cs     # MediatR相关服务注册
├── SingleModule.cs       # 单例服务注册
└── *Module.cs           # 其他功能模块
```

### 2_Infrastructure (基础设施层)

**职责**：提供具体的技术实现

**特点**：
- 按功能领域组织子目录
- 每个子目录包含相关的实现类
- 实现0_Contracts中定义的接口

**组织结构**：
```
2_Infrastructure/
├── Caching/              # 缓存实现
├── Logging/              # 日志实现
├── Messaging/            # 消息传递实现
├── Monitoring/           # 监控实现
├── ResourceLoading/      # 资源加载实现
└── ResourceManagement/   # 资源管理实现
```

### 3_Repositories (仓储层)

**职责**：数据访问抽象

**特点**：
- 当前为空项目，预留给数据持久化需求
- 可扩展支持文件系统、数据库等存储
- 实现领域驱动设计的仓储模式

## 文件组织原则

### 1. 按功能分组

每个功能领域有独立的目录，包含相关的所有文件：

```
Caching/
├── ICacheService.cs      # 接口定义
├── CacheConfig.cs        # 配置类
├── MemoryCacheService.cs # 实现类
└── CacheExtensions.cs    # 扩展方法
```

### 2. 接口与实现分离

- 接口定义在0_Contracts层
- 具体实现在2_Infrastructure层
- 配置和注册在1_Contexts层

### 3. 相关文件就近原则

- 配置类与服务类在同一目录
- 扩展方法与核心类在同一目录
- 测试文件与被测试文件对应

### 4. 依赖方向控制

```
依赖流向：
Game Logic → 1_Contexts → 0_Contracts ← 2_Infrastructure
                                      ← 3_Repositories
```

## 命名约定

### 项目命名

- **前缀**：MF (ModularFramework)
- **格式**：MF.{LayerName}
- **示例**：MF.Contracts, MF.Context, MF.Infrastructure

### 文件命名

- **接口**：I{ServiceName}.cs (如：ICacheService.cs)
- **实现类**：{ServiceName}.cs (如：MemoryCacheService.cs)
- **配置类**：{ServiceName}Config.cs (如：CacheConfig.cs)
- **扩展类**：{ServiceName}Extensions.cs (如：CacheExtensions.cs)
- **特性类**：{AttributeName}Attribute.cs (如：InjectableAttribute.cs)

### 目录命名

- **功能目录**：PascalCase (如：ResourceLoading)
- **层级目录**：数字前缀 + 描述 (如：0_Contracts)
- **子功能**：按功能领域分组 (如：Caching, Logging)

### 命名空间约定

```csharp
// 契约层
namespace MF.Contracts.Abstractions.Caching;

// 上下文层
namespace MF.Context;

// 基础设施层
namespace MF.Infrastructure.Caching;

// 仓储层
namespace MF.Repositories;
```

## 配置管理

### 项目配置文件

每个项目都有独立的.csproj文件：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <!-- 包引用 -->
  <ItemGroup>
    <PackageReference Include="..." Version="..." />
  </ItemGroup>
  
  <!-- 项目引用 -->
  <ItemGroup>
    <ProjectReference Include="..." />
  </ItemGroup>
</Project>
```

### 解决方案结构

```xml
Microsoft Visual Studio Solution File, Format Version 12.00
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "MF.Contracts", "0_Contracts\MF.Contracts.csproj"
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "MF.Context", "1_Contexts\MF.Context.csproj"
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "MF.Infrastructure", "2_Infrastructure\MF.Infrastructure.csproj"
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "MF.Repositories", "3_Repositories\MF.Repositories.csproj"
```

### 版本管理

- **统一版本**：所有项目使用相同的.NET版本
- **包版本锁定**：在Directory.Build.props中统一管理
- **语义化版本**：遵循SemVer规范

## 文档结构

### 文档分类

```
docs/
├── Architecture.md       # 架构设计文档
├── API-Reference.md      # API参考文档
├── Examples.md          # 使用示例文档
├── Contributing.md      # 贡献指南（待添加）
├── Changelog.md         # 变更日志（待添加）
└── Troubleshooting.md   # 故障排除（待添加）
```

### 规范工作流文档

```
.spec-workflow/
├── steering/            # 指导文档
│   ├── product.md       # 产品愿景
│   ├── tech.md          # 技术架构
│   └── structure.md     # 项目结构
├── specs/              # 功能规范（待添加）
└── templates/          # 文档模板
```

### 文档维护原则

1. **及时更新**：代码变更时同步更新文档
2. **版本控制**：文档与代码一起进行版本管理
3. **格式统一**：使用Markdown格式，遵循统一的文档模板
4. **示例丰富**：提供充足的代码示例和使用场景

## 构建输出

### 输出目录结构

```
bin/
├── Debug/
│   └── net9.0/
│       ├── MF.Contracts.dll
│       ├── MF.Context.dll
│       ├── MF.Infrastructure.dll
│       └── MF.Repositories.dll
└── Release/
    └── net9.0/
        └── [相同结构]
```

### 部署包结构

```
ModularGodot.Core.Package/
├── lib/
│   └── net9.0/
│       ├── MF.Contracts.dll
│       ├── MF.Context.dll
│       ├── MF.Infrastructure.dll
│       └── MF.Repositories.dll
├── docs/
│   ├── Architecture.md
│   ├── API-Reference.md
│   └── Examples.md
└── README.md
```

## 扩展指南

### 添加新功能模块

1. **在0_Contracts中定义接口**
   ```csharp
   namespace MF.Contracts.Abstractions.NewFeature;
   public interface INewFeatureService { }
   ```

2. **在2_Infrastructure中实现**
   ```csharp
   namespace MF.Infrastructure.NewFeature;
   [Injectable]
   public class NewFeatureService : INewFeatureService { }
   ```

3. **在1_Contexts中注册（如需要）**
   ```csharp
   public class NewFeatureModule : Module
   {
       protected override void Load(ContainerBuilder builder)
       {
           // 自定义注册逻辑
       }
   }
   ```

### 添加新的层级

如果需要添加新的架构层：

1. 创建新的项目目录：`4_NewLayer/`
2. 添加项目文件：`MF.NewLayer.csproj`
3. 更新解决方案文件
4. 确保依赖关系正确
5. 更新文档

---

## 总结

ModularGodot.Core的项目结构设计充分体现了分层架构和模块化的设计理念。清晰的目录组织、统一的命名约定和明确的职责分离，为项目的可维护性、可扩展性和团队协作提供了坚实的基础。

通过遵循这些结构约定，开发团队可以快速理解项目组织，高效地进行功能开发和维护工作。