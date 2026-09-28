#!/usr/bin/env bash
# ==============================================================================
# menu_register_portable.sh
# 作用：交互式引导用户为 ~/Applications 下的便携软件创建标准的 .desktop 启动菜单项
# 原理：遵循 XDG Desktop Entry 规范，将元数据写入 ~/.local/share/applications/
# ==============================================================================

set -euo pipefail

echo "=========================================================="
echo "➕ [注册便携应用] 为 ~/Applications 软件创建 KDE 菜单入口"
echo "=========================================================="
echo ""

apps_dir="$HOME/Applications"
user_desktop_dir="$HOME/.local/share/applications"
mkdir -p "$user_desktop_dir"

# 1. 扫描当前已有的便携软件
echo "当前检测到 ~/Applications 下的软件目录："
find "$apps_dir" -mindepth 1 -maxdepth 1 -type d -exec basename {} \;
echo ""

# 2. 交互收集信息
read -rp "1. 请输入应用唯一英文标识 (例如 myapp): " app_id
if [ -z "$app_id" ]; then echo "❌ 标识不能为空！"; exit 1; fi

read -rp "2. 请输入在菜单中显示的名称 (例如 我的便携工具): " app_name
if [ -z "$app_name" ]; then app_name="$app_id"; fi

read -rp "3. 请输入可执行程序或脚本的绝对路径: " app_exec
if [ ! -f "$app_exec" ]; then
    echo "⚠️ 提示：路径 $app_exec 当前不存在，请确认无误。"
fi

read -rp "4. 请输入图标路径 (留空则使用通用图标): " app_icon
if [ -z "$app_icon" ]; then app_icon="application-x-executable"; fi

echo "请选择主要分类："
echo "  [1] 游戏 (Game;Emulator;)"
echo "  [2] 开发 (Development;IDE;)"
echo "  [3] 网络与通信 (Network;InstantMessaging;WebBrowser;)"
echo "  [4] 影音多媒体 (AudioVideo;Audio;Player;)"
echo "  [5] 实用系统工具 (Utility;System;)"
read -rp "请选择编号 (默认 5): " cat_choice

case "$cat_choice" in
    1) app_cat="Game;Emulator;" ;;
    2) app_cat="Development;IDE;" ;;
    3) app_cat="Network;InstantMessaging;" ;;
    4) app_cat="AudioVideo;Audio;Player;" ;;
    *) app_cat="Utility;System;" ;;
esac

target_file="$user_desktop_dir/${app_id}.desktop"

cat << EOF > "$target_file"
[Desktop Entry]
Version=1.0
Type=Application
Name=${app_name}
Exec=${app_exec}
Icon=${app_icon}
Terminal=false
Categories=${app_cat}
StartupNotify=true
EOF

chmod +x "$target_file"

echo ""
echo "=========================================================="
echo "✅ 菜单项创建成功！"
echo "文件生成于: $target_file"
echo "现在打开 KDE 开始菜单，即可在对应分类下看到 [${app_name}]。"
echo "=========================================================="
