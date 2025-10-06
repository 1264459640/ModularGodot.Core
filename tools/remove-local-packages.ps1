# ModularGodot.Core 包移除脚本
# 用于将包引用切换回项目引用

param(
    [string]$ProjectPath = "../src/ModularGodot.Core.Test/ModularGodot.Core.Test.csproj",
    [switch]$WhatIf,
    [switch]$Force
)

Write-Host "ModularGodot.Core 包移除工具" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan

# 检查项目文件是否存在
if (-not (Test-Path $ProjectPath)) {
    Write-Error "项目文件不存在: $ProjectPath"
    exit 1
}

Write-Host "项目路径: $ProjectPath" -ForegroundColor Green

# 读取项目文件内容
[xml]$projectContent = Get-Content $ProjectPath

# 获取包引用和项目引用节点
$packageReferences = $projectContent.Project.ItemGroup.PackageReference
$projectReferences = $projectContent.Project.ItemGroup.ProjectReference

Write-Host "`n当前引用状态:" -ForegroundColor Cyan

# 显示现有的包引用
if ($packageReferences.Count -gt 0) {
    Write-Host "现有包引用:" -ForegroundColor Yellow
    foreach ($ref in $packageReferences) {
        Write-Host "  - $($ref.Include) v$($ref.Version)" -ForegroundColor Gray
    }
}

# 从备份文件获取原始项目引用（如果存在）
$backupPath = "$ProjectPath.backup"
$originalProjectRefs = @()
if (Test-Path $backupPath) {
    [xml]$backupContent = Get-Content $backupPath
    $backupProjectRefs = $backupContent.Project.ItemGroup.ProjectReference
    foreach ($ref in $backupProjectRefs) {
        $originalProjectRefs += $ref.Include
        Write-Host "  - 备份中的项目引用: $($ref.Include)" -ForegroundColor DarkGray
    }
} else {
    Write-Host "未找到备份文件，将使用默认项目引用路径" -ForegroundColor Yellow
    $originalProjectRefs = @(
        "..\ModularGodot.Core.Contexts\ModularGodot.Core.Contexts.csproj",
        "..\ModularGodot.Core.Contracts\ModularGodot.Core.Contracts.csproj",
        "..\ModularGodot.Core.Infrastructure\ModularGodot.Core.Infrastructure.csproj",
        "..\ModularGodot.Core.Repositories\ModularGodot.Core.Repositories.csproj"
    )
}

if ($WhatIf) {
    Write-Host "`n[WhatIf模式] 显示将要进行的更改，但不会实际执行" -ForegroundColor Magenta
    exit 0
}

# 如果没有Force参数，要求用户确认
if (-not $Force) {
    $confirmation = Read-Host "`n是否继续移除这些包引用？(Y/N)"
    if ($confirmation -ne 'Y' -and $confirmation -ne 'y') {
        Write-Host "操作已取消" -ForegroundColor Red
        exit 0
    }
}

Write-Host "`n正在移除包引用..." -ForegroundColor Yellow

# 移除包引用节点
$removedPackages = @()
if ($packageReferences.Count -gt 0) {
    foreach ($packageRef in $packageReferences) {
        $removedPackages += "$($packageRef.Include) v$($packageRef.Version)"
        $packageRef.ParentNode.RemoveChild($packageRef) | Out-Null
    }
    Write-Host "已移除 $($packageReferences.Count) 个包引用:" -ForegroundColor Yellow
    foreach ($pkg in $removedPackages) {
        Write-Host "  - $pkg" -ForegroundColor Red
    }
}

# 添加项目引用
$itemGroup = $projectContent.Project.ItemGroup | Where-Object { $_.ProjectReference }
if (-not $itemGroup) {
    $itemGroup = $projectContent.CreateElement("ItemGroup")
    $projectContent.Project.AppendChild($itemGroup) | Out-Null
}

foreach ($projectRefPath in $originalProjectRefs) {
    # 检查项目文件是否存在
    $fullPath = Join-Path (Split-Path $ProjectPath -Parent) $projectRefPath
    if (Test-Path $fullPath) {
        $projectRef = $projectContent.CreateElement("ProjectReference")
        $projectRef.SetAttribute("Include", $projectRefPath)
        $itemGroup.AppendChild($projectRef) | Out-Null
        Write-Host "已添加项目引用: $projectRefPath" -ForegroundColor Green
    } else {
        Write-Warning "项目文件不存在，跳过: $projectRefPath"
    }
}

# 移除本地包源配置（可选）
$restoreSourcesGroup = $projectContent.Project.PropertyGroup | Where-Object { $_.RestoreSources }
if ($restoreSourcesGroup -and ($Force -or (Read-Host "是否移除本地包源配置？(Y/N)") -eq 'Y')) {
    $projectContent.Project.RemoveChild($restoreSourcesGroup) | Out-Null
    Write-Host "已移除本地包源配置" -ForegroundColor Green
}

# 保存项目文件
$projectContent.Save($ProjectPath)
Write-Host "`n项目文件已更新: $ProjectPath" -ForegroundColor Green

Write-Host "`n操作完成!" -ForegroundColor Cyan
Write-Host "=============" -ForegroundColor Cyan

# 显示最终状态
Write-Host "`n最终项目引用状态:" -ForegroundColor Cyan
[xml]$finalContent = Get-Content $ProjectPath
$finalProjectReferences = $finalContent.Project.ItemGroup.ProjectReference
if ($finalProjectReferences.Count -gt 0) {
    foreach ($ref in $finalProjectReferences) {
        Write-Host "  ✓ $($ref.Include)" -ForegroundColor Green
    }
}

Write-Host "`n提示:" -ForegroundColor Yellow
Write-Host "  - 请运行 'dotnet restore' 来确保依赖项正确还原" -ForegroundColor Gray
Write-Host "  - 如果需要恢复，可以手动复制备份文件: $backupPath" -ForegroundColor Gray