# Cross-Platform Development Workflow Guide

## Problem Solved ✅

**Original Issue**: WSL中使用AIAgent开发 + Windows上用Rider调试 导致每次都需要 `dotnet clean`，严重影响开发流畅性

**Root Cause**:
- WSL环境变量 `PLATFORM=linux` 被注入MSBuild
- Windows Rider期望标准平台 (`AnyCPU/x64/x86`)
- 平台配置冲突导致依赖解析失败

## 解决方案

### 1. 统一平台配置 (Directory.Build.props)
已自动检测WSL环境并强制转换`linux` → `AnyCPU`：

```xml
<Platform Condition=" '$(Platform)' == '' or '$(Platform)' == 'linux' ">AnyCPU</Platform>
```

### 2. 跨平台工作流脚本

#### WSL环境 (AIAgent开发)
```bash
# 快速构建命令，无需clean
./tools/dev-workflow.sh

# 带测试的版本
./tools/dev-workflow.sh --test
```

#### Windows环境 (Rider调试)
```cmd
# 在Windows资源管理器中双击运行，或管理员CMD
tools\dev-workflow.bat

# 或者在PowerShell中
.\tools\dev-workflow.bat --test
```

## 实施后收益

✅ **完全消除频繁的 `dotnet clean`**
✅ **WSL AIAgent开发与Windows Rider调试无缝切换**
✅ **自动处理跨平台OBJ目录隔离**
✅ **NuGet依赖修复缓存冲突**

## 日常使用建议

### AIAgent开发流程 (WSL)
```bash
# 1. 日常使用 - 流畅开发
./tools/dev-workflow.sh

# 2. 运行具体测试
dotnet test src/ModularGodot.Core.XUnitTests/ModularGodot.Core.XUnitTests.csproj --filter "Mediator*"

# 3. Rider应该会自动刷新，如未刷新则手动Rebuild一下
```

### Windows Rider使用优化
1. 运行 dev-workflow.bat 预处理
2. 直接打开项目文件夹
3. Rider会自动识别正确的平台配置
4. 可以正常 debugging和profile

## 工作机制

### Platform强制转换 (`Directory.Build.props`)
- WSL: `PLATFORM=linux` → `PLATFORM=AnyCPU`
- Windows: 保持原有配置

### 依赖缓存隔离
```
obj/              # 通用配置
├── wsl/          # WSL特定缓存
├── win/          # Windows特定缓存
└── linux/        # 遗留Linux项目配置（被覆盖）
```

### 智能NuGet还原策略
- **WSL**: 禁用并行下载避免网络超时
- **Windows**: 标准下载策略

## 常见问题快速解决

**问题**: 还是提示"Debug|linux配置无效"
**解决**: 检查 Directory.Build.props 是否生效，或者运行一次 dev-workflow 脚本

**问题**: Rider中项目平台显示异常
**解决**: 重新生成解决方案文件，或手动在Rider设置中固定AnyCPU平台

**问题**: 测试中仍出现依赖错误
**解决**: 执行 `./tools/dev-workflow.sh --test` 强制完整重构建

---

**结论**: 这套配置通过一次性的平台统⼀策略和智能工作流脚本，将彻底解决您的跨平台开发痛点。