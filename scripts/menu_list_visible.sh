#!/usr/bin/env bash
# ==============================================================================
# menu_list_visible.sh
# 作用：扫描并输出当前 KDE 启动菜单中真实可见的应用程序列表
# 原理：解析系统 /usr/share/applications 与用户 ~/.local/share/applications
#       根据 XDG 优先级层叠，过滤掉 NoDisplay=true 的项
# ==============================================================================

set -euo pipefail

python3 -c "
import glob, os

user_dir = os.path.expanduser('~/.local/share/applications')
sys_dir = '/usr/share/applications'

all_desktops = {}
if os.path.exists(sys_dir):
    for f in glob.glob(os.path.join(sys_dir, '*.desktop')):
        all_desktops[os.path.basename(f)] = f
if os.path.exists(user_dir):
    for f in glob.glob(os.path.join(user_dir, '*.desktop')):
        all_desktops[os.path.basename(f)] = f

visible = []
for base, path in all_desktops.items():
    try:
        with open(path, 'r', encoding='utf-8', errors='ignore') as fp:
            content = fp.read()
            if 'NoDisplay=true' in content:
                continue
            name = ''
            cat = ''
            for line in content.splitlines():
                if line.startswith('Name=') and not name:
                    name = line[5:].strip()
                elif line.startswith('Categories=') and not cat:
                    cat = line[11:].strip()
            if name:
                visible.append((name, base, cat))
    except:
        pass

visible.sort(key=lambda x: x[0].lower())

print('=' * 85)
print(f'📋 [KDE 启动菜单可见应用清单] 当前共有 {len(visible)} 个项目呈现在桌面菜单中：')
print('=' * 85)
print(f'{\"应用显示名称\":<26} | {\".desktop 描述文件\":<35} | 分类')
print('-' * 85)
for name, base, cat in visible:
    cat_short = cat[:25] + '...' if len(cat) > 25 else cat
    print(f'{name:<26} | {base:<35} | {cat_short}')
print('=' * 85)
"
