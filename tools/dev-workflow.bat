@echo off
REM ModularGodot.Core - Windows Rider 开发工作流优化脚本
REM 用于在 Windows 环境下准备项目，避免平台冲突

echo 🚀 启动 ModularGodot.Core Windows 开发工作流优化
echo.

REM 检测是否在 Windows 环境下
if "%OS%"=="Windows_NT" (
    echo ✅ 检测到 Windows 环境
) else (
    echo ⚠️  非 Windows 环境，可能无法正常工作
)

REM 清理旧的构建缓存
echo 🧹 清理旧的构建缓存...
if exist src\ModularGodot.Core.XUnitTests\obj rmdir /s /q src\ModularGodot.Core.XUnitTests\obj 2>nul
if exist src\ModularGodot.Core.XUnitTests\bin rmdir /s /q src\ModularGodot.Core.XUnitTests\bin 2>nul

REM 统一平台配置
echo 🔧 设置统一平台配置 (AnyCPU)...
set Platform=AnyCPU
set PlatformTarget=AnyCPU
set MSBUILDTERMNODEPATH=

REM 智能 NuGet 还原
echo 📦 执行 NuGet 还原...
dotnet restore src\ModularGodot.Core.sln -p:Platform="Any CPU" --disable-parallel

if %errorlevel% neq 0 (
    echo ❌ NuGet 还原失败
    exit /b 1
)

echo ✅ NuGet 还原完成

REM 执行构建
echo 🏗️ 执行构建...
dotnet build src\ModularGodot.Core.sln -c Debug -p:Platform="Any CPU" --no-restore

if %errorlevel% neq 0 (
    echo ❌ 构建失败
    exit /b 1
)

echo ✅ 构建完成

REM 运行测试（如指定）
if "%1"=="--test" goto run_tests
if "%1"=="-t" goto run_tests

echo 🎉 工作流优化完成！
echo 💡 提示：现在可以在 Rider 中直接打开项目而无需 clean
exit /b 0

:run_tests
echo 🧪 运行测试...
dotnet test src\ModularGodot.Core.XUnitTests\ModularGodot.Core.XUnitTests.csproj --no-build --logger "console;verbosity=normal"
goto end

:end
echo 🎉 工作流优化完成！
echo 💡 提示：此脚本已自动处理平台冲突，Rider 应该可以正常工作了