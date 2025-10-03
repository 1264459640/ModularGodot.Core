# 插件架构设计文档

## 概述

ModularGodot.Core 框架支持插件化架构，允许开发者创建可扩展的功能模块。插件项目通常需要维护两个包：本体包和共享契约包。这种设计遵循了关注点分离的原则，确保了插件之间的松耦合和高内聚。

## 插件包结构

### 1. 共享契约包 (Shared Contracts Package)

共享契约包是插件的核心接口定义包，包含：

- **接口定义** - 插件提供的服务接口
- **事件定义** - 插件发布或订阅的事件
- **命令定义** - 插件处理的命令
- **数据传输对象 (DTO)** - 插件间共享的数据结构

#### 特点

- 无外部依赖关系
- 可被多个插件引用和使用
- 可由多个插件团队共同维护
- 版本稳定性要求高，避免频繁破坏性变更

#### 示例结构

```
MyPlugin.Contracts/
├── Commands/           # 命令定义
│   ├── CreateUserCommand.cs
│   └── UpdateUserCommand.cs
├── Events/             # 事件定义
│   ├── UserCreatedEvent.cs
│   └── UserUpdatedEvent.cs
├── Interfaces/         # 接口定义
│   ├── IUserService.cs
│   └── IUserRepository.cs
└── DTOs/              # 数据传输对象
    ├── UserDto.cs
    └── UserProfileDto.cs
```

### 2. 本体包 (Implementation Package)

本体包包含插件的具体实现，通常包含：

- **服务类** - 实现业务逻辑的服务
- **基础设施类** - 数据访问、外部服务调用等
- **命令处理器** - 处理特定命令的实现
- **事件处理器** - 处理特定事件的实现

#### 特点

- 必须引用共享契约包或架构核心契约包
- 可引用其他插件的共享契约包
- 包含具体的业务逻辑实现
- 可能有外部依赖（数据库驱动、HTTP客户端等）

#### 示例结构

```
MyPlugin/
├── Services/           # 服务实现
│   ├── UserService.cs
│   └── EmailService.cs
├── Infrastructure/     # 基础设施实现
│   ├── UserRepository.cs
│   └── EmailSender.cs
├── Handlers/           # 命令和事件处理器
│   ├── CreateUserHandler.cs
│   ├── UpdateUserHandler.cs
│   └── UserCreatedEventHandler.cs
└── MyPlugin.csproj     # 项目文件
```

## 依赖关系

```
插件消费者
├── MyPlugin (本体包)
│   ├── MyPlugin.Contracts (共享契约包)
│   ├── ModularGodot.Core.Contracts (核心契约包)
│   └── 其他依赖...
└── OtherPlugin.Contracts (其他插件的共享契约包)
```

## 插件开发最佳实践

### 1. 共享契约包开发

1. **接口设计**
   - 遵循接口隔离原则，定义细粒度接口
   - 使用明确的命名约定
   - 避免在接口中暴露实现细节

2. **事件设计**
   - 事件应包含足够的上下文信息
   - 使用继承来共享通用属性
   - 确保事件的不可变性

3. **命令设计**
   - 命令应表示用户的意图
   - 包含验证逻辑或明确的验证要求
   - 保持命令的简单性和专注性

### 2. 本体包开发

1. **服务实现**
   - 实现共享契约包中定义的接口
   - 遵循单一职责原则
   - 使用依赖注入管理依赖关系

2. **基础设施实现**
   - 封装外部系统的访问逻辑
   - 提供适当的错误处理和重试机制
   - 实现连接池和资源管理

3. **处理器实现**
   - 命令处理器应专注于单个命令的处理
   - 事件处理器应具有幂等性
   - 避免在处理器中实现复杂的业务逻辑

## 插件集成

### 事件订阅

插件可以通过事件总线订阅其他插件或核心框架发布的事件：

```csharp
[Injectable(Lifetime.Singleton)]
public class PluginEventHandler
{
    private readonly IEventBus _eventBus;

    public PluginEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
        // 订阅用户创建事件
        _eventBus.Subscribe<UserCreatedEvent>(OnUserCreated);
    }

    private void OnUserCreated(UserCreatedEvent evt)
    {
        // 处理用户创建事件
    }
}
```

## 版本管理

### 1. 共享契约包版本控制

- 遵循语义化版本控制 (SemVer)
- 主版本号变更表示破坏性变更
- 次版本号变更表示向后兼容的功能添加
- 修订版本号变更表示向后兼容的错误修复

### 2. 本体包版本控制

- 与共享契约包版本保持兼容性
- 可以比共享契约包含有更多的修订版本

## 测试策略

### 1. 共享契约包测试

- 主要进行接口契约测试
- 确保事件和命令的序列化兼容性
- 验证DTO的验证规则

### 2. 本体包测试

- 单元测试服务和处理器实现
- 集成测试基础设施组件
- 端到端测试核心业务流程

## 部署和分发

### 1. NuGet 包发布

两个包都应该作为独立的 NuGet 包发布：

```xml
<!-- 共享契约包 -->
<PackageReference Include="MyPlugin.Contracts" Version="1.0.0" />

<!-- 本体包 -->
<PackageReference Include="MyPlugin" Version="1.0.0" />
```

### 2. 依赖管理

- 本体包应明确依赖其共享契约包
- 避免循环依赖
- 使用版本范围来管理兼容性

## 示例插件结构

以下是一个完整插件项目的示例结构：

```
MyUserManagementPlugin/
├── src/
│   ├── MyUserManagementPlugin.Contracts/     # 共享契约包
│   │   ├── Commands/
│   │   │   ├── CreateUserCommand.cs
│   │   │   └── UpdateUserCommand.cs
│   │   ├── Events/
│   │   │   ├── UserCreatedEvent.cs
│   │   │   └── UserUpdatedEvent.cs
│   │   ├── Interfaces/
│   │   │   ├── IUserService.cs
│   │   │   └── IUserRepository.cs
│   │   ├── DTOs/
│   │   │   ├── UserDto.cs
│   │   │   └── UserProfileDto.cs
│   │   └── MyUserManagementPlugin.Contracts.csproj
│   └── MyUserManagementPlugin/               # 本体包
│       ├── Services/
│       │   └── UserService.cs
│       ├── Infrastructure/
│       │   └── UserRepository.cs
│       ├── Handlers/
│       │   ├── CreateUserHandler.cs
│       │   ├── UpdateUserHandler.cs
│       │   ├── UserCreatedEventHandler.cs
│       │   └── UserUpdatedEventHandler.cs
│       └── MyUserManagementPlugin.csproj
└── tests/
    ├── MyUserManagementPlugin.Contracts.Tests/
    └── MyUserManagementPlugin.Tests/
```

这种插件架构设计确保了模块化、可扩展性和可维护性，同时保持了插件之间的松耦合关系。通过使用自动化依赖注入机制，插件开发者可以专注于业务逻辑实现，而无需关心复杂的依赖注入配置。