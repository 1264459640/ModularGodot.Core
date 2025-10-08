param($installPath, $toolsPath, $package, $project)

# Get the project directory
$projectDir = Split-Path -Path $project.FullName -Parent

# Remove AutoLoads directory
$autoLoadsDir = Join-Path $projectDir "AutoLoads"
if (Test-Path $autoLoadsDir) {
    # Remove files from project
    Get-ChildItem -Path $autoLoadsDir -Recurse -File | ForEach-Object {
        try {
            $projectItem = $project.ProjectItems.Item($_.Name)
            if ($projectItem) {
                $projectItem.Remove()
            }
        } catch {
            # Ignore errors if file is not in project
        }
    }

    # Remove the directory
    Remove-Item $autoLoadsDir -Recurse -Force
}

Write-Host "AutoLoad files have been removed from the project."