# PowerShell脚本：批量替换命名空间
$sourceDir = "d:\GodotProjects\ModularGodot.Core\src"

# 获取所有.cs文件
$csFiles = Get-ChildItem -Path $sourceDir -Filter "*.cs" -Recurse | Where-Object { 
    $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\bin\*" 
}

Write-Host "找到 $($csFiles.Count) 个C#文件需要处理"

foreach ($file in $csFiles) {
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    
    # 替换命名空间声明
    $content = $content -replace "namespace MF\.Contracts", "namespace ModularGodot.Contracts"
    
    # 替换using语句
    $content = $content -replace "using MF\.Contracts", "using ModularGodot.Contracts"
    
    # 如果内容有变化，则写回文件
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "已更新: $($file.FullName)"
    }
}

Write-Host "命名空间替换完成！"