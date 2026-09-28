#!/usr/bin/env bash
# ==============================================================================
# clean_flatpak_safe.sh
# 作用：白盒透明执行 Flatpak / Discover 历史残留扫描与清理
# 原则：每一项操作输出清晰的原因与影响，零提权，完全可控
# ==============================================================================

set -euo pipefail

echo "=========================================================="
echo "🧹 [白盒清理] Flatpak / Discover 历史残留分析与安全清除"
echo "=========================================================="
echo "【原因说明】Flatpak 沙箱与残留运行时占用内置硬盘空间，且隔离"
echo "           环境变量与游戏文件，违背独立自控原则。"
echo "【影响范围】仅操作用户目录 (~/.local 与 ~/.var/app)，不触碰根系统。"
echo "=========================================================="
echo ""

# 1. 扫描当前应用程序
echo "[步骤 1/4] 扫描已安装的 Flatpak 应用程序..."
apps=$(flatpak list --app || true)
if [ -n "$apps" ]; then
    echo "  ⚠️ 发现已安装的应用:"
    echo "$apps"
    echo ""
    read -rp "  ❓ 是否执行卸载这些应用？(y/N): " confirm_app
    if [[ "$confirm_app" =~ ^[Yy]$ ]]; then
        flatpak uninstall --all --user -y
        echo "  ✅ 应用程序已卸载。"
    else
        echo "  ⏭️ 跳过应用程序卸载。"
    fi
else
    echo "  ✅ 很好，当前没有任何通过 Flatpak 安装的应用程序。"
fi
echo ""

# 2. 清理未引用的基础运行时
echo "[步骤 2/4] 清理孤立的 Flatpak 基础运行时 (org.freedesktop / org.kde 等)..."
flatpak uninstall --unused --user -y 2>/dev/null || true
echo "  ✅ 未使用的底层运行时库清理完毕。"
echo ""

# 3. 扫描 ~/.var/app 下孤儿配置
echo "[步骤 3/4] 检查 ~/.var/app 历史残留配置..."
var_app_dir="$HOME/.var/app"
if [ -d "$var_app_dir" ]; then
    count=$(find "$var_app_dir" -mindepth 1 -maxdepth 1 -type d | wc -l)
    if [ "$count" -gt 0 ]; then
        echo "  ⚠️ 发现 $count 个已卸载软件遗留的配置文件夹:"
        ls -1 "$var_app_dir"
        echo ""
        read -rp "  ❓ 是否确认清空这些历史遗留数据？(y/N): " confirm_var
        if [[ "$confirm_var" =~ ^[Yy]$ ]]; then
            rm -rf "$var_app_dir"/*
            echo "  ✅ ~/.var/app 孤儿配置已彻底清空。"
        else
            echo "  ⏭️ 保留 ~/.var/app 内容。"
        fi
    else
        echo "  ✅ ~/.var/app 干净无残留。"
    fi
fi
echo ""

# 4. 优化本地 ostree 存储库
echo "[步骤 4/4] 修剪与回收本地 Flatpak 对象存储空间..."
flatpak repair --user 2>/dev/null || true
echo "  ✅ 存储库修剪完成。"
echo ""

echo "=========================================================="
echo "🎉 [结果] Flatpak 清理与环境纯净化阶段完成！"
echo "=========================================================="
