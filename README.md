# SKYBREAK · 裂空：最后航线

原创日式科幻 3D 飞行射击游戏。三位成年驾驶员、三架特色战机、六种武装模式、机甲超载与三章机械首领战。

**当前版本：1.4.0 · macOS 离线单人。**

## 下载试玩

### [下载 Mac 试玩包（约 176 MB）](https://github.com/geduolin1991/skybreak-last-flight/releases/latest/download/SKYBREAK-macOS.zip)

[版本下载页](https://github.com/geduolin1991/skybreak-last-flight/releases/latest) · [Unity 源码压缩包](https://github.com/geduolin1991/skybreak-last-flight/releases/latest/download/SKYBREAK-Unity-Source.zip)

1. 下载并解压 `SKYBREAK-macOS.zip`。
2. 打开解压目录中的 `Build/SKYBREAK.app`，或双击 `开始游戏.command`。
3. 在机库选择战机，按 Enter 开始行动；阅读简报后再次按 Enter 出击。

试玩不需要安装 Unity 或 Blender。当前提供的是 **Mac 下载版**，尚未发布浏览器在线游玩版或 Windows 版。GitHub 的绿色 Code → Download ZIP 是源码，不是已编译的游戏。

## 1.4 的战斗与画面

![裂空炸弹的等离子冲击波](Docs/Media/nova-wavefront.png)

- **裂空炸弹**：聚能、由近至远扩散的等离子波前、径向折射、连续爆破和分层音效。
- **可拆解的 Boss**：独立炮台和护盾节点，破盾后 8 秒核心易伤；三台 Boss 各有四套攻击和三阶段变化。
- **真正运动的机械结构**：暴风眼的双涡轮旋转，炽天使的六翼随蓄力与核心暴露开合。
- **不同的操作体验**：苍隼双联速射与追踪弹群，蝠鲼破片覆盖与重型爆破，银针两种贯穿轨道炮。
- **三章航线**：海岸撤离、风暴都市、轨道天环；支线救援、局内强化、驾驶员成长和无尽航线。
- **原创资产**：Blender 制作机体与环境，原创配乐和合成音效，三位驾驶员保留自然动态立绘。

![炽天使核心暴露后的实战画面](Docs/Media/seraph-battle.png)

## 操作

| 按键 | 功能 |
|---|---|
| WASD / 方向键 | 移动 |
| Shift | 减速、精准飞行与显示判定点 |
| Q | 驾驶员专属技能 |
| E | 能量满后展开机甲超载 |
| 空格 | 裂空炸弹 |
| R | 切换当前战机的两种武装模式 |
| Esc | 暂停 |
| Enter | 开始行动 / 确认简报 |
| F11 | 全屏 / 窗口 |

默认自动射击。初次游玩建议新兵难度。完整玩法与设置见[试玩手册](README_开始试玩.md)。

## 使用 Unity 继续制作

本仓库根目录就是独立的 Unity 项目。用 **Unity 6000.6.0f1** 打开，加载 `Assets/Scenes/Skybreak.unity` 后运行。游戏在运行时建立关卡与界面。菜单 **Skybreak** 提供工程准备、macOS 构建和资源/规则检查。

- `Assets`：游戏逻辑、模型、材质、着色器、音频与驾驶员资源。
- `Packages` / `ProjectSettings`：固定依赖和 Unity 项目设置。
- `Tools`：可编辑 Blender 源文件和资产生成工具；最新生产场景为 `ProductionModels1.4.blend`。
- `Docs`：游戏设计、美术、更新说明、验证记录和第三方许可。

构建缓存、试玩应用、旧版本备份及发行压缩包不进入 Git 历史。已编译游戏和完整源码存档在本仓库 Releases 中提供。

## 验证与许可说明

1.4 原生构建成功，104 项机制检查通过；三架战机分别以新兵难度、零研发的自动控制器完成三章并达成全部救援。该结果不等于所有难度和设备的长期真人平衡验收。详情见[验证记录](Docs/验证记录.md)和[1.4 更新说明](Docs/爆炸与首领更新1.4.md)。

本仓库公开供查看与试玩；公开不代表所有内容采用同一种开源许可证。项目资产来源和第三方许可见[资产与许可](Docs/资产与许可.md)，嵌入包另保留其许可证。
