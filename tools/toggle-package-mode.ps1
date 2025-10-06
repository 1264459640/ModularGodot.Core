# ModularGodot.Core 包模式切换脚本
# 在包引用和项目引用之间切换

param(
    [string]$ProjectPath = "../src/ModularGodot.Core.Test/ModularGodot.Core.Test.csproj",
    [string]$PackageSource = "../packages",
    [Parameter(Mandatory=$false)]
    [ValidateSet("packages", "projects", "auto")]
    [string]$Mode = "auto",
    [switch]$WhatIf
)

Write-Host "ModularGodot.Core 包模式切换工具" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan

# 检查项目文件是否存在
if (-not (Test-Path $ProjectPath)) {
    Write-Error "项目文件不存在: $ProjectPath"
    exit 1
}

Write-Host "项目路径: $ProjectPath" -ForegroundColor Green

# 读取项目文件内容来判断当前模式
[xml]$projectContent = Get-Content $ProjectPath
$hasPackageReferences = ($projectContent.Project.ItemGroup.PackageReference.Count -gt 0)
$hasProjectReferences = ($projectContent.Project.ItemGroup.ProjectReference.Count -gt 0)

Write-Host "当前状态:" -ForegroundColor Cyan
Write-Host "  包引用: $hasPackageReferences" -ForegroundColor $(if($hasPackageReferences){"Green"}else{"Gray"})
Write-Host "  项目引用: $hasProjectReferences" -ForegroundColor $(if($hasProjectReferences){"Green"}else{"Gray"})

# 自动检测模式
if ($Mode -eq "auto") {
    if ($hasPackageReferences) {
        $Mode = "projects"
        Write-Host "检测模式: 切换到项目引用" -ForegroundColor Yellow
    } elseif ($hasProjectReferences) {
        $Mode = "packages"
        Write-Host "检测模式: 切换到包引用" -ForegroundColor Yellow
    } else {
        Write-Error "无法检测到任何引用，项目文件可能已损坏"
        exit 1
    }
}

Write-Host "目标模式: $Mode" -ForegroundColor Cyan

if ($Mode -eq "packages") {
    # 调用安装脚本
    Write-Host "`n切换到包引用模式..." -ForegroundColor Yellow
    $scriptPath = Join-Path $PSScriptRoot "install-local-packages.ps1"
    if (Test-Path $scriptPath) {
        & $scriptPath -ProjectPath $ProjectPath -PackageSource $PackageSource -WhatIf:$WhatIf
    } else {
        Write-Error "找不到安装脚本: $scriptPath"
        exit 1
    }
} else {
    # 调用移除脚本
    Write-Host "`n切换到项目引用模式..." -ForegroundColor Yellow
    $scriptPath = Join-Path $PSScriptRoot "remove-local-packages.ps1"
    if (Test-Path $scriptPath) {
        & $scriptPath -ProjectPath $ProjectPath -WhatIf:$WhatIf
    } else {
        Write-Error "找不到移除脚本: $scriptPath"
        exit 1
    }
}

Write-Host "`n操作完成!" -ForegroundColor Green
Write-Host "=========" -ForegroundColor Green