#!/bin/bash

# ModularGodot.Core - WSL/Windows 跨平台开发工作流优化脚本
# 解决 WSL 环境下使用 AIAgent 开发 + Windows Rider 调试的平台冲突问题

set -e

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}🚀 启动 ModularGodot.Core 跨平台开发工作流优化${NC}"

# 检测当前环境
if grep -qi microsoft /proc/version && [ -n "$WSL_DISTRO_NAME" ]; then
    echo -e "${GREEN}✅ 检测到 WSL 环境: $WSL_DISTRO_NAME${NC}"
    IS_WSL=true
else
    echo -e "${YELLOW}⚠️  非 WSL 环境，按普通 Linux 处理${NC}"
    IS_WSL=false
fi

# 清理旧的构建缓存（仅清理obj/bin，不清理NuGet包）
echo -e "${YELLOW}🧹 清理旧的构建缓存...${NC}"
find ./src -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true
find ./src -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true

# 统一平台配置
echo -e "${YELLOW}🔧 设置统一平台配置 (AnyCPU)...${NC}"
export MSBUILDTERMNODEPATH=""
export Platform="AnyCPU"
export PlatformTarget="AnyCPU"

# 智能 NuGet 还原
echo -e "${YELLOW}📦 执行智能 NuGet 还原...${NC}"
if [ "$IS_WSL" = true ]; then
    echo -e "${GREEN}  使用 WSL 优化策略${NC}"
    # 对于 WSL，限制并行连接，避免网络超时
    dotnet restore src/ModularGodot.Core.sln \
        --disable-parallel \
        --ignore-failed-sources \
        --no-cache \
        -p:Platform="Any CPU" \
        -p:TreatWarningsAsErrors=false
else
    dotnet restore src/ModularGodot.Core.sln \
        -p:Platform="Any CPU"
fi

echo -e "${GREEN}✅ NuGet 还原完成${NC}"

# 执行构建
echo -e "${YELLOW}🏗️  执行构建...${NC}"
dotnet build src/ModularGodot.Core.sln \
    -c Debug \
    -p:Platform="Any CPU" \
    --no-restore

echo -e "${GREEN}✅ 构建完成${NC}"

# 运行测试（可选）
if [[ "$1" == "--test" || "$1" == "-t" ]]; then
    echo -e "${YELLOW}🧪 运行测试...${NC}"
    dotnet test src/ModularGodot.Core.XUnitTests/ModularGodot.Core.XUnitTests.csproj \
        --no-build \
        --logger "console;verbosity=normal"
fi

echo -e "${GREEN}🎉 工作流优化完成！现在可以在 Windows Rider 中直接打开项目而无需 clean${NC}"
echo -e "${YELLOW}💡 提示: 此脚本已自动处理平台冲突，Rider 应该可以正常工作了${NC}"