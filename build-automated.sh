#!/bin/bash

set -e

CONFIGURATION="Release"
PROJECT_ROOT="$(pwd)"
VERBOSE="no"

# 解析参数
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        -v|--verbose)
            VERBOSE="yes"
            shift
            ;;
        -h|--help)
            echo "用法: $0 [选项]"
            echo "选项:"
            echo "  -c, --configuration CONFIG  构建配置 (默认: Release)"
            echo "  -v, --verbose               详细输出"
            echo "  -h, --help                 显示帮助"
            exit 0
            ;;
        *)
            echo "未知参数: $1"
            exit 1
            ;;
    esac
done

echo "🚀 开始自动化程序集合并构建流程..."
echo "配置: $CONFIGURATION"

SUBMODULE_PATHS=(
    "$PROJECT_ROOT/src/0_Contracts"
    "$PROJECT_ROOT/src/1_Contexts"
    "$PROJECT_ROOT/src/2_Infrastructure"
    "$PROJECT_ROOT/src/3_Repositories"
)

CORE_PROJECT_PATH="$PROJECT_ROOT/src/ModularGodot.Core"
BUILD_TEMP_DIR="$PROJECT_ROOT/build-temp"
MERGED_OUTPUT_DIR="$CORE_PROJECT_PATH/bin/$CONFIGURATION/net9.0"
COLLECTED_DLLS_PATH="$PROJECT_ROOT/collected-dlls"

# 清理和准备
echo "🔄 清理和准备临时目录..."
rm -rf "$BUILD_TEMP_DIR"
rm -rf "$COLLECTED_DLLS_PATH"
mkdir -p "$BUILD_TEMP_DIR/user-dlls"
mkdir -p "$BUILD_TEMP_DIR/third-party-dlls"
mkdir -p "$COLLECTED_DLLS_PATH"
mkdir -p "$(dirname "$MERGED_OUTPUT_DIR")"

echo "🔄 步骤 1: 构建所有项目..."

# 构建所有子模块
for SUBMODULE_PATH in "${SUBMODULE_PATHS[@]}"; do
    PROJECT_NAME=$(basename "$SUBMODULE_PATH")
    echo "  构建子模块: $PROJECT_NAME"

    if [ ! -d "$SUBMODULE_PATH" ]; then
        echo "  跳过不存在的子模块: $SUBMODULE_PATH"
        continue
    fi

    if [ "$VERBOSE" = "yes" ]; then
        dotnet build "$SUBMODULE_PATH/$(basename "$SUBMODULE_PATH").csproj" -c "$CONFIGURATION" --force --no-restore
    else
        dotnet build "$SUBMODULE_PATH/$(basename "$SUBMODULE_PATH").csproj" -c "$CONFIGURATION" --force --no-restore > /dev/null 2>&1
    fi

    if [ $? -ne 0 ]; then
        echo "❌ 子模块 $PROJECT_NAME 构建失败"
        exit 1
    fi

    # 复制 DLL 到收集目录
    OUTPUT_PATH="$SUBMODULE_PATH/bin/$CONFIGURATION/net9.0"
    if [ -d "$OUTPUT_PATH" ]; then
        cp "$OUTPUT_PATH"/*.dll "$COLLECTED_DLLS_PATH/" 2>/dev/null || true
        echo "  ✅ 子模块 $PROJECT_NAME 完成"
    fi
done

# 构建核心项目
echo "  构建核心项目: ModularGodot.Core"
cd "$CORE_PROJECT_PATH"

if [ "$VERBOSE" = "yes" ]; then
    dotnet build -c "$CONFIGURATION" --force --no-restore
else
    dotnet build -c "$CONFIGURATION" --force --no-restore > /dev/null 2>&1
fi

if [ $? -ne 0 ]; then
    echo "❌ 核心项目构建失败"
    exit 1
fi

# 复制核心 DLL 到收集目录
CORE_OUTPUT_PATH="bin/$CONFIGURATION/net9.0"
if [ -d "$CORE_OUTPUT_PATH" ]; then
    cp "$CORE_OUTPUT_PATH"/*.dll "$COLLECTED_DLLS_PATH/" 2>/dev/null || true
fi

echo "🔄 步骤 2: 收集和分类程序集..."

# 定义用户DLL和第三方DLL的模式
USER_PATTERNS=("ModularGodot.*")
THIRD_PARTY_PATTERNS=(
    "GodotSharp*"
    "Autofac*"
    "MediatR*"
    "Microsoft.Extensions*"
    "System.Reactive*"
    "R3*"
    "Serilog*"
    "netstandard*"
)

# 收集DLL并进行分类
for DLL in "$COLLECTED_DLLS_PATH"/*.dll; do
    if [ -f "$DLL" ]; then
        DLL_NAME=$(basename "$DLL")
        IS_USER_DLL=false

        # 检查是否为用户DLL
        for PATTERN in "${USER_PATTERNS[@]}"; do
            if [[ "$DLL_NAME" == $PATTERN ]]; then
                cp "$DLL" "$BUILD_TEMP_DIR/user-dlls/"
                IS_USER_DLL=true
                break
            fi
        done

        # 检查是否为第三方DLL
        if [ "$IS_USER_DLL" = false ]; then
            for PATTERN in "${THIRD_PARTY_PATTERNS[@]}"; do
                if [[ "$DLL_NAME" == $PATTERN ]]; then
                    cp "$DLL" "$BUILD_TEMP_DIR/third-party-dlls/"
                    break
                fi
            done
        fi
    fi
done

# 统计
USER_COUNT=$(ls -1 "$BUILD_TEMP_DIR/user-dlls/"*.dll 2>/dev/null | wc -l)
THIRD_PARTY_COUNT=$(ls -1 "$BUILD_TEMP_DIR/third-party-dlls/"*.dll 2>/dev/null | wc -l)

echo "  用户DLL: $USER_COUNT 个"
echo "  第三方DLL: $THIRD_PARTY_COUNT 个"

echo "🔄 步骤 3: 查找ILRepack工具..."

# 查找ILRepack工具
ILREPACK_CMD=""

# 尝试方法1: 全局工具
if command -v ilrepack > /dev/null 2>&1; then
    ILREPACK_CMD="ilrepack"
    echo "  使用全局ILRepack工具"
else
    # 尝试方法2: 项目依赖
    ILREPACK_PATH=$(find "$USERPROFILE"/.nuget/packages/ilrepack -name "ilrepack.exe" 2>/dev/null | sort -V | tail -1)
    if [ -n "$ILREPACK_PATH" ]; then
        ILREPACK_CMD="$ILREPACK_PATH"
        echo "  使用项目依赖ILRepack: $ILREPACK_PATH"
    else
        echo "❌ 未找到ILRepack工具"
        echo "  请运行: dotnet tool install -g ILRepack"
        exit 1
    fi
fi

echo "🔄 步骤 4: 执行程序集合并..."

# 准备文件路径
CORE_DLL="$MERGED_OUTPUT_DIR/ModularGodot.Core.dll"
TEMP_DLL="$MERGED_OUTPUT_DIR/ModularGodot.Core.temp.dll"
FINAL_DLL="$MERGED_OUTPUT_DIR/ModularGodot.Core.Merged.dll"

# 确保核心DLL存在
if [ ! -f "$CORE_DLL" ]; then
    echo "❌ 核心DLL不存在: $CORE_DLL"
    exit 1
fi

echo "  核心DLL大小: $(stat -c%s "$CORE_DLL") 字节"

# 步骤4.1: 合并用户程序集 (保持公共API)
echo "  步骤4.1: 合并用户程序集..."
if [ "$USER_COUNT" -gt 0 ]; then
    USER_DLL_ARGS=("$CORE_DLL")
    for DLL in "$BUILD_TEMP_DIR/user-dlls"/*.dll; do
        if [ "$(basename "$DLL")" != "ModularGodot.Core.dll" ]; then
            USER_DLL_ARGS+=("$DLL")
            echo "    添加: $(basename "$DLL")"
        fi
    done

    # 使用Windows路径格式 (重要!)
    WIN_TEMP_DLL=$(cygpath -w "$TEMP_DLL")
    WIN_USER_DLL_ARGS=()
    for DLL in "${USER_DLL_ARGS[@]}"; do
        WIN_USER_DLL_ARGS+=("$(cygpath -w "$DLL")")
    done

    if "$ILREPACK_CMD" /out:"$WIN_TEMP_DLL" "${WIN_USER_DLL_ARGS[@]}" > /dev/null 2>&1; then
        echo "    ✅ 用户程序集合并成功"
        TEMP_DLL_SIZE=$(stat -c%s "$TEMP_DLL")
        echo "    临时DLL大小: $TEMP_DLL_SIZE 字节"
    else
        echo "    ⚠️ 用户程序集合并失败，使用核心DLL作为基础"
        cp "$CORE_DLL" "$TEMP_DLL"
    fi
else
    echo "    没有用户DLL需要合并"
    cp "$CORE_DLL" "$TEMP_DLL"
fi

# 步骤4.2: 合并第三方程序集 (内部化)
echo "  步骤4.2: 合并第三方程序集..."
if [ "$THIRD_PARTY_COUNT" -gt 0 ]; then
    THIRD_PARTY_DLL_ARGS=("/internalize" "$TEMP_DLL")
    for DLL in "$BUILD_TEMP_DIR/third-party-dlls"/*.dll; do
        THIRD_PARTY_DLL_ARGS+=("$DLL")
        echo "    添加(内部化): $(basename "$DLL")"
    done

    # 使用Windows路径格式
    WIN_FINAL_DLL=$(cygpath -w "$FINAL_DLL")
    WIN_THIRD_PARTY_ARGS=()
    for DLL in "${THIRD_PARTY_DLL_ARGS[@]}"; do
        WIN_THIRD_PARTY_ARGS+=("$(cygpath -w "$DLL")")
    done

    if "$ILREPACK_CMD" /out:"$WIN_FINAL_DLL" "${WIN_THIRD_PARTY_ARGS[@]}" > /dev/null 2>&1; then
        echo "    ✅ 第三方程序集合并成功"
    else
        echo "    ⚠️ 第三方程序集合并失败，尝试不使用内部化"

        # 尝试不使用internalize
        SAFE_THIRD_PARTY_ARGS=("$TEMP_DLL")
        for DLL in "$BUILD_TEMP_DIR/third-party-dlls"/*.dll; do
            SAFE_THIRD_PARTY_ARGS+=("$DLL")
        done

        WIN_SAFE_ARGS=()
        for DLL in "${SAFE_THIRD_PARTY_ARGS[@]}"; do
            WIN_SAFE_ARGS+=("$(cygpath -w "$DLL")")
        done

        if "$ILREPACK_CMD" /out:"$WIN_FINAL_DLL" "${WIN_SAFE_ARGS[@]}" > /dev/null 2>&1; then
            echo "    ✅ 安全合并成功"
        else
            echo "    ❌ 合并失败，使用临时文件作为最终结果"
            cp "$TEMP_DLL" "$FINAL_DLL"
        fi
    fi
else
    echo "    没有第三方DLL需要合并"
    cp "$TEMP_DLL" "$FINAL_DLL"
fi

# 清理临时文件
if [ -f "$TEMP_DLL" ]; then
    rm "$TEMP_DLL"
fi

# 验证合并结果
echo "🔄 步骤 5: 验证合并结果..."

if [ -f "$FINAL_DLL" ]; then
    ORIGINAL_SIZE=$(stat -c%s "$CORE_DLL")
    FINAL_SIZE=$(stat -c%s "$FINAL_DLL")

    echo "  原始DLL: $ORIGINAL_SIZE 字节"
    echo "  合并DLL: $FINAL_SIZE 字节"
    echo "  增长倍数: $(echo "scale=1; $FINAL_SIZE / $ORIGINAL_SIZE" | bc)x"

    echo "  ✅ 程序集合并成功完成"

    # 显示文件类型验证
    if command -v file > /dev/null 2>&1; then
        echo "  文件类型: $(file "$FINAL_DLL")"
    fi
else
    echo "  ❌ 合并失败: 最终文件不存在"
    exit 1
fi

# 清理临时目录
rm -rf "$BUILD_TEMP_DIR"
rm -rf "$COLLECTED_DLLS_PATH"

echo "🎉 自动化程序集合并构建流程完成!"
echo "合并后的程序集: $FINAL_DLL"