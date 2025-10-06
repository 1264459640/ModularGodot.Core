# ModularGodot.Core 包管理工具

这个目录包含了用于管理ModularGodot.Core项目引用模式的PowerShell脚本工具。

## 脚本说明

### install-local-packages.ps1
将项目引用切换为本地NuGet包引用。

**用法:**
```powershell
./install-local-packages.ps1 [-ProjectPath <路径>] [-PackageSource <包源路径>] [-WhatIf] [-Force]
```

**参数:**
- `-ProjectPath`: 目标项目文件路径（默认: `../src/ModularGodot.Core.Test/ModularGodot.Core.Test.csproj`）
- `-PackageSource`: 本地包源目录路径（默认: `../packages`）
- `-WhatIf`: 预览模式，显示将要进行的更改但不执行
- `-Force`: 不确认直接执行

### remove-local-packages.ps1
将包引用切换回项目引用。

**用法:**
```powershell
./remove-local-packages.ps1 [-ProjectPath <路径>] [-WhatIf] [-Force]
```

**参数:**
- `-ProjectPath`: 目标项目文件路径
- `-WhatIf`: 预览模式
- `-Force`: 不确认直接执行

### toggle-package-mode.ps1
在包引用和项目引用之间智能切换。

**用法:**
```powershell
./toggle-package-mode.ps1 [-ProjectPath <路径>] [-PackageSource <包源路径>] [-Mode <模式>] [-WhatIf]
```

**参数:**
- `-Mode`: 切换模式（`packages`, `projects`, `auto`）
  - `auto`: 自动检测当前模式并切换到相反模式（默认）
  - `packages`: 强制切换到包引用
  - `projects`: 强制切换到项目引用

## 使用示例

### 基本用法
```powershell
# 切换到包引用（自动检测）
./toggle-package-mode.ps1

# 切换到项目引用
./toggle-package-mode.ps1 -Mode projects

# 预览模式
./install-local-packages.ps1 -WhatIf

# 强制切换，不确认
./remove-local-packages.ps1 -Force
```

### 高级用法
```powershell
# 指定项目和包源路径
./install-local-packages.ps1 -ProjectPath "../MyProject/MyProject.csproj" -PackageSource "../nupkgs"

# 恢复到项目引用
./remove-local-packages.ps1 -ProjectPath "../MyProject/MyProject.csproj"
```

## 工作流程

1. **开发模式**: 使用项目引用，便于调试和开发
2. **测试模式**: 使用包引用，测试发布的包
3. **发布模式**: 使用正式版本的包引用

## 注意事项

- 每次切换都会自动创建备份文件（`.backup`）
- 脚本会检查文件路径的有效性
- 包还原失败时需要手动运行`dotnet restore`
- 建议在使用前理解项目的依赖关系

## 依赖关系

项目包依赖顺序：
```
ModularGodot.Core.Repositories
  ↓ Depends on
ModularGodot.Core.Infrastructure
  ↓ Depends on
ModularGodot.Core.Contexts
  ↓ Depends on
ModularGodot.Core.Contracts
```