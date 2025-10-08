# ModularGodot.Core

一个基于 Godot 4.4 的模块化游戏开发框架，采用分层架构和现代 C# 设计模式。

## 🎯 项目概述

ModularGodot.Core 是一个为 Godot 游戏开发设计的企业级框架，提供了完整的基础设施支持，包括：

- **分层架构**：清晰的职责分离和依赖管理
- **事件驱动**：基于 R3 的响应式事件系统
- **中介者模式**：解耦的命令和查询处理
- **资源管理**：智能缓存和内存监控
- **性能监控**：实时性能指标收集
- **依赖注入**：基于 Autofac 的 IoC 容器

## 🏗️ 架构设计

### 分层结构

```
src/
├── ModularGodot.Core.Contracts/  # 契约层 - 接口定义和数据传输对象
│   ├── Abstractions/            # 核心抽象接口
│   ├── Attributes/              # 自定义特性
│   └── Events/                 # 事件定义
├── ModularGodot.Core.Contexts/   # 上下文层 - 依赖注入配置
├── ModularGodot.Core.Infrastructure/  # 基础设施层 - 具体实现
│   ├── Caching/                # 缓存服务
│   ├── Logging/                # 日志服务
│   ├── Messaging/              # 消息传递
│   ├── Monitoring/             # 性能监控
│   ├── ResourceLoading/        # 资源加载
│   └── ResourceManagement/     # 资源管理
└── ModularGodot.Core.Repositories/  # 仓储层 - 数据访问
```

### 核心组件

#### 🔄 事件系统
- **IEventBus**: 事件发布和订阅接口
- **IEventSubscriber**: 仅订阅功能的接口
- **R3EventBus**: 基于 R3 的高性能事件总线实现

#### 📨 中介者模式
- **IMyMediator**: 自定义中介者接口（无 MediatR 依赖）
- **ICommand/IQuery**: 命令和查询接口
- **MediatRAdapter**: MediatR 适配器实现

#### 💾 缓存系统
- **ICacheService**: 缓存服务抽象
- **MemoryCacheService**: 内存缓存实现

#### 📊 监控系统
- **IPerformanceMonitor**: 性能监控接口
- **IMemoryMonitor**: 内存监控接口

## 🚀 快速开始

### 0. Godot 集成

#### 全局服务节点

项目提供了 `GodotGlobalService` 节点，用于在 Godot 环境中轻松访问核心服务：

1. 将 `GodotGlobalService` 节点添加到场景树的根节点
2. 通过单例实例访问核心服务：

```csharp
// 获取全局服务实例
var globalService = GodotGlobalService.Instance;

// 解析中介者接口
var dispatcher = globalService.ResolveDispatcher();

// 解析事件总线接口
var eventBus = globalService.ResolveEventBus();
```

详细使用示例请参考：[ServiceUsageExample.cs](src/ModularGodot.Core/Examples/ServiceUsageExample.cs)

### 1. 环境要求

- .NET 9.0
- Godot 4.4.1
- **Windows**: Visual Studio 2022 或 JetBrains Rider
- **Linux**: VS Code 或 JetBrains Rider

### 2. 项目结构

```bash
# 克隆项目
git clone <repository-url>
cd ModularGodot.Core

# 构建解决方案
dotnet build src/ModularGodot.Core.sln
```

### 3. NuGet 包结构

本项目现在支持两种使用方式：

#### 方式一：使用独立的 NuGet 包（推荐）

每个架构层都可作为独立的 NuGet 包使用，便于按需引入：

- `ModularGodot.Core.Contracts` - 契约层，包含接口定义和 DTO
- `ModularGodot.Core.Contexts` - 上下文层，包含依赖注入配置
- `ModularGodot.Core.Infrastructure` - 基础设施层，包含具体实现
- `ModularGodot.Core.Repositories` - 仓储层，包含数据访问功能

包之间的依赖关系遵循架构层次：
```
ModularGodot.Core.Repositories
  ↓ 依赖
ModularGodot.Core.Infrastructure
  ↓ 依赖
ModularGodot.Core.Contexts
  ↓ 依赖
ModularGodot.Core.Contracts
```

#### 方式二：使用完整框架包

`ModularGodot.Core` 包含所有层的功能，适合快速开发：

```xml
<PackageReference Include="ModularGodot.Core" Version="1.0.0" />
```

### 4. 构建和打包

项目提供了多种构建选项：

#### 构建独立包
```bash
# 构建所有独立包
dotnet pack src/ModularGodot.Core.sln -c Release -o packages

# 或者构建单个包
dotnet pack src/ModularGodot.Core.Contracts/ModularGodot.Core.Contracts.csproj -c Release -o packages
dotnet pack src/ModularGodot.Core.Contexts/ModularGodot.Core.Contexts.csproj -c Release -o packages
dotnet pack src/ModularGodot.Core.Infrastructure/ModularGodot.Core.Infrastructure.csproj -c Release -o packages
dotnet pack src/ModularGodot.Core.Repositories/ModularGodot.Core.Repositories.csproj -c Release -o packages
```

#### 构建完整框架包
```bash
dotnet pack src/ModularGodot.Core/ModularGodot.Core.csproj -c Release -o packages
```

#### 一键打包脚本

项目提供了 PowerShell 脚本以简化构建和打包过程：

```bash
# 使用增强型构建和打包脚本（推荐）
./tools/enhanced-build-pack.ps1 -Configuration Release

# 清理构建产物
./tools/cleanup.ps1
```

`enhanced-build-pack.ps1` 脚本功能：
- 自动构建所有项目层
- 生成 NuGet 包
- 支持多种配置选项（Debug/Release）
- 可选择是否包含符号包
- 自动清理临时文件

使用示例：
```bash
# 基本用法
./tools/enhanced-build-pack.ps1 -Configuration Release

# 包含符号包
./tools/enhanced-build-pack.ps1 -Configuration Release -IncludeSymbols

# 跳过清理临时文件
./tools/enhanced-build-pack.ps1 -Configuration Release -SkipCleanup

# 指定输出目录
./tools/enhanced-build-pack.ps1 -Configuration Release -OutputDirectory "./my-packages"
```

#### Linux 环境支持

项目现在支持在 Linux 环境下进行构建和清理，提供了与 PowerShell 脚本功能相同的 Bash 脚本：

```bash
# 使用增强型构建和打包脚本（Linux 版本）
./tools/enhanced-build-pack.sh -c Release

# 清理构建产物
./tools/cleanup.sh
```

`enhanced-build-pack.sh` 脚本功能：
- 自动构建所有项目层
- 生成 NuGet 包
- 支持多种配置选项（Debug/Release）
- 可选择是否包含符号包
- 自动清理临时文件
- 跨平台兼容性

Linux 脚本使用示例：
```bash
# 基本用法
./tools/enhanced-build-pack.sh -c Release

# 包含符号包
./tools/enhanced-build-pack.sh -c Release --include-symbols

# 跳过清理临时文件
./tools/enhanced-build-pack.sh -c Release --skip-cleanup

# 指定输出目录
./tools/enhanced-build-pack.sh -c Release -o "./my-packages"

# 显示详细输出
./tools/enhanced-build-pack.sh -c Release -v

# 构建特定包
./tools/enhanced-build-pack.sh --packages ModularGodot.Core.Contracts ModularGodot.Core.Infrastructure
```

## 📚 详细文档

- [架构设计文档](docs/ARCHITECTURE.md) - 详细的架构说明和设计原则
- [NuGet 包文档](docs/NUGET_PACKAGES.md) - NuGet 包结构和使用说明
- [插件架构文档](docs/PLUGIN_ARCHITECTURE.md) - 插件开发和集成指南
- [依赖注入文档](docs/DEPENDENCY_INJECTION.md) - 依赖注入机制和使用说明

## 🛠️ 技术栈

### 核心框架
- **Godot 4.4.1** - 游戏引擎
- **.NET 9.0** - 运行时平台

### 依赖注入
- **Autofac 8.3.0** - IoC 容器

### 消息传递
- **MediatR 13.0.0** - 中介者模式实现
- **R3 1.3.0** - 响应式编程库

### 缓存
- **Microsoft.Extensions.Caching.Memory 9.0.7** - 内存缓存

### 响应式编程
- **System.Reactive 6.0.0** - Rx.NET

## 🎨 设计原则

### SOLID 原则
- **单一职责原则**: 每个类只有一个变化的理由
- **开闭原则**: 对扩展开放，对修改关闭
- **里氏替换原则**: 子类可以替换父类
- **接口隔离原则**: 客户端不应依赖不需要的接口
- **依赖倒置原则**: 依赖抽象而非具体实现

### 分层架构原则
- **依赖方向**: 只能向内层依赖，不能反向依赖
- **接口隔离**: 通过接口定义层间契约
- **关注点分离**: 每层专注于特定职责

## 🔧 开发指南

### 添加新功能

1. **定义接口** - 在 `ModularGodot.Core.Contracts/Abstractions` 中定义抽象接口
2. **实现功能** - 在 `ModularGodot.Core.Infrastructure` 中提供具体实现
3. **配置依赖** - 在 `ModularGodot.Core.Contexts` 中注册服务
4. **编写测试** - 确保功能正确性

### 最佳实践

- 优先使用接口而非具体类
- 遵循异步编程模式
- 合理使用事件驱动架构
- 注意内存管理和性能优化
- 编写清晰的文档和注释

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！请确保：

1. 遵循现有的代码风格
2. 添加适当的测试
3. 更新相关文档
4. 提供清晰的提交信息

## 📞 支持

如有问题或建议，请：

1. 查看 [文档](docs/)
2. 搜索现有 [Issues](../../issues)
3. 创建新的 Issue

---

**Happy Coding! 🎮**