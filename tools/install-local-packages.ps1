# ModularGodot.Core 本地包安装脚本
# 用于将项目引用切换为本地NuGet包引用

param(
    [string]$ProjectPath = "../src/ModularGodot.Core.Test/ModularGodot.Core.Test.csproj",
    [string]$PackageSource = "../packages",
    [switch]$WhatIf,
    [switch]$Force
)

Write-Host "ModularGodot.Core 本地包安装工具" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# 检查项目文件是否存在
if (-not (Test-Path $ProjectPath)) {
    Write-Error "项目文件不存在: $ProjectPath"
    exit 1
}

# 检查包源目录是否存在
if (-not (Test-Path $PackageSource)) {
    Write-Error "包源目录不存在: $PackageSource"
    exit 1
}

Write-Host "项目路径: $ProjectPath" -ForegroundColor Green
Write-Host "包源路径: $PackageSource" -ForegroundColor Green

# 获取所有本地包
$packages = Get-ChildItem -Path $PackageSource -Filter "*.nupkg" | Sort-Object Name

if ($packages.Count -eq 0) {
    Write-Error "在 $PackageSource 中未找到任何NuGet包"
    exit 1
}

Write-Host "发现 $($packages.Count) 个本地包:" -ForegroundColor Yellow
foreach ($package in $packages) {
    Write-Host "  - $($package.Name)" -ForegroundColor Gray
}

# 备份原始项目文件
$backupPath = "$ProjectPath.backup"
if (-not $WhatIf) {
    Write-Host "创建项目文件备份..." -ForegroundColor Yellow
    Copy-Item -Path $ProjectPath -Destination $backupPath -Force
    Write-Host "备份已创建: $backupPath" -ForegroundColor Green
}

# 读取项目文件内容
[xml]$projectContent = Get-Content $ProjectPath

# 获取包引用节点
$packageReferences = $projectContent.Project.ItemGroup.PackageReference
$projectReferences = $projectContent.Project.ItemGroup.ProjectReference

Write-Host "`n当前项目引用状态:" -ForegroundColor Cyan

# 显示现有的包引用
if ($packageReferences.Count -gt 0) {
    Write-Host "现有包引用:" -ForegroundColor Yellow
    foreach ($ref in $packageReferences) {
        Write-Host "  - $($ref.Include) v$($ref.Version)" -ForegroundColor Gray
    }
} else {
    Write-Host "无现有包引用" -ForegroundColor Gray
}

# 显示现有的项目引用
if ($projectReferences.Count -gt 0) {
    Write-Host "现有项目引用:" -ForegroundColor Yellow
    foreach ($ref in $projectReferences) {
        Write-Host "  - $($ref.Include)" -ForegroundColor Gray
    }
} else {
    Write-Host "无现有项目引用" -ForegroundColor Gray
}

# 提取包名称和版本
$packageInfo = @()
foreach ($package in $packages) {
    $fileName = $package.Name -replace '\.nupkg$', ''
    if ($fileName -match '^(.*)\.([\d\.]+)$') {
        $packageName = $Matches[1]
        $packageVersion = $Matches[2]
        $packageInfo += @{
            Name = $packageName
            Version = $packageVersion
            FileName = $package.Name
        }
    }
}

Write-Host "`n将要安装的包:" -ForegroundColor Cyan
foreach ($pkg in $packageInfo) {
    Write-Host "  - $($pkg.Name) v$($pkg.Version)" -ForegroundColor Green
}

if ($WhatIf) {
    Write-Host "`n[WhatIf模式] 显示将要进行的更改，但不会实际执行" -ForegroundColor Magenta
    exit 0
}

# 如果没有Force参数，要求用户确认
if (-not $Force) {
    $confirmation = Read-Host "`n是否继续安装这些包？(Y/N)"
    if ($confirmation -ne 'Y' -and $confirmation -ne 'y') {
        Write-Host "操作已取消" -ForegroundColor Red
        exit 0
    }
}

Write-Host "`n正在安装本地包..." -ForegroundColor Yellow

# 配置本地包源
$restoreSourcesProperty = $projectContent.Project.PropertyGroup.RestoreSources | Where-Object { $_ }
if (-not $restoreSourcesProperty) {
    # 创建新的PropertyGroup
    $newPropertyGroup = $projectContent.CreateElement("PropertyGroup")
    $restoreSourcesElement = $projectContent.CreateElement("RestoreSources")
    $restoreSourcesElement.InnerText = "`$(RestoreSources);$PackageSource"
    $newPropertyGroup.AppendChild($restoreSourcesElement) | Out-Null
    $projectContent.Project.AppendChild($newPropertyGroup) | Out-Null
    Write-Host "已添加本地包源配置: $PackageSource" -ForegroundColor Green
}

# 移除项目引用节点
if ($projectReferences.Count -gt 0) {
    foreach ($projectRef in $projectReferences) {
        $projectRef.ParentNode.RemoveChild($projectRef) | Out-Null
        Write-Host "已移除项目引用: $($projectRef.Include)" -ForegroundColor Yellow
    }
}

# 添加包引用
$itemGroup = $projectContent.Project.ItemGroup | Where-Object { $_.PackageReference }
if (-not $itemGroup) {
    $itemGroup = $projectContent.CreateElement("ItemGroup")
    $projectContent.Project.AppendChild($itemGroup) | Out-Null
}

foreach ($pkg in $packageInfo) {
    # 检查是否已存在该包引用
    $existingRef = $packageReferences | Where-Object { $_.Include -eq $pkg.Name }
    if ($existingRef) {
        # 更新版本
        $existingRef.Version = $pkg.Version
        Write-Host "已更新包引用: $($pkg.Name) v$($pkg.Version)" -ForegroundColor Green
    } else {
        # 添加新包引用
        $packageRef = $projectContent.CreateElement("PackageReference")
        $packageRef.SetAttribute("Include", $pkg.Name)
        $packageRef.SetAttribute("Version", $pkg.Version)
        $itemGroup.AppendChild($packageRef) | Out-Null
        Write-Host "已添加包引用: $($pkg.Name) v$($pkg.Version)" -ForegroundColor Green
    }
}

# 保存项目文件
$projectContent.Save($ProjectPath)
Write-Host "`n项目文件已更新: $ProjectPath" -ForegroundColor Green

# 尝试还原包
Write-Host "`n正在还原NuGet包..." -ForegroundColor Yellow
try {
    $nugetRestore = & dotnet restore $ProjectPath 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "包还原成功!" -ForegroundColor Green
    } else {
        Write-Warning "包还原失败，可能需要手动运行 'dotnet restore'"
        Write-Host "错误信息: $nugetRestore" -ForegroundColor Red
    }
} catch {
    Write-Warning "包还原失败: $($_.Exception.Message)"
    Write-Host "请手动运行 'dotnet restore $ProjectPath'" -ForegroundColor Yellow
}

Write-Host "`n操作完成!" -ForegroundColor Cyan
Write-Host "=============" -ForegroundColor Cyan
Write-Host "备份文件: $backupPath" -ForegroundColor Gray
Write-Host "如需要恢复，请运行: Copy-Item -Path `"$backupPath`" -Destination `"$ProjectPath`" -Force" -ForegroundColor Gray

# 显示最终状态
Write-Host "`n最终项目引用状态:" -ForegroundColor Cyan
[xml]$finalContent = Get-Content $ProjectPath
$finalPackageReferences = $finalContent.Project.ItemGroup.PackageReference
if ($finalPackageReferences.Count -gt 0) {
    foreach ($ref in $finalPackageReferences) {
        Write-Host "  ✓ $($ref.Include) v$($ref.Version)" -ForegroundColor Green
    }
}