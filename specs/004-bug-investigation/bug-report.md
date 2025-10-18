# 错误报告：事件总线和单元测试失败

本文档记录了在测试过程中发现的两个相关问题。

## 问题1：事件总线测试期间执行挂起

### 症状

运行测试时，应用程序会卡住并且无响应，但不会崩溃。

### 初步分析

该问题被怀疑源于事件总线的实现。挂起问题在运行与事件总线相关的测试时尤其明显。

### 相关文件

*   **测试场景:** `src/ModularGodot.Core.Test/Scenes/EventBusTestScene.cs`
*   **可疑实现:** `src/ModularGodot.Core.Infrastructure/Messaging/R3EventBus.cs`

## 问题2：单元测试结果不一致

### 症状

所有单元测试在通过 Rider IDE 单独执行时均能成功通过。但是，当通过 `dotnet test` 命令运行整个测试套件时，多个测试失败。

### 初步分析

这种差异表明问题与测试执行环境有关。Rider 测试运行器和 `dotnet test` 命令行环境之间的测试上下文或设置可能存在差异。问题初步追溯到测试上下文的设置。

### 相关文件

*   **可疑位置:** `src/ModularGodot.Core.XUnitTests/TestInfrastructure/TestContext.cs`

## 后续步骤

1.  分析 `R3EventBus` 的实现，查找潜在的死锁或无限循环。
2.  检查 `EventBusTestScene`，了解它是如何触发挂起行为的。
3.  比较 Rider 和 `dotnet test` 之间的测试执行环境和配置。
4.  检查 `TestContext.cs`，识别在通过 `dotnet test` 运行时可能导致失败的任何特定于环境的代码。
