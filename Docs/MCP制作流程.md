# Unity / Blender 制作与 MCP 连接

当前游戏实际使用 Blender → FBX → Unity 的工作流。Blender 的 Python API（bpy）生成网格、倒角、材质、机械分组，并保留 Tools 中的 .blend 源文件；导出 Assets/Art/Models/*.fbx。Unity 的 SkyBuild 编辑器脚本导入 FBX、转换材质、建立预制体与场景，再编译独立游戏。运行时 C# 和着色器实现玩法、关卡、特效、音乐、机甲展开与驾驶员立绘动画。

21 种模型资产：Kestrel、Manta、Needle、Drone、Interceptor、Gunship、Leviathan、Tempest、Seraph、AstraFrame、CrimsonFrame、OracleFrame、EvacCarrier、StormRelay、OrbitalGate、SiegeTurret、ShieldEmitter、HarborPort、StormCity、OrbitHabitat、BasaltIsland；另保留环境资产制作工作区。三位驾驶员目前是二维动态立绘，不是 Blender 人物模型；身体摇摆与胸部随动在 Unity 中进行局部变形。

## 2026-09-06 安装状态

- Blender 5.2.1 LTS：安装并启用 Blender Lab MCP 1.0.0，注册为 Codex 的 blender_lab。已通过真正的 MCP 协议读取 AstraFrame.blend，返回 49 个对象、45 个网格和 Arm_L / Arm_R / Wing_L / Wing_R 节点；已成功通过 MCP 调用 Blender 截图 API，返回本地编辑画面。共枚举出 26 个工具。
- Unity 6000.6.0f1：嵌入 MCP for Unity 10.2.0，注册为 unity_skybreak，默认目标 SkybreakUnity。用户明确回复“我同意”后，已在编辑器窗口接受条款。真实 MCP 读取编辑器状态、控制台并执行构建和资源检查成功；实例 SkybreakUnity@027feaa0，端口 6400。
- Unity MCP 使用本机连接，遥测关闭。Blender MCP 的桥接使用 localhost:9876。本次没有开通付费模型生成服务或添加 API 密钥。
- 1.1.1 应用发行档和源码档已独立备份。1.2.0 通过实际 Unity MCP 构建，发布状态以 README 与《验证记录》为准。

## 后续使用

优先用 MCP 读取当前场景、对象、材质和报错，再修改；批量资产制作与可复现构建继续用 bpy / Unity C# 编辑器 API。软件接口本来就可直接调用，MCP 增加实时上下文和交互，不会自动提高资产质量。

本机服务源码、隔离 Python 环境、安装版本、备份和验证输出位于工作区 .tools/engine-integrations。Codex 注册配置位于 ~/.codex/config.toml；没有重启 Codex 或其他任务。当前任务尚未刷新工具目录时，可用该目录的 mcp_client.py 通过标准 MCP 客户端通信。

Blender 的原有在线设置为关闭。启动制作连接时使用 Blender 的 --online-mode 参数，仅为本次进程启用插件要求的联网许可；没有修改全局在线偏好。插件服务绑定本机。普通离线启动时插件会提示不能启动桥接，需重新以该参数启动。

Unity 的 Skybreak > Development 菜单提供启动/停止本地 MCP。连接已验证；后续修改先读取编辑器状态，编译完成后检查控制台，再运行对应机制与画面验证。

## 已知兼容情况

Blender 实验版插件的大图内联传输在本机出现 JSON 截断。可通过 execute_blender_code 调用 bpy.ops.screen.screenshot(filepath=...) 保存本地图片，再读取该图片，此方式已实测成功。render_viewport_to_path 实际进行场景相机渲染，纯模型源文件没有相机时不能直接用它截图。不得把这两种接口混为一谈。

## 来源与固定版本

- Blender 官方实验室：https://www.blender.org/lab/mcp-server/
- Blender 源码：https://projects.blender.org/lab/blender_mcp ，提交 4309a39646e644261624bfcd2bca669b343b7621。
- MCP for Unity（第三方 MIT 开源项目）：https://github.com/CoplayDev/unity-mcp ，提交 30d22075093d1d35dfb0091c1c7550e9ad948577，版本 10.2.0。
- Codex MCP 配置：https://learn.chatgpt.com/docs/extend/mcp?surface=cli

Python 已安装依赖版本见 .tools/engine-integrations/requirements-installed.txt。Unity 原清单与 Blender 原偏好设置已保存到该目录的 backups。

1.4 模型细化、武装节点与环境套件通过 Blender MCP 制作。新的动态转轴在 Unity 材质合并中保留，Boss 的涡轮与六翼在原生运行时已验证运动。最新 .blend 生产总场景为 `Tools/ProductionModels1.4.blend`。
