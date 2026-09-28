#!/usr/bin/env bash
# ==============================================================================
# create_csharp_mod.sh
# 作用：为指定游戏生成全新的 C# Mod 工程脚手架，并自动追加至 DeckToolBox.slnx
# 用法：./create_csharp_mod.sh <游戏名称> <Mod工程名称>
# 示例：./create_csharp_mod.sh DysonSphere FasterCraftMod
# ==============================================================================

set -euo pipefail

if [ $# -lt 2 ]; then
    echo "❌ 参数不足！"
    echo "用法: $0 <目标游戏名称> <Mod工程英文名称>"
    echo "示例: $0 DysonSphere FasterCraftMod"
    exit 1
fi

game_name="$1"
mod_name="$2"

root_dir="/home/deck/Games/DeckToolBox"
slnx_file="$root_dir/DeckToolBox.slnx"
mods_base_dir="$root_dir/mods"
target_dir="$mods_base_dir/$mod_name"

echo "=========================================================="
echo "🚀 [生成 C# Mod 工程] 正在为 [${game_name}] 搭建工程..."
echo "工程名称: $mod_name"
echo "目标路径: $target_dir"
echo "=========================================================="

if [ -d "$target_dir" ]; then
    echo "❌ 目录已存在: $target_dir"
    exit 1
fi

mkdir -p "$target_dir"

# 1. 生成 .csproj
echo "1. 生成 .NET Standard 2.1 项目文件..."
cat << EOF > "$target_dir/${mod_name}.csproj"
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <Nullable>enable</Nullable>
    <AssemblyName>${mod_name}</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="../../src/DeckToolBox.ModCommon/DeckToolBox.ModCommon.csproj" />
  </ItemGroup>

</Project>
EOF

# 2. 生成 Plugin.cs 骨架
echo "2. 生成 C# Mod 插件入口类..."
cat << EOF > "$target_dir/${mod_name}Plugin.cs"
using System;
using DeckToolBox.ModCommon;

namespace ${mod_name}
{
    /// <summary>
    /// 为 [${game_name}] 定制的 C# Mod 插件入口
    /// </summary>
    public class ${mod_name}Plugin
    {
        public const string ModGuid = "com.decktoolbox.${mod_name,,}";
        public const string ModName = "${mod_name}";
        public const string ModVersion = "1.0.0";

        public void Awake()
        {
            ModLogger.Prefix = $"[{ModName}]";
            ModLogger.Info("[${game_name}] Mod 已成功装载进入进程内存空间！");
        }
    }
}
EOF

# 3. 挂载到主解决方案
echo "3. 挂载到统一解决方案 DeckToolBox.slnx..."
if [ -f "$slnx_file" ]; then
    dotnet sln "$slnx_file" add "$target_dir/${mod_name}.csproj"
    echo "   ✅ 成功挂载至解决方案。"
fi

echo ""
echo "=========================================================="
echo "🎉 工程生成完成！"
echo "开发步骤："
echo "  1. 使用文本编辑器或 IDE (如 Zed) 打开: $target_dir"
echo "  2. 编写你的 Harmony 补丁或逻辑代码"
echo "  3. 运行编译命令测试: dotnet build $slnx_file"
echo "=========================================================="
