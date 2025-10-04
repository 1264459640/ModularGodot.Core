#!/bin/bash

# ModularGodot.Core - 自适应跨平台构建系统
# 支持 WSL ↔ Windows 双向开发和调试

set -e

# 颜色输出
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 显示帮助信息
show_help() {
    echo -e "${BLUE}ModularGodot.Core 跨平台构建系统${NC}"
    echo -e "Usage: $0 [OPTION]"
    echo -e ""
    echo -e "Options:"
    echo -e "  ${YELLOW}--wsl${NC}        构建并配置WSL环境 (默认)"
    echo -e "  ${YELLOW}--win${NC}        构建并配置Windows环境"
    echo -e "  ${YELLOW}--switch${NC}     环境切换模式 (WSL↔Windows)"
    echo -e "  ${YELLOW}--sync${NC}       双向同步模式"
    echo -e "  ${YELLOW}--test${NC}       运行测试"
    echo -e "  ${YELLOW}--debug${NC}      调试模式"
    echo -e "  ${YELLOW}--clean${NC}      完全清理所有平台缓存"
    echo -e "  ${YELLOW}--help${NC}       显示帮助"
    exit 0
}

# 检测环境
detect_environment() {
    if grep -qi microsoft /proc/version && [ -n "$WSL_DISTRO_NAME" ]; then
        echo "wsl"
    else
        echo "native"
    fi
}

# 清理指定平台的缓存
clean_platform_cache() {
    local platform=$1
    echo -e "${YELLOW}清理 ${platform} 平台缓存...${NC}"

    # 清理所有项目的obj和bin
    find ./src -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true
    find ./src -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true

    # 清理NuGet缓存（可选）
    if [[ "$platform" == "all" ]]; then
        echo -e "${YELLOW}清理NuGet缓存...${NC}"
        dotnet nuget locals all --clear
    fi
}

# 构建WSL环境
build_wsl() {
    echo -e "${BLUE}🔗 配置WSL开发环境...${NC}"

    export Platform="AnyCPU"
    export PlatformTarget="AnyCPU"
    export IsWSL=true

    # WSL专用目录
    export BaseIntermediateOutputPath="obj/wsl/"
    export BaseOutputPath="bin/wsl/"

    echo -e "${GREEN}✅ WSL环境配置完成${NC}"
}

# 构建Windows环境
build_windows() {
    echo -e "${BLUE}🔗 配置Windows开发环境...${NC}"

    export Platform="AnyCPU"
    export PlatformTarget="AnyCPU"
    export IsWindows=true

    # Windows专用目录
    export BaseIntermediateOutputPath="obj/win/"
    export BaseOutputPath="bin/win/"

    echo -e "${GREEN}✅ Windows环境配置完成${NC}"
}

# 智能构建
smart_build() {
    local env_type=$1
    local config=${2:-Debug}

    echo -e "${YELLOW}🏗️  执行智能构建 (${env_type} - ${config})...${NC}"

    if [[ "$env_type" == "wsl" ]]; then
        build_wsl
    else
        build_windows
    fi

    # 修复NuGet还原错误 - 先清空旧缓存
    echo -e "${BLUE}🧹 清理旧的平台冲突缓存...${NC}"
    find ./src -name "*.nuget.g.props" -exec rm -f {} \; 2>/dev/null || true
    find ./src -name "*.nuget.g.targets" -exec rm -f {} \; 2>/dev/null || true

    # NuGet 还原
    echo -e "${YELLOW}📦 执行NuGet还原...${NC}"
    dotnet restore src/ModularGodot.Core.sln \
        -p:Platform="Any CPU" \
        --disable-parallel \
        --ignore-failed-sources

    # 执行构建（移除--no-restore，因为之前已经 Restore）
    dotnet build src/ModularGodot.Core.sln \
        -c "$config" \
        -p:Platform="Any CPU" \
        -v minimal

    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ ${config} 构建成功${NC}"
    else
        echo -e "${RED}❌ ${config} 构建失败${NC}"
        exit 1
    fi
}

# 运行测试
run_tests() {
    local env_type=$1
    echo -e "${YELLOW}🧪 运行测试 (${env_type})...${NC}"

    dotnet test src/ModularGodot.Core.XUnitTests/ModularGodot.Core.XUnitTests.csproj \
        --no-build \
        --logger "console;verbosity=normal" \
        --filter "FullyQualifiedName~ModularGodot.Core.XUnitTests.Mediator"

    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ 测试通过${NC}"
    else
        echo -e "${RED}❌ 测试失败${NC}"
        exit 1
    fi
}

# 环境切换
switch_environment() {
    echo -e "${BLUE}🔄 执行环境切换模式...${NC}"

    detect_environment
    current_env=$(detect_environment)

    clean_platform_cache "$current_env"

    if [[ "$current_env" == "wsl" ]]; then
        echo -e "${YELLOW}从WSL切换到Windows模式...${NC}"
        build_windows
        smart_build "windows"
    else
        echo -e "${YELLOW}从Windows切换到WSL模式...${NC}"
        build_wsl
        smart_build "wsl"
    fi
}

# 主函数
main() {
    local command=${1:-"--wsl"}
    local env_type=$(detect_environment)

    echo -e "${GREEN}🚀 启动 ModularGodot.Core 自适应跨平台构建系统${NC}"
    echo -e "${BLUE}当前环境: $env_type${NC}"

    case $command in
        --help|-h)
            show_help
            ;;
        --wsl)
            build_wsl
            clean_platform_cache "wsl"
            smart_build "wsl" "Debug"
            if [[ "${2:-}" == "--test" ]]; then
                run_tests "wsl"
            fi
            ;;
        --win)
            build_windows
            clean_platform_cache "windows"
            smart_build "windows" "Debug"
            if [[ "${2:-}" == "--test" ]]; then
                run_tests "windows"
            fi
            ;;
        --switch)
            switch_environment
            ;;
        --sync)
            echo -e "${YELLOW}执行双向同步构建...${NC}"
            build_wsl
            smart_build "wsl" "Debug"
            build_windows
            smart_build "windows" "Debug"
            ;;
        --clean)
            clean_platform_cache "all"
            echo -e "${GREEN}✅ 完全清理完成${NC}"
            ;;
        --test)
            run_tests "$env_type"
            ;;
        --debug)
            set -x
            build_wsl
            smart_build "wsl" "Debug"
            ;;
        *)
            echo -e "${RED}❌ 未知参数: $command${NC}"
            show_help
            ;;
    esac

    echo -e "${GREEN}🎉 构建完成！环境已切换到 ${env_type} 模式${NC}"
}

# 执行主函数
main "$@"