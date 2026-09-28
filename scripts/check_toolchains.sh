#!/usr/bin/env bash
# ==============================================================================
# check_toolchains.sh
# 作用：全面检测 uv, pnpm, node, dotnet 与 python3 运行环境
# 原则：白盒透明，清晰列出每个工具的物理绝对路径与版本号
# ==============================================================================

set -euo pipefail

echo "=========================================================="
echo "🛠️ [开发工具链自检] 游戏定制环境就绪状态审计"
echo "=========================================================="
echo ""

check_tool() {
    local name="$1"
    local cmd="$2"
    local ver_arg="$3"
    local desc="$4"

    echo "▶ 检查 $name ($desc):"
    if command -v "$cmd" >/dev/null 2>&1; then
        local p
        p=$(command -v "$cmd")
        local v
        v=$("$cmd" "$ver_arg" 2>&1 | head -n 1)
        echo "   状态: ✅ 已安装"
        echo "   版本: $v"
        echo "   路径: $p"
    else
        echo "   状态: ❌ 未在 PATH 中找到命令: $cmd"
    fi
    echo ""
}

check_tool "uv" "uv" "--version" "Python 新一代虚拟环境与包管理"
check_tool "pnpm" "pnpm" "--version" "Node.js 硬链接高性能包管理"
check_tool "node" "node" "--version" "Node.js 运行时"
check_tool "dotnet" "dotnet" "--version" "C# / .NET 10 原生 SDK 编译器"
check_tool "python3" "python3" "--version" "系统 Python 解释器"

echo "=========================================================="
echo "💡 [工具链架构共识]"
echo "   • uv    -> 掌管 Python 爬虫、资源解析与 Omni Deck 启动服务"
echo "   • pnpm  -> 掌管 Web 游戏界面、NW.js 包装与前端扩展"
echo "   • C#    -> 掌管 Unity / Mono 引擎游戏 Mod 研发 (BepInEx / Harmony)"
echo "=========================================================="
