# ModularGodot.Core NuGet 包结构与依赖关系文档

## 1. 概述

本项目采用多包发布策略，为每个架构层提供独立的NuGet包，同时提供一个完整的框架包以简化使用。这种设计既保持了架构的完整性，又提供了灵活的依赖管理。

## 2. NuGet 包结构

### 2.1 独立层包

为每个架构层提供独立的NuGet包：

1. **ModularGodot.Core.Contracts**
   - 包含：接口定义和DTOs
   - 依赖：ModularGodot.Contracts

2. **ModularGodot.Core.Contexts**
   - 包含：依赖注入配置
   - 依赖：ModularGodot.Contexts, ModularGodot.Core.Contracts

3. **ModularGodot.Core.Infrastructure**
   - 包含：具体实现
   - 依赖：ModularGodot.Infrastructure, ModularGodot.Core.Contexts, ModularGodot.Core.Contracts

4. **ModularGodot.Core.Repositories**
   - 包含：数据访问层
   - 依赖：ModularGodot.Repositories, ModularGodot.Core.Contexts, ModularGodot.Core.Contracts

### 2.2 完整框架包

**ModularGodot.Core**
   - 包含：完整框架功能
   - 依赖：ModularGodot.Core.Contexts, ModularGodot.Core.Contracts, ModularGodot.Core.Infrastructure, ModularGodot.Core.Repositories

### 2.3 合并包

**ModularGodot.Core.Merged**
   - 包含：所有模块合并为单个程序集的版本
   - 依赖：无

## 3. 依赖关系图

```
消费者项目
├── ModularGodot.Core (完整框架包)
│   ├── ModularGodot.Core.Contexts
│   │   ├── ModularGodot.Core.Contracts
│   │   └── ModularGodot.Contexts
│   ├── ModularGodot.Core.Infrastructure
│   │   ├── ModularGodot.Core.Contexts
│   │   ├── ModularGodot.Core.Contracts
│   │   └── ModularGodot.Infrastructure
│   ├── ModularGodot.Core.Repositories
│   │   ├── ModularGodot.Core.Contexts
│   │   ├── ModularGodot.Core.Contracts
│   │   └── ModularGodot.Repositories
│   └── ModularGodot.Core.Contracts
│       └── ModularGodot.Contracts
└── ModularGodot.Core.Merged (合并版本，无依赖)
```

## 4. 使用指南

### 4.1 完整框架使用

对于需要完整功能的项目：

```xml
<PackageReference Include="ModularGodot.Core" Version="1.0.0" />
```

### 4.2 按需引用

对于只需要特定功能的项目：

```xml
<!-- 只需要契约层 -->
<PackageReference Include="ModularGodot.Core.Contracts" Version="1.0.0" />

<!-- 需要上下文和契约层 -->
<PackageReference Include="ModularGodot.Core.Contexts" Version="1.0.0" />
<PackageReference Include="ModularGodot.Core.Contracts" Version="1.0.0" />
```

### 4.3 合并版本使用

对于需要最小依赖的项目：

```xml
<PackageReference Include="ModularGodot.Core.Merged" Version="0.1.0" />
```

## 5. 构建和发布

### 5.1 多包构建

使用 `build-multiple-packages.ps1` 脚本构建所有NuGet包：

```powershell
.\build-multiple-packages.ps1 -Configuration Release
```

### 5.2 合并包构建

使用 `manual-merge-pack-final-fixed.ps1` 脚本构建合并版本：

```powershell
cd tools
.\manual-merge-pack-final-fixed.ps1 -Configuration Release
```

## 6. 版本管理

所有包使用统一的版本号，通过 `Directory.Build.props` 文件进行管理：

```xml
<PropertyGroup>
  <Version>1.0.0</Version>
</PropertyGroup>
```

## 7. 架构优势

1. **保持架构完整性** - 各层职责清晰，依赖方向明确
2. **灵活的依赖管理** - 消费者可按需引用特定层
3. **简化使用** - 提供完整框架包，一键集成所有功能
4. **优化部署** - 提供合并版本，减少程序集数量
5. **易于维护** - 各层可独立演进，通过版本号管理兼容性