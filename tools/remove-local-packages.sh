#!/bin/bash

# ModularGodot.Core 包移除脚本
# 用于将包引用切换回项目引用

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
WHAT_IF=false
FORCE=false

# 用法说明
usage() {
    echo -e "${CYAN}ModularGodot.Core 包移除工具${NC}"
    echo -e "${CYAN}================================${NC}"
    echo
    echo "用法: $0 [选项]"
    echo
    echo "选项:"
    echo "  -p, --project <路径>      目标项目文件路径"
    echo "  -w, --what-if             预览模式，不实际执行"
    echo "  -f, --force               不确认直接执行"
    echo "  -h, --help                显示此帮助信息"
    echo
    echo "默认项目路径: $PROJECT_PATH"
    exit 0
}

# 解析参数
while [[ $# -gt 0 ]]; do
    case $1 in
        -p|--project)
            PROJECT_PATH="$2"
            shift 2
            ;;
        -w|--what-if)
            WHAT_IF=true
            shift
            ;;
        -f|--force)
            FORCE=true
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

echo -e "${CYAN}ModularGodot.Core 包移除工具${NC}"
echo -e "${CYAN}================================${NC}"
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

echo -e "${GREEN}项目路径: $PROJECT_PATH${NC}"
echo

# 检查XML解析器是否可用
if ! command -v xmllint &> /dev/null; then
    echo -e "${RED}未找到xmllint命令，请安装libxml2-utils${NC}"
    exit 1
fi

# 读取项目文件内容
echo -e "${YELLOW}正在分析项目文件...${NC}"

# 显示当前引用状态
echo -e "${CYAN}当前引用状态:${NC}"

# 检查现有包引用
PACKAGE_REFS=$(xmllint --xpath "count(//PackageReference)" "$PROJECT_PATH" 2>/dev/null || echo "0")
if [[ "$PACKAGE_REFS" -gt "0" ]]; then
    echo -e "${YELLOW}现有包引用:${NC}"
    xmllint --xpath "//PackageReference" "$PROJECT_PATH" 2>/dev/null | while IFS= read -r line; do
        if [[ -n "$line" ]]; then
            include=$(echo "$line" | grep -o 'Include="[^"]*"' | cut -d'"' -f2)
            version=$(echo "$line" | grep -o 'Version="[^"]*"' | cut -d'"' -f2)
            if [[ -n "$include" ]]; then
                echo -e "${GRAY}  - $include v${version:-unknown}${NC}"
            fi
        fi
    done
else
    echo -e "${GRAY}无现有包引用${NC}"
fi

# 从备份文件获取原始项目引用
BACKUP_PATH="$PROJECT_PATH.backup"
ORIGINAL_PROJECT_REFS=()

if [[ -f "$BACKUP_PATH" ]]; then
    echo -e "${YELLOW}发现备份文件，提取原始项目引用...${NC}"
    BACKUP_REFS=$(xmllint --xpath "//ProjectReference/@Include" "$BACKUP_PATH" 2>/dev/null || echo "")
    if [[ -n "$BACKUP_REFS" ]]; then
        echo "$BACKUP_REFS" | tr ' ' '\n' | while read -r line; do
            if [[ -n "$line" ]]; then
                ref=${line#Include=}
                echo -e "${GRAY}  - 备份中的项目引用: $ref${NC}"
            fi
        done

        # 读取备份中的引用到数组
        while IFS= read -r line; do
            if [[ -n "$line" ]]; then
                ref=${line#Include=}
                ref=${ref%\"}
                ref=${ref#\"}
                ORIGINAL_PROJECT_REFS+=("$ref")
            fi
        done <<< "$(echo "$BACKUP_REFS" | tr ' ' '\n')"
    fi
else
    echo -e "${YELLOW}未找到备份文件，将使用默认项目引用路径${NC}"
    ORIGINAL_PROJECT_REFS=(
        "..\ModularGodot.Core.Contexts\ModularGodot.Core.Contexts.csproj"
        "..\ModularGodot.Core.Contracts\ModularGodot.Core.Contracts.csproj"
        "..\ModularGodot.Core.Infrastructure\ModularGodot.Core.Infrastructure.csproj"
        "..\ModularGodot.Core.Repositories\ModularGodot.Core.Repositories.csproj"
    )
fi

if [[ "$WHAT_IF" == true ]]; then
    echo -e "${MAGENTA}[预览模式] 显示将要进行的更改，但不会实际执行${NC}"
    exit 0
fi

# 如果没有Force参数，要求用户确认
if [[ "$FORCE" == false ]]; then
    echo
    read -p "是否继续移除这些包引用？(Y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo -e "${RED}操作已取消${NC}"
        exit 0
    fi
fi

echo -e "${YELLOW}正在移除包引用...${NC}"

# 创建临时文件用于XML处理
TEMP_FILE=$(mktemp)
cp "$PROJECT_PATH" "$TEMP_FILE"

# 统计要移除的包数量
REMOVED_COUNT=0
if [[ "$PACKAGE_REFS" -gt "0" ]]; then
    REMOVED_COUNT=$PACKAGE_REFS
fi

# 移除包引用节点
if [[ "$PACKAGE_REFS" -gt "0" ]]; then
    # 备份包引用信息
    REMOVED_PACKAGES=$(xmllint --xpath "//PackageReference" "$TEMP_FILE" 2>/dev/null)
    echo "$REMOVED_PACKAGES" > "$TEMP_FILE.packages"

    # 移除所有PackageReference节点
    sed -i '/<PackageReference/d' "$TEMP_FILE"
    echo -e "${YELLOW}已移除 $REMOVED_COUNT 个包引用${NC}"
fi

# 添加项目引用
if [[ ${#ORIGINAL_PROJECT_REFS[@]} -gt 0 ]]; then
    echo -e "${YELLOW}正在添加项目引用...${NC}"

    # 找到或创建ItemGroup
    itemgroup_exists=$(xmllint --xpath "count(//ItemGroup[ProjectReference])" "$TEMP_FILE" 2>/dev/null || echo "0")
    if [[ "$itemgroup_exists" -eq "0" ]]; then
        # 创建新的ItemGroup
        sed -i '/<\/Project>/i\    <ItemGroup>\n    </ItemGroup>' "$TEMP_FILE"
    fi

    # 添加项目引用
    for project_ref_path in "${ORIGINAL_PROJECT_REFS[@]}"; do
        # 检查项目文件是否存在
        full_path="$(dirname "$PROJECT_PATH")/$project_ref_path"
        if [[ -f "$full_path" ]]; then
            # 在ItemGroup中添加ProjectReference
            sed -i '/<ItemGroup>/a\        <ProjectReference Include="'$project_ref_path'" />' "$TEMP_FILE"
            echo -e "${GREEN}已添加项目引用: $project_ref_path${NC}"
        else
            echo -e "${YELLOW}警告: 项目文件不存在，跳过: $project_ref_path${NC}"
        fi
    done
else
    echo -e "${YELLOW}未找到原始项目引用${NC}"
fi

# 询问是否移除本地包源配置
if [[ "$FORCE" == true ]]; then
    REMOVE_SOURCES=true
elif [[ $(xmllint --xpath "count(//RestoreSources)" "$TEMP_FILE" 2>/dev/null || echo "0") -gt "0" ]]; then
    echo
    read -p "是否移除本地包源配置？(Y/N) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        REMOVE_SOURCES=true
    else
        REMOVE_SOURCES=false
    fi
else
    REMOVE_SOURCES=false
fi

# 移除本地包源配置
if [[ "$REMOVE_SOURCES" == true ]]; then
    # 移除包含RestoreSources的PropertyGroup
    sed -i '/<PropertyGroup>/,/<\/PropertyGroup>/{ /RestoreSources/d; }' "$TEMP_FILE"
    sed -i '/^[[:space:]]*$/d' "$TEMP_FILE"  # 移除空行
    echo -e "${GREEN}已移除本地包源配置${NC}"
fi

# 验证XML格式
if ! xmllint --noout "$TEMP_FILE" 2>/dev/null; then
    echo -e "${RED}XML格式验证失败，放弃更改${NC}"
    rm "$TEMP_FILE"
    exit 1
fi

# 替换原文件
mv "$TEMP_FILE" "$PROJECT_PATH"
echo -e "${GREEN}项目文件已更新: $PROJECT_PATH${NC}"

echo -e "${CYAN}操作完成!${NC}"
echo -e "${CYAN}=============${NC}"

# 显示最终状态
echo -e "${CYAN}最终项目引用状态:${NC}"
final_projects=$(xmllint --xpath "count(//ProjectReference)" "$PROJECT_PATH" 2>/dev/null || echo "0")
if [[ "$final_projects" -gt "0" ]]; then
    xmllint --xpath "//ProjectReference/@Include" "$PROJECT_PATH" 2>/dev/null | tr ' ' '\n' | while read -r line; do
        if [[ -n "$line" ]]; then
            echo -e "${GREEN}  ✓ ${line#Include=}${NC}"
        fi
    done
fi

echo
echo -e "${YELLOW}提示:${NC}"
echo -e "${GRAY}  - 请运行 'dotnet restore' 来确保依赖项正确还原${NC}"
echo -e "${GRAY}  - 如果需要恢复，可以手动复制备份文件: $BACKUP_PATH${NC}"
echo -e "${GRAY}  - 使用命令: cp '$BACKUP_PATH' '$PROJECT_PATH'${NC}"

# 清理临时文件
rm -f "$TEMP_FILE"*

exit 0