# Simple namespace replacement script
$sourceDir = "d:\GodotProjects\ModularGodot.Core\src"
$csFiles = Get-ChildItem -Path $sourceDir -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\bin\*" }

Write-Host "Found $($csFiles.Count) C# files to process"

foreach ($file in $csFiles) {
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content
    
    $content = $content -replace "namespace MF\.Contracts", "namespace ModularGodot.Contracts"
    $content = $content -replace "using MF\.Contracts", "using ModularGodot.Contracts"
    
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Updated: $($file.Name)"
    }
}

Write-Host "Namespace replacement completed!"