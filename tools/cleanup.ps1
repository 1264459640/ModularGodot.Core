#requires -version 5.1
param(
    [switch]$Verbose
)

function Write-Log {
    param([string]$Message, [string]$Color = "Gray")
    if ($Verbose) {
        Write-Host $Message -ForegroundColor $Color
    }
}

# Set paths
$ProjectRoot = "$PSScriptRoot\.."
$BuildTempDirName = $env:BUILD_TEMP_DIR
if (-not $BuildTempDirName) { $BuildTempDirName = "build_temp" }
$BuildTempDir = "$ProjectRoot\$BuildTempDirName"

$CollectedDllsDirName = $env:COLLECTED_DLLS_DIR
if (-not $CollectedDllsDirName) { $CollectedDllsDirName = "collected_dlls" }
$CollectedDllsDir = "$BuildTempDir\$CollectedDllsDirName"

$PackageOutputDir = $env:PACKAGE_OUTPUT_DIR
if (-not $PackageOutputDir) { $PackageOutputDir = "packages" }
$PackageOutputPath = "$ProjectRoot\$PackageOutputDir"

Write-Host "Starting cleanup process..." -ForegroundColor Green

# Clean build temp directory
if (Test-Path $BuildTempDir) {
    Write-Log "Removing build temp directory: $BuildTempDir"
    Remove-Item -Path $BuildTempDir -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Build temp directory cleaned" -ForegroundColor Green
} else {
    Write-Log "Build temp directory does not exist: $BuildTempDir"
}

# Clean package output directory
if (Test-Path $PackageOutputPath) {
    Write-Log "Removing package output directory: $PackageOutputPath"
    Remove-Item -Path $PackageOutputPath -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Package output directory cleaned" -ForegroundColor Green
} else {
    Write-Log "Package output directory does not exist: $PackageOutputPath"
}

# Clean bin and obj folders in src directory
Write-Host "Cleaning bin and obj folders in src directory..." -ForegroundColor Yellow
Get-ChildItem -Path "$ProjectRoot\src" -Recurse -Directory | Where-Object { $_.Name -eq "bin" -or $_.Name -eq "obj" } | ForEach-Object {
    Write-Log "Removing $($_.FullName)"
    Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "Cleanup process completed!" -ForegroundColor Green