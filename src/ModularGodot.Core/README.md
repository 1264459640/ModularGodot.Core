# ModularGodot.Core

Complete ModularGodot.Core package containing all layers for modular Godot game development.

## 包含的模块

### 0_Contracts (合约层)
- 定义所有接口和抽象
- 事件定义
- 枚举和常量
- 基础类型

### 1_Contexts (上下文层)  
- 依赖注入容器配置
- MediatR 配置
- 模块注册

### 2_Infrastructure (基础设施层)
- 缓存服务实现
- 日志服务实现
- 消息总线实现
- 性能监控实现
- 资源管理实现

### 3_Repositories (仓储层)
- 数据访问抽象
- 仓储模式实现

## 主要功能

- **依赖注入**: 基于 Autofac 的 IoC 容器
- **事件驱动架构**: 基于 MediatR 的命令/查询/事件处理
- **缓存系统**: 内存缓存和分布式缓存支持
- **日志框架**: 结构化日志记录
- **性能监控**: 实时性能指标收集
- **资源管理**: Godot 资源加载和缓存
- **响应式编程**: 基于 R3 的响应式扩展

## 使用方法

```csharp
// 获取版本信息
var version = ModularGodotCore.Version;

// 获取支持的功能
var features = ModularGodotCore.SupportedFeatures;

// 获取可用接口
var interfaces = ModularGodotCore.AvailableInterfaces;
```

## 版本

当前版本: 0.1.0

## 许可证

MIT License