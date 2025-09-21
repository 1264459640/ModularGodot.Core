# ModularGodot.Core 动态DLL加载功能

## 概述

`SingleModule` 现在支持动态发现和加载DLL文件，无需硬编码程序集名称。这使得在NuGet包环境中更加灵活，能够自动发现并加载指定文件夹中的相关DLL文件。

## 主要特性

### 1. 灵活的搜索路径配置
- **默认搜索路径**：
  - 当前应用程序目录 (`AppDomain.CurrentDomain.BaseDirectory`)
  - `bin` 文件夹
  - `lib` 文件夹
  - `modules` 文件夹
  - `plugins` 文件夹

- **自定义搜索路径**：可以通过构造函数指定任意搜索路径

### 2. 智能文件模式匹配
- **默认文件模式**：
  - `MF.Services*.dll`
  - `MF.Repositories*.dll`
  - `MF.Infrastructure*.dll`
  - `*Services.dll`
  - `*Repositories.dll`
  - `*Infrastructure.dll`

- **自定义文件模式**：支持通配符模式，如 `*.Services.dll`、`Plugin*.dll` 等

### 3. 智能程序集验证
- 自动验证程序集是否包含可注册的类型
- 过滤编译器生成的类型
- 避免重复加载同名程序集
- 提供详细的调试日志

### 4. 后备加载机制
- 如果动态发现失败，自动回退到传统的 `Assembly.Load` 方式
- 确保向后兼容性

## 使用方法

### 方法1：使用默认配置（推荐）

```csharp
var builder = new ContainerBuilder();
builder.RegisterModule<SingleModule>();
var container = builder.Build();
```

### 方法2：自定义搜索路径和模式

```csharp
var builder = new ContainerBuilder();

var customSearchPaths = new[]
{
    AppDomain.CurrentDomain.BaseDirectory,
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "custom_modules"),
    @"C:\MyModules"
};

var customPatterns = new[]
{
    "*.Services.dll",
    "MyApp.*.dll",
    "Plugin*.dll"
};

var customModule = new SingleModule(customSearchPaths, customPatterns);
builder.RegisterModule(customModule);
var container = builder.Build();
```

## 在不同环境中的使用

### NuGet包环境

在NuGet包中使用时，DLL文件通常会被复制到输出目录。默认配置已经包含了常见的搜索路径：

```csharp
// 默认配置即可满足大多数NuGet包场景
builder.RegisterModule<SingleModule>();
```

### Godot项目环境

在Godot项目中，可能需要搜索特定的目录：

```csharp
var godotSearchPaths = new[]
{
    AppDomain.CurrentDomain.BaseDirectory,
    Path.Combine(Environment.CurrentDirectory, "bin"),
    Path.Combine(Environment.CurrentDirectory, ".godot", "mono", "temp", "bin", "Debug")
};

var godotModule = new SingleModule(godotSearchPaths, null); // 使用默认模式
builder.RegisterModule(godotModule);
```

## 调试和故障排除

### 启用调试日志

程序集加载过程会输出详细的调试信息到 `System.Diagnostics.Debug`：

```
Discovered assembly: MF.Services from C:\MyApp\bin\MF.Services.dll
Successfully loaded assembly: MF.Services
Registered: UserService with all implemented interfaces
```

### 常见问题

1. **找不到程序集**
   - 检查搜索路径是否正确
   - 确认DLL文件存在于指定路径
   - 验证文件名模式是否匹配

2. **程序集加载失败**
   - 检查依赖项是否完整
   - 确认.NET版本兼容性
   - 查看调试日志中的错误信息

3. **类型注册失败**
   - 确认类型不是抽象类或接口
   - 检查是否标记了 `SkipRegistrationAttribute`
   - 验证类型构造函数是否可访问

## 性能考虑

- 程序集发现只在容器构建时执行一次
- 使用文件系统缓存避免重复扫描
- 智能过滤减少不必要的程序集加载
- 后备机制确保关键程序集始终可用

## 安全注意事项

- 只从受信任的目录加载程序集
- 避免从网络路径或临时目录加载
- 考虑使用代码签名验证程序集完整性

## 示例代码

完整的使用示例请参考 `example_usage/DynamicLoadingExample.cs` 文件。