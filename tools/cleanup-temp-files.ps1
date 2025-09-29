# 清理临时文件和缓存文件夹的脚本
# 基于 manual-merge-pack-final-fixed.ps1 脚本生成的临时文件

# 获取项目根目录（脚本所在目录的父目录）
$ProjectRoot = Split-Path -Path $PSScriptRoot -Parent
Set-Location $ProjectRoot

Write-Host "清理临时文件和缓存文件夹..." -ForegroundColor Green

# 定义要清理的临时目录和文件
$TempDirectories = @(
    "build-temp",           # 脚本创建的构建临时目录
    "collected-dlls",       # 脚本创建的DLL收集目录
    "output",               # 脚本创建的输出目录
    "temp_package_final",   # 早期版本的临时包目录
    "test_merge",           # 测试合并目录
    "extracted-package"     # 提取的包目录
)

$TempFiles = @(
    "build_log.txt"         # 构建日志文件
)

# 清理临时目录
foreach ($dir in $TempDirectories) {
    $fullPath = Join-Path $ProjectRoot $dir
    if (Test-Path $fullPath) {
        Write-Host "  删除临时目录: $dir" -ForegroundColor Gray
        Remove-Item $fullPath -Recurse -Force
    }
}

# 清理临时文件
foreach ($file in $TempFiles) {
    $fullPath = Join-Path $ProjectRoot $file
    if (Test-Path $fullPath) {
        Write-Host "  删除临时文件: $file" -ForegroundColor Gray
        Remove-Item $fullPath -Force
    }
}

# 清理脚本生成的 obj 和 bin 目录（如果存在）
$BuildDirs = Get-ChildItem -Directory -Recurse | Where-Object { $_.Name -eq "obj" -or $_.Name -eq "bin" }
foreach ($buildDir in $BuildDirs) {
    # 跳过 src 目录下的 obj/bin，因为它们可能是项目文件的一部分
    if ($buildDir.FullName -notlike "*\src\*") {
        Write-Host "  删除构建目录: $($buildDir.FullName)" -ForegroundColor Gray
        Remove-Item $buildDir.FullName -Recurse -Force
    }
}

Write-Host "✅ 临时文件和缓存文件夹清理完成" -ForegroundColor Green
Write-Host "保留的目录:" -ForegroundColor Cyan
Write-Host "  - .git (版本控制)" -ForegroundColor Gray
Write-Host "  - .claude (Claude配置)" -ForegroundColor Gray
Write-Host "  - .spec-workflow (规范工作流)" -ForegroundColor Gray
Write-Host "  - .vscode (VS Code配置)" -ForegroundColor Gray
Write-Host "  - src (源代码)" -ForegroundColor Gray
Write-Host "  - docs (文档)" -ForegroundColor Gray
Write-Host "  - packages (NuGet包输出)" -ForegroundColor Gray