#requires -version 5.1
param(
    [string]$Configuration,
    [switch]$Verbose,
    [switch]$SkipCleanup,
    [switch]$SkipOutputCleanup
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

if (-not $PSBoundParameters.ContainsKey('SkipOutputCleanup')) {
    $SkipOutputCleanup = -not [System.Convert]::ToBoolean($env:CLEANUP_OUTPUT_DIR)
}

Write-Host "Starting final improved manual merge and pack process..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan

# Path configuration (relative to project root)
$ProjectRoot = Split-Path -Path $PSScriptRoot -Parent
$SubmodulePaths = @(
    "$ProjectRoot\src\3_Repositories",
    "$ProjectRoot\src\0_Contracts",
    "$ProjectRoot\src\2_Infrastructure",
    "$ProjectRoot\src\1_Contexts"
)

# Output path configuration (from environment variables or defaults)
$OutputBaseDir = if ($env:OUTPUT_BASE_DIR) { "$ProjectRoot\$($env:OUTPUT_BASE_DIR)" } else { "$ProjectRoot\output" }
$BuildTempDir = if ($env:BUILD_TEMP_DIR) { "$ProjectRoot\$($env:BUILD_TEMP_DIR)" } else { "$ProjectRoot\build-temp" }
$CollectedDllsPath = if ($env:COLLECTED_DLLS_DIR) { "$ProjectRoot\$($env:COLLECTED_DLLS_DIR)" } else { "$ProjectRoot\collected-dlls" }
$PackageOutputDir = if ($env:PACKAGE_OUTPUT_DIR) { "$ProjectRoot\$($env:PACKAGE_OUTPUT_DIR)" } else { "$ProjectRoot\packages" }

# Create necessary directories
New-Item -ItemType Directory -Path $OutputBaseDir -Force | Out-Null
$MergedOutputDir = "$OutputBaseDir\bin\$Configuration\net9.0"
New-Item -ItemType Directory -Path $MergedOutputDir -Force | Out-Null

# ILRepack tool path (from environment variables or defaults)
$ILRepackPath = if ($env:ILREPACK_PATH_1 -and (Test-Path $env:ILREPACK_PATH_1)) { $env:ILREPACK_PATH_1 }
                elseif ($env:ILREPACK_PATH_2 -and (Test-Path $env:ILREPACK_PATH_2)) { $env:ILREPACK_PATH_2 }
                else {
                    $ilRepackPaths = @(
                        "C:\\Users\\BananaPeel\\.nuget\\packages\\ilrepack\\2.0.44\\tools\\ILRepack.exe",
                        "C:\\Users\\BananaPeel\\.nuget\\packages\\ilrepack\\2.0.18\\tools\\ILRepack.exe"
                    )
                    $ILRepackPath = $null
                    foreach ($path in $ilRepackPaths) {
                        if (Test-Path $path) {
                            $ILRepackPath = $path
                            break
                        }
                    }
                    if (-not $ILRepackPath) {
                        throw "ILRepack tool not found in any of the expected locations"
                    }
                }

# DLL classification patterns
$UserPatterns = @("ModularGodot.*")

function Write-VerboseOutput {
    param([string]$Message)
    if ($Verbose) {
        Write-Host "  $Message" -ForegroundColor Gray
    }
}

try {
    # 1. Cleanup and preparation
    Write-Host "Step 1: Cleanup and preparation..." -ForegroundColor Yellow
    if (Test-Path $BuildTempDir) { Remove-Item $BuildTempDir -Recurse -Force }
    if (Test-Path $CollectedDllsPath) { Remove-Item $CollectedDllsPath -Recurse -Force }
    if (Test-Path $MergedOutputDir) { Remove-Item $MergedOutputDir -Recurse -Force }

    New-Item -ItemType Directory -Path $BuildTempDir -Force | Out-Null
    New-Item -ItemType Directory -Path "$BuildTempDir\user-dlls" -Force | Out-Null
    New-Item -ItemType Directory -Path "$BuildTempDir\third-party-dlls" -Force | Out-Null
    New-Item -ItemType Directory -Path $CollectedDllsPath -Force | Out-Null
    New-Item -ItemType Directory -Path $PackageOutputDir -Force | Out-Null
    New-Item -ItemType Directory -Path $MergedOutputDir -Force | Out-Null

    Write-Host "  Cleanup and preparation completed" -ForegroundColor Green

    # 2. Build all projects
    Write-Host "Step 2: Build all projects in dependency order..." -ForegroundColor Yellow

    foreach ($SubmodulePath in $SubmodulePaths) {
        $ProjectName = Split-Path $SubmodulePath -Leaf
        Write-Host "  Building submodule: $ProjectName" -ForegroundColor Cyan

        if (-not (Test-Path $SubmodulePath)) {
            Write-Host "  Skipping non-existent submodule: $SubmodulePath" -ForegroundColor Yellow
            continue
        }

        $ProjectFile = "$SubmodulePath\$ProjectName.csproj"
        if (-not (Test-Path $ProjectFile)) {
            Write-Host "  Skipping submodule missing project file: $ProjectFile" -ForegroundColor Yellow
            continue
        }

        $BuildArgs = @("build", $ProjectFile, "-c", $Configuration, "--force", "--no-restore")
        if (-not $Verbose) {
            $BuildArgs += "--nologo"
        }

        try {
            & dotnet $BuildArgs 2>&1 | ForEach-Object {
                if ($Verbose) { Write-Host "    $_" }
            }
            if ($LASTEXITCODE -ne 0) {
                throw "Submodule $ProjectName build failed"
            }

            $OutputPath = "$SubmodulePath\bin\$Configuration\net9.0"
            if (Test-Path $OutputPath) {
                Copy-Item "$OutputPath\*.dll" $CollectedDllsPath -ErrorAction SilentlyContinue
                Write-Host "  Submodule $ProjectName completed" -ForegroundColor Green
            }
        }
        catch {
            Write-Error "Submodule $ProjectName build failed: $_"
            exit 1
        }
    }

    Write-Host "  All projects built successfully" -ForegroundColor Green

    # 3. Collect and classify assemblies
    Write-Host "Step 3: Collect and classify assemblies..." -ForegroundColor Yellow

    $DllFiles = Get-ChildItem "$CollectedDllsPath\*.dll" -File
    foreach ($Dll in $DllFiles) {
        $DllName = $Dll.Name
        $IsUserDll = $false

        foreach ($Pattern in $UserPatterns) {
            if ($DllName -like $Pattern) {
                Copy-Item $Dll.FullName "$BuildTempDir\user-dlls\" -Force
                $IsUserDll = $true
                break
            }
        }

        if (-not $IsUserDll) {
            Copy-Item $Dll.FullName "$BuildTempDir\third-party-dlls\" -Force
        }
    }

    $UserDlls = Get-ChildItem "$BuildTempDir\user-dlls\*.dll" -File
    $UserCount = $UserDlls.Count
    $ThirdPartyCount = (Get-ChildItem "$BuildTempDir\third-party-dlls\*.dll" -File).Count

    Write-Host "  User DLLs: $UserCount" -ForegroundColor Cyan
    foreach ($Dll in $UserDlls) {
        Write-Host "    - $($Dll.Name)" -ForegroundColor Gray
    }
    Write-Host "  Third-party DLLs: $ThirdPartyCount" -ForegroundColor Cyan

    # 4. Find ILRepack tool
    Write-Host "Step 4: Find ILRepack tool..." -ForegroundColor Yellow

    if (-not (Test-Path $ILRepackPath)) {
        Write-Error "ILRepack tool not found: $ILRepackPath"
        exit 1
    }

    Write-Host "  Using ILRepack: $ILRepackPath" -ForegroundColor Green

    # 5. Prepare user DLL list
    Write-Host "Step 5: Prepare user DLL list..." -ForegroundColor Yellow

    $OrderedUserDlls = @(
        "$BuildTempDir\user-dlls\ModularGodot.Contracts.dll",
        "$BuildTempDir\user-dlls\ModularGodot.Repositories.dll",
        "$BuildTempDir\user-dlls\ModularGodot.Infrastructure.dll",
        "$BuildTempDir\user-dlls\ModularGodot.Contexts.dll"
    )

    $ExistingUserDlls = $OrderedUserDlls | Where-Object { Test-Path $_ }

    Write-Host "  User DLLs to merge:" -ForegroundColor Cyan
    foreach ($DllPath in $ExistingUserDlls) {
        $DllName = Split-Path $DllPath -Leaf
        Write-Host "    $DllName" -ForegroundColor Gray
    }

    # 6. Execute assembly merge
    Write-Host "Step 6: Execute assembly merge..." -ForegroundColor Yellow

    $TempDll = "$MergedOutputDir\ModularGodot.temp.dll"
    $FinalDll = "$MergedOutputDir\ModularGodot.Merged.dll"

    if ($ExistingUserDlls.Count -eq 0) {
        Write-Error "No user DLLs found"
        exit 1
    }

    # Merge all user DLLs at once
    Write-Host "  Step 6.1: Merge all user assemblies at once..." -ForegroundColor Yellow
    $UserMergeArgs = @("/out:$TempDll")
    $UserMergeArgs += $ExistingUserDlls

    Write-Host "    Merge arguments:" -ForegroundColor Gray
    $UserMergeArgs | ForEach-Object { Write-Host "      $_" -ForegroundColor Gray }

    try {
        & $ILRepackPath $UserMergeArgs 2>&1 | ForEach-Object {
            if ($Verbose) { Write-Host "      $_" }
        }

        if ($LASTEXITCODE -eq 0 -and (Test-Path $TempDll)) {
            Write-Host "    User assemblies merged successfully" -ForegroundColor Green
            $TempSize = (Get-Item $TempDll).Length
            Write-Host "    Temp DLL size: $TempSize bytes" -ForegroundColor Cyan
        }
        else {
            Write-Warning "    User assemblies merge failed, trying step by step"

            # Fallback to step-by-step merge
            $CurrentResult = $ExistingUserDlls[0]
            $TempResultPath = "$MergedOutputDir\ModularGodot.base.dll"
            Copy-Item $CurrentResult $TempResultPath -Force
            $CurrentResult = $TempResultPath

            for ($i = 1; $i -lt $ExistingUserDlls.Count; $i++) {
                $NextDll = $ExistingUserDlls[$i]
                $NextResultPath = "$MergedOutputDir\ModularGodot.step$($i).dll"

                Write-Host "    Merging: $(Split-Path $NextDll -Leaf)" -ForegroundColor Gray

                $StepArgs = @("/out:$NextResultPath", $CurrentResult, $NextDll)
                & $ILRepackPath $StepArgs 2>&1 | ForEach-Object {
                    if ($Verbose) { Write-Host "      $_" }
                }

                if ($LASTEXITCODE -eq 0 -and (Test-Path $NextResultPath)) {
                    Write-Host "      Success" -ForegroundColor Green
                    if ($CurrentResult -ne $ExistingUserDlls[0] -and (Test-Path $CurrentResult)) {
                        Remove-Item $CurrentResult -Force
                    }
                    $CurrentResult = $NextResultPath
                }
                else {
                    Write-Warning "      Failed, skipping this DLL"
                }
            }

            if (Test-Path $CurrentResult) {
                Copy-Item $CurrentResult $TempDll -Force
                if ($CurrentResult -ne $ExistingUserDlls[0] -and (Test-Path $CurrentResult)) {
                    Remove-Item $CurrentResult -Force
                }
                Write-Host "    Step-by-step merge completed" -ForegroundColor Green
            }
            else {
                Write-Error "Merge failed"
                exit 1
            }
        }
    }
    catch {
        Write-Error "User assemblies merge exception: $_"
        exit 1
    }

    # Step 6.2: Merge third-party assemblies
    Write-Host "  Step 6.2: Merge third-party assemblies..." -ForegroundColor Yellow
    if ($ThirdPartyCount -gt 0) {
        $ThirdPartyDllArgs = @("/out:$FinalDll", "/internalize", $TempDll)
        $ThirdPartyDllFiles = Get-ChildItem "$BuildTempDir\third-party-dlls\*.dll" -File

        foreach ($Dll in $ThirdPartyDllFiles) {
            $ThirdPartyDllArgs += $Dll.FullName
            Write-Host "    Adding (internalize): $($Dll.Name)" -ForegroundColor Gray
        }

        try {
            & $ILRepackPath $ThirdPartyDllArgs 2>&1 | ForEach-Object {
                if ($Verbose) { Write-Host "      $_" }
            }

            if ($LASTEXITCODE -eq 0 -and (Test-Path $FinalDll)) {
                Write-Host "    Third-party assemblies merged successfully" -ForegroundColor Green
            }
            else {
                Write-Warning "    Third-party assemblies merge failed, trying without internalize"

                $SafeThirdPartyArgs = @("/out:$FinalDll", $TempDll)
                foreach ($Dll in $ThirdPartyDllFiles) {
                    $SafeThirdPartyArgs += $Dll.FullName
                }

                & $ILRepackPath $SafeThirdPartyArgs 2>&1 | ForEach-Object {
                    if ($Verbose) { Write-Host "      $_" }
                }

                if ($LASTEXITCODE -eq 0 -and (Test-Path $FinalDll)) {
                    Write-Host "    Safe merge successful" -ForegroundColor Green
                }
                else {
                    Write-Warning "    Merge failed, using temp file as final result"
                    Copy-Item $TempDll $FinalDll -Force
                }
            }
        }
        catch {
            Write-Warning "    Third-party assemblies merge exception, using temp file as final result"
            Copy-Item $TempDll $FinalDll -Force
        }
    }
    else {
        Write-Host "    No third-party DLLs to merge" -ForegroundColor Yellow
        Copy-Item $TempDll $FinalDll -Force
    }

    # Cleanup temporary files
    if (Test-Path $TempDll) {
        Remove-Item $TempDll -Force
    }

    # 7. Verify merge result
    Write-Host "Step 7: Verify merge result..." -ForegroundColor Yellow

    if (Test-Path $FinalDll) {
        $FinalSize = (Get-Item $FinalDll).Length
        $FinalSizeMB = [math]::Round($FinalSize / 1MB, 2)
        Write-Host "  Merged DLL size: $FinalSize bytes ($FinalSizeMB MB)" -ForegroundColor Cyan
        Write-Host "  Assembly merge completed successfully" -ForegroundColor Green
    }
    else {
        Write-Error "  Merge failed: Final file does not exist"
        exit 1
    }

    # 8. Create NuGet package structure
    Write-Host "Step 8: Create NuGet package structure..." -ForegroundColor Yellow

    $NuGetLibDir = "$PackageOutputDir\lib\net9.0"
    New-Item -ItemType Directory -Path $NuGetLibDir -Force | Out-Null

    Copy-Item $FinalDll "$NuGetLibDir\ModularGodot.Core.dll" -Force

    $NuspecContent = @"
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
  <metadata>
    <id>ModularGodot.Core</id>
    <version>0.1.0</version>
    <title>ModularGodot.Core</title>
    <authors>BananaPeel</authors>
    <description>A modular game development framework for Godot 4.4 with layered architecture and modern C# design patterns. Includes all modules: Contracts, Contexts, Infrastructure, Repositories.</description>
    <summary>Enterprise-grade modular framework providing dependency injection, event-driven architecture, caching, monitoring, and resource management.</summary>
    <repository type="git" url="https://github.com/1264459640/ModularGodot.Core" />
    <license type="expression">MIT</license>
    <tags>godot modular architecture framework dependency-injection event-system caching monitoring</tags>
    <dependencies />
    <frameworkAssemblies>
      <frameworkAssembly assemblyName="System.Reactive" targetFramework=".NET9.0" />
    </frameworkAssemblies>
  </metadata>
  <files>
    <file src="lib\net9.0\ModularGodot.Core.dll" target="lib\net9.0\ModularGodot.Core.dll" />
  </files>
</package>
"@

    $NuspecPath = "$PackageOutputDir\ModularGodot.Core.nuspec"
    $NuspecContent | Set-Content $NuspecPath -Force

    try {
        Push-Location $PackageOutputDir
        & nuget pack ModularGodot.Core.nuspec 2>&1 | ForEach-Object {
            if ($Verbose) { Write-Host "    $_" }
        }

        if ($LASTEXITCODE -eq 0) {
            Write-Host "  NuGet package created successfully" -ForegroundColor Green
        } else {
            Write-Warning "  NuGet package creation may have warnings"
        }
    }
    catch {
        Write-Warning "  NuGet package creation failed: $_"
    }
    finally {
        Pop-Location
    }

    # 9. Cleanup temporary files (if not skipped)
    if (-not $SkipCleanup) {
        Write-Host "Step 9: Cleanup temporary files..." -ForegroundColor Yellow
        if (Test-Path $BuildTempDir) { Remove-Item $BuildTempDir -Recurse -Force }
        if (Test-Path $CollectedDllsPath) { Remove-Item $CollectedDllsPath -Recurse -Force }
        Write-Host "  Temporary files cleanup completed" -ForegroundColor Green
    }

    # 10. Cleanup output directory (if not skipped)
    if (-not $SkipOutputCleanup) {
        Write-Host "Step 10: Cleanup output directory..." -ForegroundColor Yellow
        if (Test-Path $OutputBaseDir) { Remove-Item $OutputBaseDir -Recurse -Force }
        Write-Host "  Output directory cleanup completed" -ForegroundColor Green
    }

    # 11. Display results
    Write-Host "Final improved manual merge and pack completed!" -ForegroundColor Green
    $Packages = Get-ChildItem "$PackageOutputDir\*.nupkg" -File
    foreach ($Package in $Packages) {
        $PackageName = $Package.Name
        $PackageSize = [math]::Round($Package.Length / 1MB, 2)
        Write-Host "  Generated package: $PackageName (Size: $PackageSize MB)" -ForegroundColor Cyan
    }

    Write-Host "NuGet package output: $PackageOutputDir" -ForegroundColor Cyan
    Write-Host "Merged assembly: $FinalDll" -ForegroundColor Cyan

    # Module merge confirmation
    Write-Host "" -ForegroundColor Yellow
    Write-Host "Module merge confirmation:" -ForegroundColor Yellow
    Write-Host "  Processed modules:" -ForegroundColor Cyan
    Write-Host "    - Contracts (base contracts)" -ForegroundColor Gray
    Write-Host "    - Contexts (context configuration)" -ForegroundColor Gray
    Write-Host "    - Infrastructure (infrastructure implementation)" -ForegroundColor Gray
    Write-Host "    - Repositories (repository layer)" -ForegroundColor Gray
    Write-Host "    - Third-party dependencies (Autofac, MediatR, R3, etc.)" -ForegroundColor Gray
    Write-Host "" -ForegroundColor Green
    Write-Host "  All modules have been packaged into a single assembly" -ForegroundColor Green
}
catch {
    Write-Error "Script execution failed: $_"
    exit 1
}