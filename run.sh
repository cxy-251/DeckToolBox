#!/usr/bin/env bash
# ==============================================================================
# DeckToolBox - 本地启动脚本（参数 --check / --commands 用于内容检查）
# 说明：本项目仅作为本地工程运行，不创建任何系统菜单快捷方式，不污染任何外部环境。
# ==============================================================================
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

export PATH="$HOME/Applications/dotnet:$HOME/.local/bin:$PATH"

echo "[*] 正在启动 DeckToolBox ..."
dotnet run --project "$SCRIPT_DIR/src/DeckToolBox.App/DeckToolBox.App.csproj" -- "$@"
