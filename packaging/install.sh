#!/usr/bin/env bash
# DeckToolBox 安装脚本：复制到 ~/Applications/DeckToolBox 并加入应用菜单，不需要 root。
#   ./install.sh              安装或更新
#   ./install.sh --uninstall  卸载（删除程序、菜单项与图标）
set -e

SRC="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEST="$HOME/Applications/DeckToolBox"
DESKTOP="${XDG_DATA_HOME:-$HOME/.local/share}/applications/decktoolbox.desktop"
ICON="${XDG_DATA_HOME:-$HOME/.local/share}/icons/hicolor/256x256/apps/decktoolbox.png"

if [ "$1" = "--uninstall" ]; then
    rm -rf "$DEST"
    rm -f "$DESKTOP" "$ICON"
    echo "已卸载 DeckToolBox。"
    exit 0
fi

mkdir -p "$DEST" "$(dirname "$DESKTOP")" "$(dirname "$ICON")"
install -m 755 "$SRC/DeckToolBox" "$DEST/DeckToolBox"
install -m 644 "$SRC/decktoolbox.png" "$ICON"

cat > "$DESKTOP" <<DESKTOP_ENTRY
[Desktop Entry]
Type=Application
Name=DeckToolBox
Comment=Linux 使用手册：命令、原理与操作
Exec="$DEST/DeckToolBox"
Icon=decktoolbox
Terminal=false
Categories=Utility;Documentation;
DESKTOP_ENTRY

command -v update-desktop-database >/dev/null && update-desktop-database -q "$(dirname "$DESKTOP")" || true
echo "已安装到 $DEST，可以在应用菜单中找到 DeckToolBox。"
