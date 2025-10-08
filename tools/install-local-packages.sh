#!/bin/bash

# ModularGodot.Core 本地包安装脚本
# 用于将项目引用切换为本地NuGet包引用

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
WHAT_IF=false
FORCE=false
LATEST_MODE=false

# 用法说明
usage() {
    echo -e "${CYAN}ModularGodot.Core 本地包安装工具${NC}"
    echo -e "${CYAN}=================================${NC}"
    echo
    echo "用法: $0 [选项]"
    echo
    echo "选项:"
    echo "  -p, --project <路径>      目标项目文件路径"
    echo "  -s, --source <路径>       本地包源目录路径"
    echo "  -l, --latest              自动获取最新构建的包版本"
    echo "  -w, --what-if             预览模式，不实际执行"
    echo "  -f, --force               不确认直接执行"
    echo "  -h, --help                显示此帮助信息"
    echo
    echo "默认项目路径: $PROJECT_PATH"
    echo "默认包源路径: $PACKAGE_SOURCE"
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
        -l|--latest)
            LATEST_MODE=true
            shift
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

echo -e "${CYAN}ModularGodot.Core 本地包安装工具${NC}"
echo -e "${CYAN}=================================${NC}"
echo

# 检查项目文件是否存在
if [[ ! -f "$PROJECT_PATH" ]]; then
    echo -e "${RED}项目文件不存在: $PROJECT_PATH${NC}"
    exit 1
fi

# 转换为绝对路径（如果不是）
if [[ ! "$PROJECT_PATH" = /* ]]; then
    PROJECT_PATH="$(cd "$(dirname "$PROJECT_PATH")" && pwd)/$(basename "$PROJECT_PATH")"
fi

if [[ ! "$PACKAGE_SOURCE" = /* ]]; then
    PACKAGE_SOURCE="$(cd "$(dirname "$PACKAGE_SOURCE")" && pwd)/$(basename "$PACKAGE_SOURCE")"
fi

# 检查包源目录是否存在
if [[ ! -d "$PACKAGE_SOURCE" ]]; then
    echo -e "${RED}包源目录不存在: $PACKAGE_SOURCE${NC}"
    exit 1
fi

echo -e "${GREEN}项目路径: $PROJECT_PATH${NC}"
echo -e "${GREEN}包源路径: $PACKAGE_SOURCE${NC}"
echo

# 获取所有本地包
PACKAGES=($(find "$PACKAGE_SOURCE" -name "*.nupkg" -type f | sort))

if [[ ${#PACKAGES[@]} -eq 0 ]]; then
    echo -e "${RED}在 $PACKAGE_SOURCE 中未找到任何NuGet包${NC}"
    exit 1
fi

echo -e "${YELLOW}发现 ${#PACKAGES[@]} 个本地包:${NC}"
for package in "${PACKAGES[@]}"; do
    echo -e "${GRAY}  - $(basename "$package")${NC}"
done

# 备份原始项目文件
BACKUP_PATH="$PROJECT_PATH.backup"
if [[ "$WHAT_IF" == false ]]; then
    echo -e "${YELLOW}正在创建项目文件备份...${NC}"
    cp "$PROJECT_PATH" "$BACKUP_PATH"
    echo -e "${GREEN}备份已创建: $BACKUP_PATH${NC}"
fi

# 检查XML解析器是否可用
if ! command -v xmllint &> /dev/null; then
    echo -e "${RED}未找到xmllint命令，请安装libxml2-utils${NC}"
    exit 1
fi

# 读取项目文件内容
echo -e "${YELLOW}正在分析项目文件...${NC}"

# 显示当前引用状态
echo -e "${CYAN}当前项目引用状态:${NC}"

# 检查现有包引用
PACKAGE_REFS=$(xmllint --xpath "count(//PackageReference)" "$PROJECT_PATH" 2>/dev/null || echo "0")
if [[ "$PACKAGE_REFS" -gt "0" ]]; then
    echo -e "${YELLOW}现有包引用:${NC}"
    xmllint --xpath "//PackageReference/@Include" "$PROJECT_PATH" 2>/dev/null | tr ' ' '\n' | while read -r line; do
        if [[ -n "$line" ]]; then
            echo -e "${GRAY}  - ${line#Include=}${NC}"
        fi
    done
else
    echo -e "${GRAY}无现有包引用${NC}"
fi

# 检查现有项目引用
PROJECT_REFS=$(xmllint --xpath "count(//ProjectReference)" "$PROJECT_PATH" 2>/dev/null || echo "0")
if [[ "$PROJECT_REFS" -gt "0" ]]; then
    echo -e "${YELLOW}现有项目引用:${NC}"
    xmllint --xpath "//ProjectReference/@Include" "$PROJECT_PATH" 2>/dev/null | tr ' ' '\n' | while read -r line; do
        if [[ -n "$line" ]]; then
            echo -e "${GRAY}  - ${line#Include=}${NC}"
        fi
    done
else
    echo -e "${GRAY}无现有项目引用${NC}"
fi

# 提取包名称和版本
declare -A PACKAGE_INFO
echo -e "${CYAN}将要安装的包:${NC}"

# 如果是latest模式，显示版本获取方式
if [[ "$LATEST_MODE" == true ]]; then
    echo -e "${YELLOW}模式: 自动获取最新构建的包版本${NC}"
else
    echo -e "${YELLOW}模式: 使用文件扫描获取包版本${NC}"
fi

# 处理不同模式下的包版本获取
if [[ "$LATEST_MODE" == true ]]; then
    # 最新模式：查找每个包名的最新版本
    for package_base_name in "ModularGodot.Core.Contracts" "ModularGodot.Core.Contexts" "ModularGodot.Core.Infrastructure" "ModularGodot.Core.Repositories" "ModularGodot.Core"; do
        # 查找该包名的所有版本，按修改时间排序取最新
        latest_package=$(find "$PACKAGE_SOURCE" -name "${package_base_name}*.nupkg" -type f -printf '%T@ %p\n' | sort -n | tail -1 | cut -d' ' -f2-)
        if [[ -n "$latest_package" ]]; then
            filename=$(basename "$latest_package")
            clean_name="${filename%.nupkg}"

            # 提取版本号
            if [[ "$clean_name" =~ ^"${package_base_name}"\.([0-9]+\.[0-9]+\.[0-9]+.*)$ ]]; then
                package_version="${BASH_REMATCH[1]}"
                PACKAGE_INFO["$package_base_name"]="$package_version"
                echo -e "${GREEN}  - $package_base_name v$package_version (最新)${NC}"
            fi
        else
            echo -e "${RED}未找到包: $package_base_name${NC}"
        fi
    done
else
    # 原始模式：处理所有发现的包文件
    for package in "${PACKAGES[@]}"; do
        filename=$(basename "$package")
        clean_name="${filename%.nupkg}"

        # 提取包名和版本（假设格式为 Package.Name.Version.nupkg）
        if [[ "$clean_name" =~ ^(.*)\.([0-9]+\.[0-9]+\.[0-9]+.*)$ ]]; then
            package_name="${BASH_REMATCH[1]}"
            package_version="${BASH_REMATCH[2]}"
            PACKAGE_INFO["$package_name"]="$package_version"
            echo -e "${GREEN}  - $package_name v$package_version${NC}"
        fi
    done
fi

if [[ "$WHAT_IF" == true ]]; then
    echo -e "${MAGENTA}[预览模式] 显示将要进行的更改，但不会实际执行${NC}"
    exit 0
fi

# 如果没有Force参数，要求用户确认
if [[ "$FORCE" == false ]]; then
    echo
    read -p "是否继续安装这些包？(Y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo -e "${RED}操作已取消${NC}"
        exit 0
    fi
fi

echo -e "${YELLOW}正在安装本地包...${NC}"

# 创建临时文件用于XML处理
TEMP_FILE=$(mktemp)
cp "$PROJECT_PATH" "$TEMP_FILE"

# 备份当前的项目引用
CURRENT_PROJECT_REFS=$(xmllint --xpath "//ProjectReference" "$PROJECT_PATH" 2>/dev/null || echo "")

# 添加或更新包引用
for package_name in "${!PACKAGE_INFO[@]}"; do
    package_version="${PACKAGE_INFO[$package_name]}"

    # 检查是否已存在该包引用
    existing_ref=$(xmllint --xpath "count(//PackageReference[@Include='$package_name'])" "$TEMP_FILE" 2>/dev/null || echo "0")

    if [[ "$existing_ref" -gt "0" ]]; then
        # 更新现有包引用版本
        echo -e "${GREEN}已更新包引用: $package_name v$package_version${NC}"
    else
        # 添加新包引用
        # 找到ItemGroup或创建新的
        itemgroup_exists=$(xmllint --xpath "count(//ItemGroup[PackageReference])" "$TEMP_FILE" 2>/dev/null || echo "0")
        if [[ "$itemgroup_exists" -eq "0" ]]; then
            # 创建新的ItemGroup
            sed -i '/<\/Project>/i\    <ItemGroup>\n    </ItemGroup>' "$TEMP_FILE"
        fi

        # 在第一个ItemGroup中添加PackageReference
        sed -i '/<ItemGroup>/{:a; /PackageReference/!{N; ba}; s/\(<ItemGroup>\)/\1\n        <PackageReference Include="'$package_name'" Version="'$package_version'" />/}' "$TEMP_FILE"
        echo -e "${GREEN}已添加包引用: $package_name v$package_version${NC}"
    fi
done

# 配置本地包源
restore_sources=$(xmllint --xpath "count(//RestoreSources)" "$TEMP_FILE" 2>/dev/null || echo "0")
if [[ "$restore_sources" -eq "0" ]]; then
    # 创建新的PropertyGroup
    sed -i '/<\/Project>/i\    <PropertyGroup>\n        <RestoreSources>$(RestoreSources);'"$PACKAGE_SOURCE"'<\/RestoreSources>\n    </PropertyGroup>' "$TEMP_FILE"
    echo -e "${GREEN}已添加本地包源配置: $PACKAGE_SOURCE${NC}"
fi

# 移除项目引用
if [[ -n "$CURRENT_PROJECT_REFS" ]]; then
    # 备份项目引用到临时文件
    echo "$CURRENT_PROJECT_REFS" > "$TEMP_FILE.projectrefs"
    echo -e "${YELLOW}已备份项目引用，稍后可以恢复${NC}"
fi

# 移除ProjectReference节点
sed -i '/<ProjectReference/d' "$TEMP_FILE"

# 验证XML格式
if ! xmllint --noout "$TEMP_FILE" 2>/dev/null; then
    echo -e "${RED}XML格式验证失败，放弃更改${NC}"
    rm "$TEMP_FILE"
    exit 1
fi

# 替换原文件
mv "$TEMP_FILE" "$PROJECT_PATH"
echo -e "${GREEN}项目文件已更新: $PROJECT_PATH${NC}"

# 尝试还原包
echo -e "${YELLOW}正在还原NuGet包...${NC}"
cd "$(dirname "$PROJECT_PATH")"
if dotnet restore "$PROJECT_PATH" 2>&1; then
    echo -e "${GREEN}包还原成功!${NC}"
else
    echo -e "${RED}包还原失败，可能需要手动运行 'dotnet restore'${NC}"
fi

echo -e "${CYAN}操作完成!${NC}"
echo -e "${CYAN}=============${NC}"
echo -e "${GRAY}备份文件: $BACKUP_PATH${NC}"
echo -e "${GRAY}如需要恢复，请运行: cp '$BACKUP_PATH' '$PROJECT_PATH'${NC}"

# 显示最终状态
echo -e "${CYAN}最终项目引用状态:${NC}"
final_packages=$(xmllint --xpath "count(//PackageReference)" "$PROJECT_PATH" 2>/dev/null || echo "0")
if [[ "$final_packages" -gt "0" ]]; then
    xmllint --xpath "//PackageReference/@Include" "$PROJECT_PATH" 2>/dev/null | tr ' ' '\n' | while read -r line; do
        if [[ -n "$line" ]]; then
            echo -e "${GREEN}  ✓ ${line#Include=}${NC}"
        fi
    done
fi

# 清理临时文件
rm -f "$TEMP_FILE"*

exit 0