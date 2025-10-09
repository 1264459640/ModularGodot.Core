#requires -version 5.1
param(
    [string]$Configuration,
    [switch]$Verbose,
    [switch]$SkipCleanup,
    [switch]$ForceRebuild,
    [string]$OutputDirectory,
    [string[]]$PackagesToBuild,
    [switch]$IncludeSymbols
)

# Function to load environment variables from .env file
function Load-EnvFile {
    param([string]$Path = "$PSScriptRoot\..\.env")

    if (Test-Path $Path) {
        Write-Host "Loading environment variables from $Path" -ForegroundColor Gray
        Get-Content $Path | ForEach-Object {
            if ($_ -match "^\s*([^#].*?)\s*=\s*(.*)$") {
                $key = $matches[1].Trim()
                $value = $matches[2].Trim()
                # Remove quotes if present
                if ($value -match '^"(.*)"$' -or $value -match "^'(.*)'$") {
                    $value = $matches[1]
                }
                Set-Item -Path "env:$key" -Value $value
            }
        }
    }
}

# Function to resolve ILRepack tool path
function Resolve-ILRepackPath {
    param([string[]]$PossiblePaths)

    foreach ($Path in $PossiblePaths) {
        if (Test-Path $Path) {
            return $Path
        }
    }

    # Try to find in PATH
    $ilRepackInPath = Get-Command "ILRepack" -ErrorAction SilentlyContinue
    if ($ilRepackInPath) {
        return $ilRepackInPath.Source
    }

    return $null
}

# Function to clean directory
function Remove-DirectorySafely {
    param([string]$Path)

    if (Test-Path $Path) {
        try {
            Remove-Item -Path $Path -Recurse -Force
            Write-VerboseOutput "Cleaned directory: $Path"
        }
        catch {
            Write-Warning "Failed to clean directory $Path`: $_"
        }
    }
}

# Load environment variables
Load-EnvFile

# Set default values from environment variables if not provided as parameters
if (-not $Configuration) { $Configuration = $env:BUILD_CONFIGURATION }
if (-not $Configuration) { $Configuration = "Release" }

if (-not $PSBoundParameters.ContainsKey('Verbose')) {
    $Verbose = [System.Convert]::ToBoolean($env:VERBOSE_OUTPUT)
}

if (-not $PSBoundParameters.ContainsKey('SkipCleanup')) {
    $SkipCleanup = -not [System.Convert]::ToBoolean($env:CLEANUP_TEMP_FILES)
}

if (-not $OutputDirectory) { $OutputDirectory = $env:PACKAGE_OUTPUT_DIR }
if (-not $OutputDirectory) { $OutputDirectory = "$PSScriptRoot\..\packages" }

# Path configuration
$ProjectRoot = "$PSScriptRoot\.."

Write-Host "Starting enhanced build and pack process..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "Output Directory: $OutputDirectory" -ForegroundColor Cyan
if ($ForceRebuild) { Write-Host "Force Rebuild: Enabled" -ForegroundColor Cyan }
if ($IncludeSymbols) { Write-Host "Include Symbols: Enabled" -ForegroundColor Cyan }

# Create necessary directories
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

# Define packages to build with their project paths and dependencies
$AllPackages = @(
    @{
        Name = "ModularGodot.Core.Contracts"
        ProjectPath = "$ProjectRoot\src\ModularGodot.Core.Contracts\ModularGodot.Core.Contracts.csproj"
        Dependencies = @()
        IsLibrary = $true
    },
    @{
        Name = "ModularGodot.Core.Contexts"
        ProjectPath = "$ProjectRoot\src\ModularGodot.Core.Contexts\ModularGodot.Core.Contexts.csproj"
        Dependencies = @("ModularGodot.Core.Contracts")
        IsLibrary = $true
    },
    @{
        Name = "ModularGodot.Core.Infrastructure"
        ProjectPath = "$ProjectRoot\src\ModularGodot.Core.Infrastructure\ModularGodot.Core.Infrastructure.csproj"
        Dependencies = @("ModularGodot.Core.Contracts", "ModularGodot.Core.Contexts")
        IsLibrary = $true
    },
    @{
        Name = "ModularGodot.Core.Repositories"
        ProjectPath = "$ProjectRoot\src\ModularGodot.Core.Repositories\ModularGodot.Core.Repositories.csproj"
        Dependencies = @("ModularGodot.Core.Contracts", "ModularGodot.Core.Contexts")
        IsLibrary = $true
    },
    @{
        Name = "ModularGodot.Core"
        ProjectPath = "$ProjectRoot\src\ModularGodot.Core\ModularGodot.Core.csproj"
        Dependencies = @("ModularGodot.Core.Contracts", "ModularGodot.Core.Contexts", "ModularGodot.Core.Infrastructure", "ModularGodot.Core.Repositories")
        IsLibrary = $false
    }
)

# Filter packages if specific ones are requested
if ($PackagesToBuild -and $PackagesToBuild.Count -gt 0) {
    $Packages = $AllPackages | Where-Object { $PackagesToBuild -contains $_.Name }
    Write-Host "Building specific packages: $($PackagesToBuild -join ', ')" -ForegroundColor Yellow
} else {
    $Packages = $AllPackages
    Write-Host "Building all packages" -ForegroundColor Yellow
}

function Write-VerboseOutput {
    param([string]$Message)
    if ($Verbose) {
        Write-Host "  $Message" -ForegroundColor Gray
    }
}

function Test-ProjectExists {
    param([string]$ProjectPath)
    return Test-Path $ProjectPath
}

function Invoke-BuildProject {
    param(
        [string]$ProjectPath,
        [string]$Configuration,
        [string]$OutputPath,
        [bool]$IncludeSymbols
    )

    $BuildArgs = @("build", $ProjectPath, "-c", $Configuration)
    if ($OutputPath) {
        $BuildArgs += "-o", $OutputPath
    }
    if (-not $Verbose) {
        $BuildArgs += "--nologo"
    }

    Write-VerboseOutput "Executing: dotnet $($BuildArgs -join ' ')"

    & dotnet $BuildArgs 2>&1 | ForEach-Object {
        if ($Verbose) { Write-Host "    $_" }
    }

    return $LASTEXITCODE -eq 0
}

function Invoke-PackProject {
    param(
        [string]$ProjectPath,
        [string]$Configuration,
        [string]$OutputPath,
        [bool]$IncludeSymbols
    )

    $PackArgs = @("pack", $ProjectPath, "-c", $Configuration, "-o", $OutputPath)
    if ($IncludeSymbols) {
        $PackArgs += "--include-symbols", "-p:SymbolPackageFormat=snupkg"
    }
    if (-not $Verbose) {
        $PackArgs += "--nologo"
    }

    Write-VerboseOutput "Executing: dotnet $($PackArgs -join ' ')"

    & dotnet $PackArgs 2>&1 | ForEach-Object {
        if ($Verbose) { Write-Host "    $_" }
    }

    return $LASTEXITCODE -eq 0
}

try {
    # Clean output directory if force rebuild
    if ($ForceRebuild -and -not $SkipCleanup) {
        Write-Host "Cleaning output directory..." -ForegroundColor Yellow
        Remove-DirectorySafely $OutputDirectory
        New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    }

    # Build each package in dependency order
    Write-Host "Building packages in dependency order..." -ForegroundColor Yellow

    $BuildSuccess = $true

    foreach ($Package in $Packages) {
        Write-Host "Processing package: $($Package.Name)" -ForegroundColor Cyan

        # Check if project file exists
        if (-not (Test-ProjectExists $Package.ProjectPath)) {
            Write-Warning "Project file not found: $($Package.ProjectPath)"
            $BuildSuccess = $false
            continue
        }

        # Build the project
        Write-Host "  Building project..." -ForegroundColor Gray
        $BuildResult = Invoke-BuildProject -ProjectPath $Package.ProjectPath -Configuration $Configuration
        if (-not $BuildResult) {
            Write-Error "Project build failed: $($Package.Name)"
            $BuildSuccess = $false
            continue
        }

        # Package the project
        Write-Host "  Creating NuGet package..." -ForegroundColor Gray
        $PackResult = Invoke-PackProject -ProjectPath $Package.ProjectPath -Configuration $Configuration -OutputPath $OutputDirectory -IncludeSymbols $IncludeSymbols
        if (-not $PackResult) {
            Write-Warning "Package creation completed with warnings or errors: $($Package.Name)"
        } else {
            Write-Host "  Package $($Package.Name) created successfully" -ForegroundColor Green
        }
    }

    # Display results
    if ($BuildSuccess) {
        Write-Host "Enhanced build and pack process completed successfully!" -ForegroundColor Green
    } else {
        Write-Warning "Enhanced build and pack process completed with some errors"
    }

    $GeneratedPackages = Get-ChildItem "$OutputDirectory\*.nupkg" -File
    if ($GeneratedPackages.Count -gt 0) {
        Write-Host "Generated packages:" -ForegroundColor Cyan
        foreach ($Package in $GeneratedPackages) {
            $PackageName = $Package.Name
            $PackageSize = [math]::Round($Package.Length / 1KB, 2)
            Write-Host "  $PackageName (Size: $PackageSize KB)" -ForegroundColor Gray
        }
    } else {
        Write-Warning "No packages were generated"
    }

    Write-Host "Packages output directory: $OutputDirectory" -ForegroundColor Cyan

    # Cleanup if not skipped
    if (-not $SkipCleanup) {
        Write-Host "Skipping temporary files cleanup (no temporary files to clean)" -ForegroundColor Yellow
    }

    exit 0
}
catch {
    Write-Error "Script execution failed: $_"
    Write-Error $_.ScriptStackTrace
    exit 1
}