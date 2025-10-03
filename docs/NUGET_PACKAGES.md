# ModularGodot.Core NuGet 包文档

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
   - 包含：作为框架安装时的原子性操作包
   - 作用：自动解析并安装核心的四个包（Contracts, Contexts, Infrastructure, Repositories）
   - 依赖：ModularGodot.Core.Contexts, ModularGodot.Core.Contracts, ModularGodot.Core.Infrastructure, ModularGodot.Core.Repositories

## 3. 依赖关系图

```
消费者项目
├── ModularGodot.Core (完整框架包 - 自动安装以下所有核心包)
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
```

## 4. 使用指南

### 4.1 完整框架使用

对于需要完整功能的项目，只需安装一个包即可自动获取所有核心依赖：

```xml
<PackageReference Include="ModularGodot.Core" Version="1.0.0" />
```

### 4.2 按需引用

对于只需要特定功能的项目，可以按需引用独立层包：

```xml
<!-- 只需要契约层 -->
<PackageReference Include="ModularGodot.Core.Contracts" Version="1.0.0" />

<!-- 需要上下文和契约层 -->
<PackageReference Include="ModularGodot.Core.Contexts" Version="1.0.0" />
<PackageReference Include="ModularGodot.Core.Contracts" Version="1.0.0" />
```

## 5. 构建和发布

### 5.1 多包构建

使用 `build-multiple-packages.ps1` 脚本构建所有NuGet包：

```powershell
.\build-multiple-packages.ps1 -Configuration Release
```

也可以使用增强型构建脚本：

```powershell
./tools/enhanced-build-pack.ps1 -Configuration Release
```

### 5.2 单独构建包

```bash
# 构建所有独立包
dotnet pack src/ModularGodot.Core.sln -c Release -o packages

# 构建单个包
dotnet pack src/ModularGodot.Core.Contracts/ModularGodot.Core.Contracts.csproj -c Release -o packages
dotnet pack src/ModularGodot.Core.Contexts/ModularGodot.Core.Contexts.csproj -c Release -o packages
dotnet pack src/ModularGodot.Core.Infrastructure/ModularGodot.Core.Infrastructure.csproj -c Release -o packages
dotnet pack src/ModularGodot.Core.Repositories/ModularGodot.Core.Repositories.csproj -c Release -o packages

# 构建完整框架包（作为依赖解析包，不包含实际代码）
dotnet pack src/ModularGodot.Core/ModularGodot.Core.csproj -c Release -o packages
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
4. **易于维护** - 各层可独立演进，通过版本号管理兼容性

## 8. 实现细节

### 8.1 已完成任务

1. **创建 NuGet 包项目目录结构**
   - 为每个架构层创建了独立的NuGet包项目目录
   - 创建了完整的框架便利包目录
   - 维护了项目的整体结构

2. **创建 NuGet 包项目文件 (.csproj)**
   - 为Contracts、Contexts、Infrastructure、Repositories层创建了独立的NuGet包项目文件
   - 为完整框架创建了便利包项目文件（仅作为依赖解析）
   - 正确配置了各包的依赖关系和元数据

3. **创建版本同步配置文件 (Directory.Build.props)**
   - 创建了统一的版本管理配置文件
   - 实现了所有包的版本同步

4. **更新解决方案文件**
   - 在解决方案中添加了所有新的NuGet包项目
   - 正确配置了项目间的依赖关系

5. **更新构建脚本**
   - 创建了多包构建脚本(build-multiple-packages.ps1)
   - 更新了现有的合并打包脚本以支持新的项目结构

6. **测试 NuGet 包生成**
   - 成功生成了所有独立层的NuGet包
   - 成功生成了完整框架便利包

7. **验证包依赖关系**
   - 验证了Contracts包的依赖关系
   - 验证了Contexts包的依赖关系
   - 验证了Infrastructure包的依赖关系
   - 验证了Repositories包的依赖关系
   - 验证了完整框架包的依赖关系

### 8.2 解决的问题

1. **NU1101 错误** - 通过创建包装项目和正确的依赖配置解决
2. **架构完整性** - 保持了原有的分层架构设计
3. **灵活依赖管理** - 消费者可以选择按需引用特定层或使用完整框架
4. **版本同步** - 通过Directory.Build.props实现统一版本管理

## 9. 最佳实践

### 9.1 选择合适的包

- **初学者或快速原型开发**：使用 `ModularGodot.Core` 完整框架包
- **生产环境或对包大小敏感**：按需引用独立层包

### 9.2 版本管理建议

- 始终保持包版本同步
- 在生产环境中锁定版本号
- 定期更新以获取最新功能和安全修复

### 9.3 开发工作流

1. 选择合适的包引用方式
2. 遵循分层架构原则进行开发
3. 使用依赖注入特性标记服务
4. 通过构建脚本生成和测试包
5. 验证包的功能和依赖关系