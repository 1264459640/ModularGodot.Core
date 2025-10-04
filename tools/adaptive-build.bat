@echo off
REM ModularGodot.Core - 自适应跨平台构建系统 (Windows版本)
REM 支持 WSL ↔ Windows 双向开发和调试

setlocal enabledelayedexpansion

:: 颜色代码
set "GREEN=[92m"
set "YELLOW=[93m"
set "BLUE=[94m"
set "RED=[91m"
set "NC=[0m"

:: 显示帮助信息
if "%1"=="--help" goto show_help
if "%1"=="-h" goto show_help

echo %GREEN%🚀 启动 ModularGodot.Core Windows 自适应构建系统%NC%

:: 环境检测
echo %BLUE%当前环境: Windows Native%NC%

:: 参数处理
set "COMMAND=%~1"
set "ARG2=%~2"

if "%COMMAND%"=="" set "COMMAND=--win"

:: 主要逻辑跳转
if "%COMMAND%"=="--win" goto build_windows
if "%COMMAND%"=="--wsl" goto build_wsl
if "%COMMAND%"=="--switch" goto switch_env
if "%COMMAND%"=="--sync" goto sync_both
if "%COMMAND%"=="--clean" goto clean_all
if "%COMMAND%"=="--test" goto run_tests
if "%COMMAND%"=="--debug" goto debug_mode

echo %RED%❌ 未知参数: %COMMAND%%NC%
goto show_help

:show_help
echo %BLUE%ModularGodot.Core 跨平台构建系统%NC%
echo Usage: %0 [OPTION]
echo.
echo Options:
echo   %YELLOW%--win%NC%        构建并配置Windows环境 ^(默认^)
echo   %YELLOW%--wsl%NC%        构建并配置WSL兼容模式
echo   %YELLOW%--switch%NC%     从当前环境切换到另一个环境
echo   %YELLOW%--sync%NC%       双向同步构建模式
echo   %YELLOW%--clean%NC%      完全清理所有平台缓存
echo   %YELLOW%--test%NC%       运行测试
echo   %YELLOW%--debug%NC%      调试模式
echo   %YELLOW%--help%NC%       显示帮助
exit /b 0

:build_windows
echo %YELLOW%🔗 配置Windows开发环境...%NC%

:: 设置Windows环境变量
set "Platform=AnyCPU"
set "PlatformTarget=AnyCPU"
set "IsWindows=true"
set "BaseIntermediateOutputPath=obj\\win\\"
set "BaseOutputPath=bin\\win\\"

call :clean_platform windows
call :smart_build windows Debug

if "%ARG2%"=="--test" goto run_tests
if "%ARG2%"=="-t" goto run_tests

echo %GREEN%✅ Windows环境构建完成！%NC%
exit /b 0

:clean_platform
echo %YELLOW%清理 %~1 平台缓存...%NC%

:: 清理项目和bin
if exist src\ModularGodot.Core.XUnitTests\obj rd /s /q src\ModularGodot.Core.XUnitTests\obj 2>nul
if exist src\ModularGodot.Core.XUnitTests\bin rd /s /q src\ModularGodot.Core.XUnitTests\bin 2>nul

exit /b 0

:smart_build
echo %YELLOW%🏗️ 执行智能构建 ^(%~1 - %~2^)...%NC%

:: 执行构建
dotnet build src\ModularGodot.Core.sln ^
    -c %~2 ^
    -p:Platform="Any CPU" ^
    --no-restore ^
    -v minimal

if %errorlevel% neq 0 (
    echo %RED%❌ %~2 构建失败%NC%
    exit /b 1
)

echo %GREEN%✅ %~2 构建成功%NC%
exit /b 0

:run_tests
echo %YELLOW%🧪 运行测试...%NC%

dotnet test src\ModularGodot.Core.XUnitTests\ModularGodot.Core.XUnitTests.csproj ^
    --no-build ^
    --logger "console;verbosity=normal" ^
    --filter "FullyQualifiedName~ModularGodot.Core.XUnitTests.Mediator"

if %errorlevel% equ 0 (
    echo %GREEN%✅ 测试通过%NC%
) else (
    echo %RED%❌ 测试失败%NC%
    exit /b 1
)
exit /b 0

:switch_env
echo %BLUE%🔄 环境切换模式准备中...%NC%
echo %YELLOW%Windows环境下请手动切换 Rider 的 Target Framework%NC%
goto build_windows

:sync_both
echo %YELLOW%执行双向同步构建...%NC%
:: 先构建Windows版本，然后再处理兼容
goto build_windows

:clean_all
echo %YELLOW%执行完全清理...%NC%
call :clean_platform all
echo %YELLOW%清理NuGet缓存...%NC%
dotnet nuget locals all --clear
echo %GREEN%✅ 完全清理完成%NC%
exit /b 0

:debug_mode
echo %BLUE%🔍 调试模式已启用%NC%
goto build_windows

:: 错误处理
:error_handler
echo %RED%❌ 发生错误，退出码: %errorlevel%%NC%
exit /b 1

endlocal