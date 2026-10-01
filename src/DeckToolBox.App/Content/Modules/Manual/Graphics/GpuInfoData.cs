using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Graphics;

public static class GpuInfoData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-gpu-info",
        Title = "显卡与驱动信息",
        Keywords = "vulkaninfo · glxinfo · drm_info · wayland-info · Mesa · RADV",
        Level = ModuleLevel.Intermediate,
        Overview = "游戏报「不支持 Vulkan」、求助时被问「驱动版本多少」，就用这几个命令查看：vulkaninfo 查 Vulkan 驱动与支持的功能，glxinfo 查 OpenGL，drm_info 查内核显示驱动与连接器，wayland-info 查合成器支持的协议。",

        Why =
        [
            "1. Steam Deck 的图形栈：",
            "- 内核驱动：`amdgpu`，负责显存管理、调度与显示输出。",
            "- 用户态驱动：Mesa 提供。Vulkan 驱动为 **RADV**，OpenGL 驱动为 **radeonsi**。",
            "- Proton 把 DirectX 翻译为 Vulkan，所以 Windows 游戏最终都跑在 RADV 上。",
            "- APU 代号 Van Gogh（`vangogh`），GPU 在系统中名为 `AMD Custom GPU 0405`（LCD 版）或 `0932`（OLED 版）。",
            "2. vulkaninfo：`--summary` 输出简要信息（API 版本、驱动名与版本、设备类型）；不带参数会输出几千行完整能力列表，可以配合 grep 查询某个扩展是否支持。",
            "3. glxinfo（mesa-utils 提供）：`-B` 输出简要信息：渲染器、OpenGL 版本、是否硬件加速、显存。",
            "4. 注意：Vulkan 与 OpenGL 报告的 Mesa 版本可能不同。SteamOS 为游戏提供了较新的 RADV，版本号可能显示为开发版（如 `26.0.0-devel`），而 OpenGL 仍使用系统的稳定版。",
            "5. drm_info：内核 DRM 子系统的视角，列出驱动、每个连接器 (eDP/DP) 的状态与支持的显示模式、显示平面等。",
            "6. wayland-info：列出当前 Wayland 合成器（KWin）支持的协议及版本，排查「某个程序在 Wayland 下缺功能」时有用。",
        ],

        Consequences =
        [
            "• 全部是只读查询。",
            "• 这些命令需要在图形会话中运行（桌面模式终端）；通过 SSH 执行时，glxinfo 等会因找不到显示而失败。",
        ],

        Steps =
        [
            Step.Read(
                "Vulkan 驱动摘要",
                "vulkaninfo --summary 2>/dev/null | sed -n '/Devices:/,$p'",
                "查询 Vulkan ICD 加载的物理 GPU 拓扑；用 sed 提取 GPU0 的设备名称、Mesa RADV 驱动与 Vulkan API 版本。").Then("列出 `GPU0`：`apiVersion = 1.4.x`、`deviceName = AMD Custom GPU 0932 (RADV VANGOGH)`、`driverName = radv`、`driverInfo = Mesa ...`，以及一个 `llvmpipe` 软件渲染设备（如果存在）。"),
            Step.Read(
                "检查是否支持某个 Vulkan 扩展",
                "vulkaninfo 2>/dev/null | grep -m 3 -E 'VK_KHR_ray_tracing_pipeline|VK_EXT_mesh_shader'",
                "`-m 3` 最多显示 3 行匹配。").Then("列出支持的扩展名及其版本号；没有输出表示不支持。"),
            Step.Read(
                "OpenGL 渲染器信息",
                "glxinfo -B",
                "`-B` 简要模式。").Then("显示 `direct rendering: Yes`、`Device: AMD Custom GPU 0932 (radeonsi, vangogh, LLVM ...)`、`Version: 25.x`、`Accelerated: yes`、`Max core profile version: 4.6`。"),
            Step.Read(
                "内核显示驱动与连接器",
                "drm_info 2>/dev/null | grep -E 'Driver|Connector [0-9]|Status|Name:' | head -n 12",
                "完整输出很长，这里只取关键行。").Then("显示 `Driver: amdgpu (AMD GPU) version 3.64.0`，以及每个连接器的名称与 `Status: connected/disconnected`。"),
            Step.Read(
                "合成器支持的 Wayland 协议",
                "wayland-info 2>/dev/null | grep interface | head -n 20",
                "向 KWin 或 Gamescope 运行时套接字发送查询，枚举合成器暴露的核心协议与扩展协议接口列表。").Then("每行一个协议，如 `wl_compositor version: 6`、`wp_fractional_scale_manager_v1`（分数缩放）、`wp_viewporter` 等。"),
        ],

        Verify =
        [
            "1. `vulkaninfo --summary` 中 `driverName` 为 `radv`，说明游戏使用的是硬件 Vulkan 驱动。",
        ],
    };
}
