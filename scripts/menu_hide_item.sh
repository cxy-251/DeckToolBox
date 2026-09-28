#!/usr/bin/env bash
# ==============================================================================
# menu_hide_item.sh
# 作用：免提权从 KDE 启动菜单中隐藏指定的应用程序
# 原理：在 ~/.local/share/applications/ 中写入用户级覆盖规则 NoDisplay=true
# 用法：./menu_hide_item.sh <desktop文件名或应用名>
# ==============================================================================

set -euo pipefail

if [ $# -lt 1 ]; then
    echo "❌ 缺少参数！"
    echo "用法: $0 <desktop文件名>"
    echo "示例: $0 org.mozilla.firefox.desktop"
    exit 1
fi

target="$1"
if [[ "$target" != *.desktop ]]; then
    target="${target}.desktop"
fi

user_dir="$HOME/.local/share/applications"
sys_dir="/usr/share/applications"
mkdir -p "$user_dir"

user_file="$user_dir/$target"
sys_file="$sys_dir/$target"

if [ ! -f "$user_file" ] && [ ! -f "$sys_file" ]; then
    echo "❌ 找不到指定的 .desktop 文件: $target"
    echo "   建议先运行 ./menu_list_visible.sh 查看当前文件名。"
    exit 1
fi

echo "=========================================================="
echo "🙈 [隐藏菜单项] 正在配置用户覆盖规则..."
echo "目标文件: $target"
echo "=========================================================="

if [ -f "$user_file" ]; then
    # 若用户文件已存在，修改或追加 NoDisplay=true
    if grep -q "^NoDisplay=" "$user_file"; then
        sed -i 's/^NoDisplay=.*/NoDisplay=true/' "$user_file"
    else
        echo "NoDisplay=true" >> "$user_file"
    fi
else
    # 从系统级拷贝基础结构并追加 NoDisplay=true
    cp "$sys_file" "$user_file"
    echo "NoDisplay=true" >> "$user_file"
fi

echo "✅ 已成功配置 NoDisplay=true 规则！"
echo "   覆盖写入路径: $user_file"
echo "   生效说明: KDE Plasma 会立即采纳该覆盖项并在菜单中隐藏该软件。"
echo "   如需恢复，只需删除该文件或使用文本编辑器去掉 NoDisplay=true 即可。"
