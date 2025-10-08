#!/bin/bash

# ModularGodot.Core 包模式切换脚本
# 在包引用和项目引用之间切换

set -e  # 遇到错误时退出

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
MAGENTA='\033[0;35m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

# 默认参数
PROJECT_PATH="../src/ModularGodot.Core.Test/ModularGodot.Core.Test.csproj"
PACKAGE_SOURCE="../packages"
MODE="auto"
WHAT_IF=false

# 用法说明
usage() {
    echo -e "${CYAN}ModularGodot.Core 包模式切换工具${NC}"
    echo -e "${CYAN}==================================${NC}"
    echo
    echo "用法: $0 [选项]"
    echo
    echo "选项:"
    echo "  -p, --project <路径>      目标项目文件路径"
    echo "  -s, --source <路径>       本地包源目录路径"
    echo "  -m, --mode <模式>         切换模式 (packages|projects|auto)"
    echo "                            auto: 自动检测并切换到相反模式"
    echo "                            packages: 强制切换到包引用"
    echo "                            projects: 强制切换到项目引用"
    echo "  -w, --what-if             预览模式，不实际执行"
    echo "  -h, --help                显示此帮助信息"
    echo
    echo "默认项目路径: $PROJECT_PATH"
    echo "默认包源路径: $PACKAGE_SOURCE"
    echo "默认模式: $MODE"
    exit 0
}

# 解析参数
while [[ $# -gt 0 ]]; do
    case $1 in
        -p|--project)
            PROJECT_PATH="$2"
            shift 2
            ;;
        -s|--source)
            PACKAGE_SOURCE="$2"
            shift 2
            ;;
        -m|--mode)
            MODE="$2"
            if [[ "$MODE" != "packages" && "$MODE" != "projects" && "$MODE" != "auto" ]]; then
                echo -e "${RED}无效的模式: $MODE${NC}"
                usage
            fi
            shift 2
            ;;
        -w|--what-if)
            WHAT_IF=true
            shift
            ;;
        -h|--help)
            usage
            ;;
        *)
            echo -e "${RED}未知参数: $1${NC}"
            usage
            ;;
    esac
done

echo -e "${CYAN}ModularGodot.Core 包模式切换工具${NC}"
echo -e "${CYAN}===================================${NC}"
echo

# 检查项目文件是否存在
if [[ ! -f "$PROJECT_PATH" ]]; then
    echo -e "${RED}项目文件不存在: $PROJECT_PATH${NC}"
    exit 1
fi

# 转换为绝对路径
if [[ ! "$PROJECT_PATH" = /* ]]; then
    PROJECT_PATH="$(cd "$(dirname "$PROJECT_PATH")" && pwd)/$(basename "$PROJECT_PATH")"
fi

if [[ ! "$PACKAGE_SOURCE" = /* ]]; then
    PACKAGE_SOURCE="$(cd "$(dirname "$PACKAGE_SOURCE")" && pwd)/$(basename "$PACKAGE_SOURCE")"
fi

echo -e "${GREEN}项目路径: $PROJECT_PATH${NC}"

# 检查XML解析器是否可用
if ! command -v xmllint &> /dev/null; then
    echo -e "${RED}未找到xmllint命令，请安装libxml2-utils${NC}"
    exit 1
fi

# 读取项目文件内容来判断当前模式
PACKAGE_REFS=$(xmllint --xpath "count(//PackageReference)" "$PROJECT_PATH" 2>/dev/null || echo "0")
PROJECT_REFS=$(xmllint --xpath "count(//ProjectReference)" "$PROJECT_PATH" 2>/dev/null || echo "0")

echo -e "${CYAN}当前状态:${NC}"
echo -e "${GREEN}  包引用: $PACKAGE_REFS${NC}"
echo -e "${GREEN}  项目引用: $PROJECT_REFS${NC}"

# 自动检测模式
if [[ "$MODE" == "auto" ]]; then
    if [[ "$PACKAGE_REFS" -gt "0" ]]; then
        MODE="projects"
        echo -e "${YELLOW}检测模式: 切换到项目引用${NC}"
    elif [[ "$PROJECT_REFS" -gt "0" ]]; then
        MODE="packages"
        echo -e "${YELLOW}检测模式: 切换到包引用${NC}"
    else
        echo -e "${RED}无法检测到任何引用，项目文件可能已损坏${NC}"
        exit 1
    fi
fi

echo -e "${CYAN}目标模式: $MODE${NC}"

# 获取脚本目录
SCRIPT_DIR="$(dirname "$0")"

if [[ "$MODE" == "packages" ]]; then
    # 调用安装脚本
    echo -e "${YELLOW}切换到包引用模式...${NC}"
    INSTALL_SCRIPT="$SCRIPT_DIR/install-local-packages.sh"
    if [[ -f "$INSTALL_SCRIPT" ]]; then
        "$INSTALL_SCRIPT" -p "$PROJECT_PATH" -s "$PACKAGE_SOURCE" $([ "$WHAT_IF" == true ] && echo "-w")
    else
        echo -e "${RED}找不到安装脚本: $INSTALL_SCRIPT${NC}"
        exit 1
    fi
else
    # 调用移除脚本
    echo -e "${YELLOW}切换到项目引用模式...${NC}"
    REMOVE_SCRIPT="$SCRIPT_DIR/remove-local-packages.sh"
    if [[ -f "$REMOVE_SCRIPT" ]]; then
        "$REMOVE_SCRIPT" -p "$PROJECT_PATH" $([ "$WHAT_IF" == true ] && echo "-w")
    else
        echo -e "${RED}找不到移除脚本: $REMOVE_SCRIPT${NC}"
        exit 1
    fi
fi

echo -e "${GREEN}操作完成!${NC}"
echo -e "${GREEN}=========${NC}"

exit 0