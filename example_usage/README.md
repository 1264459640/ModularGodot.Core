# ModularGodot.Core 使用示例

这个示例项目演示了如何在你的Godot项目中使用ModularGodot.Core NuGet包。

## 依赖管理方式

### 1. 使用本地NuGet包

项目中的 `nuget.config` 文件配置了本地包源：

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="local" value="../src/ModularGodot.Core/bin/Release" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

### 2. 在项目文件中引用

在 `.csproj` 文件中添加包引用：

```xml
<ItemGroup>
  <PackageReference Include="ModularGodot.Core" Version="0.1.0" />
</ItemGroup>
```

### 3. 安装包

运行以下命令安装包：

```bash
dotnet restore
```

## 包含的功能

ModularGodot.Core 包含以下层：

- **Contracts**: 接口和契约定义
- **Contexts**: 游戏上下文管理
- **Infrastructure**: 基础设施服务（日志、缓存等）
- **Repositories**: 数据访问层

## 主要依赖

- Autofac (依赖注入)
- MediatR (中介者模式)
- GodotSharp (Godot引擎支持)
- Microsoft.Extensions.Caching (缓存)
- R3 & System.Reactive (响应式编程)

## 使用方法

参考 `ExampleService.cs` 文件，了解如何：

1. 注入和使用各种服务
2. 使用游戏上下文
3. 使用日志系统
4. 使用MediatR进行消息传递

## 发布到NuGet.org

如果要发布到公共NuGet仓库：

```bash
dotnet nuget push src/ModularGodot.Core/bin/Release/ModularGodot.Core.0.1.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```