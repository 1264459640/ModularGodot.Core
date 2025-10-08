param($installPath, $toolsPath, $package, $project)

# Get the project directory
$projectDir = Split-Path -Path $project.FullName -Parent

# Create AutoLoads directory if it doesn't exist
$autoLoadsDir = Join-Path $projectDir "AutoLoads"
if (!(Test-Path $autoLoadsDir)) {
    New-Item -ItemType Directory -Path $autoLoadsDir | Out-Null
}

# Copy AutoLoad files from package to project
$packageAutoLoadsDir = Join-Path $installPath "tools\AutoLoads"
if (Test-Path $packageAutoLoadsDir) {
    Get-ChildItem -Path $packageAutoLoadsDir -Recurse -File | ForEach-Object {
        $relativePath = $_.FullName.Substring($packageAutoLoadsDir.Length + 1)
        $destinationPath = Join-Path $autoLoadsDir $relativePath

        # Create subdirectory if needed
        $destinationDir = Split-Path $destinationPath -Parent
        if (!(Test-Path $destinationDir)) {
            New-Item -ItemType Directory -Path $destinationDir | Out-Null
        }

        # Copy file
        Copy-Item $_.FullName $destinationPath -Force
    }
}

# Add copied files to project
Get-ChildItem -Path $autoLoadsDir -Recurse -File | ForEach-Object {
    $relativePath = $_.FullName.Substring($projectDir.Length + 1)
    try {
        $project.ProjectItems.AddFromFile($_.FullName) | Out-Null
    } catch {
        # File might already be in project, ignore error
    }
}

Write-Host "AutoLoad files have been copied to the project's AutoLoads directory."