# CMGM 框架架构说明

> 本文档是框架化改造的长期参考（「北极星」）。  
> 目标：将当前工程从「带 Demo 的原型项目」逐步改造为「可跨项目迁移的框架」。  
> 最后更新：2026-06-19 决策（§7.5）：`LoadSceneAsync` 下沉 Core、撤销 `CMGM.Scene` asmdef；Loading 复活为独立模块（L1~L5）。§7.1 主路线图整体重排讨论中。

---

## 1. 项目定位

| 维度 | 说明 |
|------|------|
| 游戏类型 | 单机独立，主线 20~50 小时；JRPG / SRPG / MUG 等 |
| 当前状态 | 约 30% 完成度的基础运行时 + 工具链骨架 |
| 远期目标 | 框架可插拔、游戏层可替换；未来网游扩展时不推倒重来 |

**一句话：** C# 管引擎 / UI / 资源，Lua 管剧情 / 关卡 / 事件；Excel 管数值配表。

---

## 2. 目录结构（当前）

```
Assets/_WorkSpace/
├── ARCHITECTURE.md          ← 本文档
├── Excels/                  策划 Excel 源文件
├── HotRes/                  可热更 / Addressables 资源
│   ├── Lua/                 Lua 脚本（.lua.txt）
│   ├── Scenes/              游戏场景
│   ├── UI/Panels/           UI Prefab
│   └── RhythmMap/           MUG 节拍映射数据
├── Resources/               不可热更的内置资源
│   ├── CmgmFrameSettings.asset
│   ├── UI/UICamera.prefab
│   └── BeforeGame/logos/    启动 Logo
├── Editor/                  ⚠ 待 2.8 迁入 `Framework/Editor/`（非 Core 内，见 §6.2）
└── Scripts/
    ├── Game/                游戏专属（2.1 ✅）
    │   ├── UI/Panels/
    │   ├── Archive/         GameRuntimeData（2.2 ✅）
    │   └── Config/          配表 Container（2.3 ✅）
    ├── Framework/           框架（2.1a ✅）
    │   ├── Core/            CMGM.Core asmdef、Consts.Paths
    │   └── Modules/
    │       ├── UI/          原 GameUI
    │       ├── Data/        原 GameData（CMGM.Data ✅）
    │       │   ├── Archive/ ArchiveManager、I_Saveable
    │       │   ├── Config/  ConfigTableManager
    │       │   └── Editor/  ExcelTool、ArchiveEditor（CMGM.Data.Editor）
    │       ├── Scene/       原 GameLevel / Level
    │       ├── Lua/         原 LuaCore
    │       ├── Audio/       原 AudioSystem
    │       ├── Input/       原 GameInput
    │       ├── Optional/    原 OptionalSystem
    │       └── Utils/
    └── GameBattle/          占位（未纳入 Framework，按需处理）
```

> **2.3 ✅**：`RoleInfoContainer` → `Scripts/Game/Config/`；**B** `Consts.Paths.Framework` / `.Game` 分层。

### 路径常量入口

所有路径由 **`Consts.Paths`**（`Framework/Core/Consts.Paths.cs`）统一定义：

- **`WorkSpace`** — 从 `CmgmFrameSettings.WORK_SPACE_ROOT` 读取（默认 `Assets/_WorkSpace`）
- **共享**：`HotRes`、`ARCHIVE_PATH`、`ConfigData` 等
- **`Paths.Framework.*`** — 框架目录（`Core`、`Modules`）
- **`Paths.Game.*`** — 游戏层（`UI_Panels`、`Archive`、`Config`）
- **`Paths.Framework.DataModule.*`** — 框架 Data 模块（`Archive`、`Config`）

换项目时：改 Settings 里的根路径即可，不必改代码里的字符串。

---

## 2b. 路径与资源引用优化线（并入总路线图）

除主阶段外，路径/地址相关改进按下列步骤穿插推进：

| 代号 | 内容 | 计划阶段 | 状态 |
|------|------|----------|------|
| **A** | 合并 `Consts` 为单文件，去掉 partial | 1.2b | ✅ |
| **C** | `WorkSpace` 根路径迁入 `CmgmFrameSettings` | 1.2b | ✅ |
| **B** | 拆 `Consts.Paths.Framework` 与 `Consts.Paths.Game` | 2.3 | ✅ |
| **D** | Addressables 加载键独立为 `AssetAddresses` 或 Label 分组 | 2.4（ScenesManager 配置化同期） | 待做 |
| **F** | 关键 Prefab/SO 改用 `AssetReference`，减少字符串路径 | 2.1 迁 Panel 后 / 7.x | 按需 |
| **E** | 扫描 `HotRes/` 自动生成路径常量（代码生成） | 8.4 前后（Lua 懒加载、内容量上来后） | 远期 |

---

## 3. 模块清单

### 3.1 核心模块（现 `GameCore/` → 2.1a 后 `Framework/Core/`）

| 组件 | 文件 | 职责 | 成熟度 |
|------|------|------|--------|
| 单例基类 | `Singleton/` | 懒汉 / Mono / AutoMono 三种单例 | ★★★ |
| 资源管理 | `ResourceManagement/AddressablesResMgr` | AB 加载、引用计数、防重复、预加载 | ★★★★ |
| 对象池 | `ResourceManagement/ObjectPool/` | 基础对象池 | ★★ |
| 帧设置 | `CmgmFrameSettings` | LOG、Lua 热重载、**工作区根路径**、根脚本路径 | ★★★ |
| 路径常量 | `Consts.Paths` | 由 Settings 派生的目录与管线路径 | ★★★ |
| 日志 | `CoreUtils/CmgmLog` | 分级日志；Error 不受 LOG 开关影响 | ★★★ |
| Mono 调度 | `MonoManager/` | Update 委托挂载 | ★★ |

### 3.2 UI（现 `GameUI/` → 2.1a 后 `Framework/Modules/UI/`）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `UIManager` | 分层 Canvas、异步加载 Panel、Hide 中途取消容错 | ★★★★ |
| `BasePanel` | Panel 基类 | ★★★ |
| `Panels/MainPanel` 等 | 已迁至 `Scripts/Game/UI/Panels/`（2.1 ✅） | — |
| Editor 工具 | Panel 模板创建、快速搜索 | ★★★ |

### 3.3 数据（现 `GameData/` → 2.1a 后 `Framework/Modules/Data/`）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `ConfigTableManager` | 读取 `.cmgm` 二进制配表（反射 + 解密） | ★★★★ |
| `ExcelTool`（Editor） | Excel → Container.cs + 二进制 | ★★★★ |
| `RoleInfoContainer` 等 | 游戏配表（`Scripts/Game/Config/`，2.3 ✅） | — |
| `ArchiveManager` | 存档元数据 + 运行时数据读写 | ★★★ |
| `GameRuntimeData` | 游戏存档结构（`Scripts/Game/Archive/`，2.2 ✅） | — |
| `CipherTool` | 配表 / 存档加解密 | ★★★ |

### 3.4 Lua（现 `LuaCore/` → 2.1a 后 `Framework/Modules/Lua/`）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `LuaManager` | LuaEnv 生命周期、Loader 链、脚本执行 | ★★★ |
| `LuaBridge` | C# ↔ Lua 桥接（Talk / Wait / DebugLog） | ★★ |

**Lua 加载策略（阶段 0.3 整理后）：**

| 模式 | 预加载 | require / ExecuteLua |
|------|--------|----------------------|
| 热重载（Editor） | 跳过 `LoadLuaMapper` | Loader 1 / `GetLuaContent` 直读磁盘 |
| 正式包体 | `LoadLuaMapper` → `luaMapper` | Loader 3 查内存字典 |

### 3.5 启动编排（`CmgmFrameBoot`，过渡位置见 §6.5）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `CmgmFrameBoot` | InitScene 上的 **组合根**：Logo + 各框架 Manager Init + 调 `ScenesManager.GoToMainScene` | ★★ |

> **不归 Core、也不语义上属于 Scene**——它横切 UI / Data / Lua / Audio / Scene 等 Modules；**不能**放进 `CMGM.Core`（否则 Core 反向依赖 Modules，见 §6.5）。  
> **当前（过渡）：** `Framework/CmgmFrameBoot.cs`——无 namespace、无 asmdef；阶段 **3.x** 再定 `CMGM.Bootstrap` 分层。

### 3.6 场景 / 流程（`Framework/Modules/Scene/`，`CMGM.Scene`）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `ScenesManager` | 场景切换、回主界面；**进游戏 Loading** 进度与 `LoadSceneAsync` 协作（**2.5c**，不单独建 Modules） | ★★ |
| `LoadingPanel` / 加载编排（规划 **2.5c**） | 进度条 UI、分段任务、对接 Addressables 进度；包装 `GameBootstrap.EnterGameplayAsync` | ☆ |

> **边界：** Scene 模块 = **场景加载与轻量流程**（非「关卡内容 / 关卡管理器」）；角色控制器、地图触发器等见 §6.4 `CmgmGameKits`、`Scripts/Game/`。  
> **命名：** 2026-06 自 `Level` / `CMGM.Level` 重命名，避免与关卡设计语义混淆。

### 3.7 可选模块

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `WwiseAudioManager` | Wwise 音频 + 节拍事件 | ★★★ |
| `MusicSyncTool` | MUG 节拍同步 | ★★★ |
| `InputManager` | Input System 封装 | ★★ |
| `OptionalSystem/EventSystem` | **未实现**（仅 .meta） | ☆ |
| `OptionalSystem/CommandSystem` | **未实现**（仅 .meta） | ☆ |

---

## 4. 启动流程（当前）

```
InitScene（CmgmFrameBoot.Awake）
    │
    ├─ 显示 Logo（Video / Texture）
    │
    └─ InitGame()  ── UniTask 并行 ──┐
                                      │
        ┌─────────────────────────────┘
        │
        ├─ AddressablesResMgr.PreloadAssetsAsync("MainScene")
        ├─ UIManager.Init()
        ├─ ArchiveManager.Init()        ← 构造函数已读存档元数据
        ├─ LuaManager.Init()
        │     ├─ 注册 Loader 链
        │     ├─ [非热重载] LoadLuaMapper()
        │     └─ ExecuteLua(ROOT_LUA_URI)
        ├─ WwiseAudioManager.Init()           ← 框架：Init 宿主； gameplay Bank 进游戏再载（2.5c）
        │
        _gameInitFinished = true
        │
        └─ ScenesManager.GoToMainScene()      ← 主界面必要：主 Panel + 主场景
              ├─ ShowPanel(Settings.MAIN_PANEL_NAME)
              └─ LoadSceneAsync(Settings.MAIN_SCENE_NAME)

主界面 → 进游戏（点击「开始」等，**非** Logo 链）：
    MainPanel / GameState.MainMenu
        └─ Scene 模块 · 进游戏 Loading（2.5c：ScenesManager + LoadingPanel）
              └─ GameBootstrap.EnterGameplayAsync()（游戏层加载清单）
                    ├─ LoadTable<RoleInfo> 等配表
                    ├─ 预载关卡场景 / Addressables
                    └─ Wwise Bank 等
        └─ 进入 Gameplay 场景 / GameState.Gameplay（5.3）
```

> **资源分层（§8）**：Logo→主界面尽量轻；角色表、关卡资源、音频 Bank 在「进游戏 Loading」阶段加载。  
> **待优化**：启动链中 `PreloadAssetsAsync(MAIN_SCENE_NAME)` 是否保留仅主界面体量，2.5c 落地后再收敛。

### 已知生命周期问题（待阶段 3 解决）

- 部分 Manager 在**构造函数**里做重活（`UIManager`、`ArchiveManager`），`Init()` 反而是空的
- `LuaManager.Init()` 内部 fire-and-forget，`CmgmFrameBoot` 不 await Lua 真正就绪
- 无统一模块注册 / 依赖顺序 / 失败回滚

---

## 5. 耦合点（框架化的主要障碍）

以下代码属于**游戏层**，但目前放在框架 Scripts 中，迁移新项目时必须改框架源码：

| 耦合点 | 位置 | 问题 |
|--------|------|------|
| 主界面 Panel | `CmgmFrameSettings.MAIN_PANEL_NAME` → `ShowPanel(name)`（2.4 ✅） | 换项目改 Settings |
| 主场景名 | `CmgmFrameSettings.MAIN_SCENE_NAME`（2.4 ✅） | 换场景改 Settings |
| 存档数据结构 | `GameRuntimeData`（博物、任务、背包…） | 游戏专属字段 |
| 配表容器 | `RoleInfo` 等（`Scripts/Game/Config/`，2.3 ✅） | 游戏专属表结构 |
| Lua 桥接 | `LuaBridge.Talk` | 空实现，且写死在框架里 |
| 第三方绑定 | Wwise / Odin / URP 直接引用 | 换音频 / RP 需改 Core |

### 其他技术债

| 问题 | 位置 | 计划阶段 |
|------|------|----------|
| namespace / asmdef | `GameCore` 已有 `CMGM.Core` + asmdef（1.1~1.3 ✅）；其余模块待阶段 2 模块闭环（2.1b~2.8） | 见 §7.1 |
| `BinaryFormatter` 序列化 | `ArchiveManager` | 阶段 4 |
| 无 GameState 状态机 | — | 阶段 5 |
| 无事件总线 | `OptionalSystem/` | 阶段 6 |

---

## 6. 目标架构（三层）

```
┌─────────────────────────────────────────────────┐
│  YourGame.Runtime（每个项目独有）                  │
│  GameRuntimeData / 配表 Container / Panel /       │
│  GameBootstrap.EnterGameplayAsync                 │
└───────────────────────┬─────────────────────────┘
                        │ 可选依赖
┌───────────────────────▼─────────────────────────┐
│  CmgmGameKits（可选游戏工具包，与框架同发，§6.4）   │
│  角色控制器 / 场景触发器 / 相机控制 等模板           │
└───────────────────────┬─────────────────────────┘
                        │ 依赖
┌───────────────────────▼─────────────────────────┐
│  CMGM.Modules（可选框架模块）                      │
│  UI / Data / Scene / Lua / Audio / Input …       │
└───────────────────────┬─────────────────────────┘
                        │ 依赖
┌───────────────────────▼─────────────────────────┐
│  CMGM.Core（框架核心，跨项目复用）                  │
│  单例 / 资源 / UI 基类 / 存档接口 / IGameModule …   │
└───────────────────────┬─────────────────────────┘
                        │ 组合（引用 Core + 已选 Modules）
┌───────────────────────▼─────────────────────────┐
│  CMGM.Bootstrap（组合根，§6.5）                   │
│  CmgmFrameBoot：InitScene 入口，**不**反向污染 Core │
└─────────────────────────────────────────────────┘
```

### 目标目录（2.1a 起逐步落地，远期 UPM 化）

```
Assets/_WorkSpace/
  HotRes/  Excels/
  Scripts/
    Game/                          游戏专属（Panel、RuntimeData、配表 Container…）
      UI/Panels/
      Archive/                     2.2 运行时存档结构
      Config/                      2.3 配表 Container
      Bootstrap/                   2.5 GameBootstrap（进游戏加载入口）
    CmgmGameKits/                  可选游戏工具包（与框架同发，§6.4；**非** Framework/Modules）
      RoleControl/                 2D/3D 角色控制器模板
      MapTriggers/                 场景触发器模板
      Camera/                      （规划）相机控制
    Framework/
      CmgmFrameBoot.cs             过渡：无 asmdef；3.x → Bootstrap/
      Core/                        必选（原 GameCore 内容直接在此，无 GameCore 子目录）
      Modules/                     可选，按项目勾选（§6.1、§6.3）
        UI/                        原 GameUI
        Data/                      原 GameData（CMGM.Data）
          Archive/                 ArchiveManager、I_Saveable
          Config/                  ConfigTableManager
          Editor/                  ExcelTool、ArchiveEditor
        Bootstrap/                 3.x 组合根；`CmgmFrameBoot`（§6.5）
        Scene/                     ScenesManager、进游戏 Loading（2.5c）；**不含** Boot / 玩法工具
        Lua/                       原 LuaCore
        Audio/                     原 AudioSystem
        Input/                     原 GameInput
        Optional/                  原 OptionalSystem
        Utils/
      Editor/                      框架级 Editor（2.8）；路径检查、模板、模块导入向导（8.7）
  Resources/CmgmFrameSettings.asset

Packages/（远期）
  com.cmgm.core/                   由 Framework/Core 导出
  com.cmgm.modules.*/              由 Framework/Modules/* 按需拆包
```

### 6.1 框架模块分级（Core / 可选 Modules）

| 层级 | 目录 | 模块 id | 默认 | 说明 |
|------|------|---------|------|------|
| **Core** | `Framework/Core/` | `Core` | **必选** | 原 `GameCore/`；`CMGM.Core` asmdef |
| **Modules** | `Framework/Modules/UI/` | `UI` | 推荐 | 原 `GameUI/`；`CMGM.UI` |
| **Modules** | `…/Data/` | `Data` | 推荐 | 原 `GameData/`；`CMGM.Data` |
| **Modules** | `…/Scene/` | `Scene` | 推荐 | 原 `GameLevel/`；`CMGM.Scene`（曾用名 Level / `CMGM.Level`） |
| **Modules** | `…/Lua/` | `Lua` | 可选 | 原 `LuaCore/`；`CMGM.Lua` |
| **Modules** | `…/Audio/` | `Audio` | 可选 | 原 `AudioSystem/`；`CMGM.Audio` |
| **Modules** | `…/Input/` | `Input` | 可选 | 原 `GameInput/`；`CMGM.Input` |
| **Modules** | `…/Optional/` | `Optional` | 可选 | 原 `OptionalSystem/`；事件总线等 |
| **Modules** | `…/Utils/` | `Utils` | 按需 | 通用工具 |

**跨项目导入（后续迭代）：**

| 阶段 | 内容 |
|------|------|
| **2.1a** | 物理目录 + **去 `Game*` 前缀重命名**（§6.3）；更新 `Consts.Paths` / 路径检查 |
| **2.1b~2.8** | 各模块 asmdef 建在对应子目录下 |
| **2.8** | `_WorkSpace/Editor/` → `Framework/Editor/`（与 Core **并列**，见 §6.2） |
| **7.5** | `CmgmModuleManifest`（ScriptableObject 或 JSON）：声明模块 id、依赖链、是否必选 |
| **8.7** | Editor「新项目 / 模块导入」：勾选 Modules，生成 asmdef 引用与目录检查清单 |

最小 JRPG 示例：`Core` + `UI` + `Data` + `Scene` + `Lua`  
最小 MUG 示例：`Core` + `UI` + `Audio` + `Input`（+ `Scene` 若走统一 Init）

### 6.2 Editor 目录分层（Runtime 与 Editor 分离）

| 位置 | 内容 | 阶段 | 说明 |
|------|------|------|------|
| `Framework/Core/` | Runtime | 2.1a | 原 `Scripts/GameCore/` 内容；**不含** Editor；`CMGM.Core` |
| `Framework/Modules/*/Editor/` | 模块 Editor | 2.1b~2.8 | 如 `ExcelTool`→Data、`Edt_CreateUIPanelAction`→UI |
| `Framework/Editor/` | 框架级 Editor | **2.8 M6** | 由原 `_WorkSpace/Editor/` 迁入：路径检查、通用模板、**8.7** 导入向导 |
| `Scripts/Game/Editor/` | 游戏 Editor（按需） | 远期 | 仅本项目策划/关卡工具，不随框架复制 |

**为何不放进 `Framework/Core/`？**

1. **程序集边界**：`CMGM.Core` 是 Runtime；Editor 需独立 `CMGM.*.Editor` asmdef，且常 `includePlatforms: Editor`。
2. **依赖范围**：工作区 Editor 常横切多个 Modules（UI 模板 + 配表路径 + manifest），放在 Core 下易让人误以为「只依赖 Core Runtime」。
3. **可选模块导入**：**8.7** 勾选 Modules 时，`Framework/Editor/` 作为框架壳层保留；各 `Modules/*/Editor/` 随模块一并勾选或跳过。

**结论：** `_WorkSpace/Editor/` → **`Framework/Editor/`**（与 `Core/`、`Modules/` **同级**），**不要**塞进 `Framework/Core/`。

### 6.3 模块目录重命名（2.1a，去 `Game*` 前缀）

与 `Scripts/Game/` 游戏层区分，迁入 `Framework/` 时**统一去掉 `Game` 前缀**（`AudioSystem`→`Audio` 等同步缩短）。namespace / asmdef 在对应 **M 闭环**步骤与目录对齐。

| 现目录（`Scripts/` 下） | 2.1a 目标 | 计划 namespace / asmdef |
|-------------------------|-----------|-------------------------|
| `GameCore/` | `Framework/Core/` | `CMGM.Core`（已有） |
| `GameUI/` | `Framework/Modules/UI/` | `CMGM.UI` |
| `GameData/` | `Framework/Modules/Data/` | `CMGM.Data` |
| `GameLevel/` | `Framework/Modules/Scene/` | `CMGM.Scene` |
| `LuaCore/` | `Framework/Modules/Lua/` | `CMGM.Lua` |
| `AudioSystem/` | `Framework/Modules/Audio/` | `CMGM.Audio` |
| `GameInput/` | `Framework/Modules/Input/` | `CMGM.Input` |
| `OptionalSystem/` | `Framework/Modules/Optional/` | `CMGM.Optional` |
| `Utils/` | `Framework/Modules/Utils/` | 随模块闭环 |

> **Core 无 `GameCore` 子文件夹**：原 `GameCore/` 内文件直接进入 `Framework/Core/`，不再嵌套一层 `GameCore/`。

### 6.4 CmgmGameKits（可选游戏工具包）

与 **`Framework/` 框架**并列、计划**与框架一起发布**，但**不属于** `Framework/Modules`，也**不参与** M1~M6 模块闭环主路线。

| 项 | 说明 |
|----|------|
| **目录** | `Scripts/CmgmGameKits/`（与 `Scripts/Game/`、`Scripts/Framework/` 同级） |
| **定位** | 跨项目可复用的**游戏层工具模板**（比框架 Modules 更贴近玩法，比 `Scripts/Game/` 更通用） |
| **示例内容** | `RoleControl/`（2D/3D 角色控制器）、`MapTriggers/`（场景触发器）、`Camera/`（相机控制，规划） |
| **依赖** | 引用已选 Framework Modules（UI、Scene、Input 等）；**框架不反向依赖 GameKits** |
| **与 `Scripts/Game/`** | `Game/` = 本项目独有（`MainPanel`、`RoleInfo`…）；GameKits = 可抄可删的模板库 |
| **asmdef** | 远期 `CMGM.GameKits`（**K 闭环**，见 §7.1 **8.K**）；框架 2.x 完成后独立推进 |
| **优先级** | **低**；与阶段 0~7 框架主线**解耦**，框架可单独发布，GameKits **最后补充** |

> **记录：** 2026-06-16 用户新建 `CmgmGameKits/`，自原 `Level/` 占位迁出 `RoleControl/`、`MapTriggers/`。

### 6.5 Bootstrap 与组合根（`CmgmFrameBoot` 放哪）

**问题：** `CmgmFrameBoot` 要调 `UIManager`、`ArchiveManager`、`LuaManager`、`WwiseAudioManager`、`ScenesManager`——若放进 **`CMGM.Core`**，Core 必须 `references` 各 Module 程序集，**依赖倒置**，与 §6.1「Modules 依赖 Core」冲突。

**结论：Boot 不能归 Core（在直接引用具体 Manager 的前提下）。**

| 层级 | 放什么 | 程序集依赖方向 |
|------|--------|----------------|
| **Core** | `IGameModule`、`CmgmInitContext`、接口与基础设施 | 不引用 UI / Data / Scene / Lua / Audio |
| **Modules** | 各 `*Manager` 实现 `IGameModule`（阶段 3） | Module → Core |
| **Bootstrap（组合根）** | `CmgmFrameBoot`、模块注册表 / Manifest 解析 | Bootstrap → Core + **已选** Modules |
| **Game / GameKits** | 业务与可选模板 | → Bootstrap 或 Modules，不被 Core 引用 |

```
Modules ──► Core          ✅ 正确
Core ──► Modules          ❌ 禁止（Boot 若进 Core 且直接调 Manager 会触发）
Bootstrap ──► Core + Modules   ✅ 组合根例外：允许「知道一切」，但不被 Core/Modules 引用
```

**阶段 3 目标：**

1. **3.1** Core 定义 `IGameModule`；Boot **只**持有模块列表并 `await InitAsync`，不 spread 具体类型到 Core。
2. **3.5** `CmgmFrameBoot` 迁至 `Framework/Bootstrap/`，新建 **`CMGM.Bootstrap`** asmdef（引用当前项目启用的 Modules）。
3. InitScene 仍挂同一 MonoBehaviour；改的是**目录 + 程序集**，不是场景逻辑。

**当前过渡（2.5b~3.x）：** Boot 在 `Framework/CmgmFrameBoot.cs`，无 namespace/asmdef；**不是**最终归属。阶段 3 再迁入 `Framework/Bootstrap/`（`CMGM.Bootstrap`）或等价组合根程序集。

---

## 7. 改造路线图

每步改动量可控，按顺序推进。**✅ = 已完成；待做 = 未开始；⏸ = 已尝试后回退。**  
已完成步骤**保留原文不删减**，只改状态列，便于回顾曾做过什么。

### 7.0 迭代原则（2026-06-15 修订）

原方案「阶段 1 先给全部模块加 asmdef → 阶段 2 再框架/游戏分层」在 1.3 实践中暴露问题：**框架与游戏代码仍混在同一目录时拆程序集**，会引发跨程序集引用、XLua Gen/Runtime 分裂、以及为凑编译而改动业务逻辑（如 `MainPanel` 退出流程）等连锁错误。

**修订后的推进方式：按模块闭环**

对每个模块（除已稳定的 Core 外），按固定顺序做完再进入下一模块：

```
① 框架 / 游戏分离（该迁的游戏代码迁到 Scripts/Game/）
② 加 namespace（可与 ① 同步）
③ 创建 asmdef（边界干净后再建）
④ Unity 编译 + 进 Play 验证
```

**硬性约定：**

| 约定 | 说明 |
|------|------|
| Core 可先 asmdef | `CMGM.Core` 边界清晰，1.1~1.2b 已完成 |
| 其余模块后建 asmdef | 不在分层完成前给混合目录建程序集 |
| 禁止为修编译改业务 | 不得删改 Panel 按钮逻辑、场景跳转等；边界问题用迁移 / 接口 / 引用解决 |
| 动功能前先确认 | 任何可能影响运行时行为的改法，先与用户确认 |
| XLua 与 asmdef 同单 | 若给 XLua 建 asmdef，须同时把 `Gen/` 纳入同一程序集或重配 Generate Code，禁止 Runtime 与 Gen 分裂 |
| 小步验证 | 每模块闭环后编译 + 主流程 Play 一次，再开下一模块 |

**建议模块闭环顺序：** UI → Data → Scene → Lua → Audio / Input → Editor 工具

### 7.0b 子编号说明（2.x / 2.xa / 2.xb）

主表里的 **2.1、2.2…** 是阶段内主步骤；**字母后缀不是按 a→z 的执行顺序**，而是按**类型**区分。看 **「← 下一步」** 列和本表为准。

| 编号形式 | 含义 | 示例 |
|----------|------|------|
| **2.x** | 主步骤：迁移、配置化、Bootstrap 等 | 2.1 迁 Panel ✅；2.2 迁存档 |
| **2.xa** | **目录 / 布局**（仅 2.1a）：`Framework/` 收拢 + §6.3 去 `Game*` 前缀 | 在 **2.1b** 之前做 |
| **2.xb** | **M 模块闭环**：namespace + asmdef | 2.1b=M1 UI；2.3b=M2 Data；2.5b=M3 Scene… |
| **Mx** | 与 **2.xb** 同义，闭环索引用 | M1↔2.1b，见模块闭环索引表 |

**2.1 系列执行顺序（固定）：**

```
2.1 ✅ 迁 Panel 至 Scripts/Game/
  → 2.1a  Framework 目录 + 重命名（§6.3）
  → 2.1b  M1：CMGM.UI asmdef（Game 层不设框架级 asmdef，见 §7.4）
  → 2.2 …
```

> 曾用编号 **2.1c** 指目录步骤，已改为 **2.1a**，避免「c 在 b 后却先做」的误解。

**编号约定：** 主路线图见 **§7.1**（0→9 连续编号，只列当前有效步骤）。若聊天记录或旧文档出现已废止的旧编号，查 **§7.4**（前缀 `*原`，如 `*原1.3`）。

---

### 7.1 路线图明细（含状态）

| 阶段 | 内容 | 状态 |
|------|------|------|
| **0** 清理 | 0.1 配表 bug（`RoleInfoContainer` 字典类型、`ExcelTool` 生成器、`CmgmLog.fError` 不受 LOG 开关影响） | ✅ |
| **0** 清理 | 0.2 移除 `Singleton.cs` 对 NUnit 的错误引用 | ✅ |
| **0** 清理 | 0.3 `LuaManager`：去掉启动打印全部 Lua 源码；Addressables Loader 去同步阻塞；热重载跳过 `LoadLuaMapper` | ✅ |
| **0** 清理 | 0.4 编写本文档 `ARCHITECTURE.md` | ✅ |
| **1** 编译边界 | 1.1 给 `GameCore` 下所有类加 `namespace CMGM.Core` | ✅ |
| **1** 编译边界 | 1.2 创建 `CMGM.Core.asmdef`；`Consts` partial 收拢至 GameCore（后为 1.2b 合并单文件取代） | ✅ |
| **1** 编译边界 | 1.2b 合并 `Consts` 为 `Consts.Paths.cs`；`WorkSpace` 迁入 `CmgmFrameSettings`（§2b A/C） | ✅ |
| **1** 编译边界 | 1.3 路线图修订：采用「按模块闭环」；非 Core 的 asmdef 并入阶段 2 闭环步骤（§7.0） | ✅ |
| **2** 框架/游戏分层 | **2.1** 新建 `Scripts/Game/`，迁移 `MainPanel`、`SamplePanel`、`zzzTextPanel` 等游戏 Panel | ✅ |
| **2** 框架/游戏分层 | 2.1a 新建 `Framework/Core/`、`Framework/Modules/`；Runtime 迁入 + **§6.3 去 `Game*` 前缀** | ✅ |
| **2** 框架/游戏分层 | 2.1b **M1 闭环**：`CMGM.UI` + `.Editor` asmdef；`namespace CMGM.UI`；Game 层仅目录 + `CMGM.Game` namespace（**无框架级 Game asmdef**，§7.4）；`CmgmApplication.Quit` | ✅ |
| **2** 框架/游戏分层 | 2.2 迁移 `GameRuntimeData` 等至 `Scripts/Game/Archive/` | ✅ |
| **2** 框架/游戏分层 | 2.3 迁移游戏配表（如 `RoleInfoContainer`）至 `Scripts/Game/Config/`；**B** `Paths.Framework` / `Paths.Game` | ✅ |
| **2** 框架/游戏分层 | 2.3b **M2 闭环**：`CMGM.Data` + `CMGM.Data.Editor` asmdef | ✅ |
| **2** 框架/游戏分层 | 2.4 `ScenesManager` 配置化；`MAIN_SCENE_NAME` + `MAIN_PANEL_NAME` | ✅ |
| **2** 框架/游戏分层 | 2.5 `GameBootstrap`：游戏**进游戏**加载入口（`EnterGameplayAsync`）；Logo 链不载大表 | ✅ |
| **2** 框架/游戏分层 | 2.5b **M3 闭环**：`Scene` 模块 `CMGM.Scene` asmdef（`ScenesManager`；Boot 待 **3.5** → `CMGM.Bootstrap`） | ✅ |
| **2** 框架/游戏分层 | **2.5c** Scene · **进游戏 Loading**（`ScenesManager` + LoadingPanel；**不**新建 `Modules/Loading`） | **← 下一步** |
| **2** 框架/游戏分层 | 2.6 **M4 闭环**：`Lua` 模块 `CMGM.Lua` asmdef（与 XLua Generate Code 同单） | 待做 |
| **2** 框架/游戏分层 | 2.7 **M5 闭环**：`Audio` + `Input` 模块 asmdef | 待做 |
| **2** 框架/游戏分层 | 2.8 **M6 闭环**：Editor namespace + asmdef | 待做 |
| **3** Bootstrap | 3.1 定义 `IGameModule` + `CmgmInitContext`（放 **Core**，无 Module 引用） | 待做 |
| **3** Bootstrap | 3.2~3.4 将各 Manager 改为 Module，构造函数不再做重活 | 待做 |
| **3** Bootstrap | 3.5 `CmgmFrameBoot` 迁至 `Framework/Bootstrap/`（**`CMGM.Bootstrap`** 组合根）；按 Order await 注册模块 | 待做 |
| **3** Bootstrap | 3.6 游戏项目在 `GameBootstrap` 注册自己的 Module | 待做 |
| **4** 存档升级 | 4.1~4.6 分块存档、`ISaveChunk`、版本头、替换 `BinaryFormatter`、迁移示例 | 待做 |
| **5** GameState | 5.1~5.5 状态机基础态 + Pause/Cutscene/Battle 预留 | 待做 |
| **6** 事件总线 | 6.1~6.4 `IEventBus` 落地 OptionalSystem，替代一处直接调用 | 待做 |
| **7** 依赖抽象 | 7.1~7.4 `IAudioService`、`ILuaBridgeRegistry`、Odin 降级、URP 文档或抽象 | 待做 |
| **7** 依赖抽象 | 7.5 `CmgmModuleManifest`：模块 id、Core/Modules 分级、依赖链（§6.1） | 待做 |
| **8** 内容扩展 | 8.1 对话；8.2 场景持久化；8.3 配表类型扩展；8.4 Lua 懒加载 + **E** 路径代码生成（§2b）；8.5 MUG；8.6 SRPG 接口 | 按需 |
| **8** 内容扩展 | 8.7 Editor 模块导入向导：勾选 `Framework/Modules` 子目录，校验 asmdef / 场景引用 | 待做 |
| **8.K** **GameKits**（低优先级，与框架主线独立） | `CmgmGameKits`：`CMGM.GameKits` asmdef、RoleControl / MapTriggers / Camera 等模板；与框架同发、**最后补充**（§6.4） | 待做 |
| **9** 网游预埋 | 9.1~9.4 LocalSave vs ServerSync、网络层、战斗重放、Cloud save | 远期 |

---

### 7.2 九阶段详细任务表

下表是 §7.1 的展开版，便于排期与验收。

#### 阶段 0 · 清理 ✅

| 编号 | 任务 | 涉及文件 / 范围 | 验收 |
|------|------|-----------------|------|
| 0.1 | 修复配表生成：`ExcelTool` 写对 `{TableName}Row`；`RoleInfoContainer.dataDic` 类型；`CmgmLog.fError` 不受 LOG 开关影响 | `ExcelTool.cs`、`RoleInfoContainer.cs`、`CmgmLog.cs` | 导表 + 运行时读表无类型错误 |
| 0.2 | 移除 `Singleton.cs` 对 NUnit 的错误引用 | `Singleton.cs` | 无测试框架污染 |
| 0.3 | `LuaManager` 启动行为整理：不打印全部 Lua；Addressables Loader 去 `.GetResult()` 阻塞；热重载跳过 `LoadLuaMapper` | `LuaManager.cs`、Loader 链 | Editor 热重载、Player 正式加载均正常 |
| 0.4 | 编写并维护 `ARCHITECTURE.md` | 本文档 | 路线图与模块清单可查 |

#### 阶段 1 · 编译边界 ✅

| 编号 | 任务 | 涉及文件 / 范围 | 验收 |
|------|------|-----------------|------|
| 1.1 | `GameCore` 全部类加 `namespace CMGM.Core` | `Framework/Core/**`（原 `Scripts/GameCore/`） | 外部 `using CMGM.Core` 编译通过 |
| 1.2 | 创建 `CMGM.Core.asmdef`；Consts partial 收拢 | `CMGM.Core.asmdef`、`Consts.*` | 仅 Core 独立程序集 |
| 1.2b | 合并 Consts 单文件；`WorkSpace` 迁入 Settings | `Consts.Paths.cs`、`CmgmFrameSettings.cs` | 改 Settings 即可换工作区根路径 |
| 1.3 | 路线图修订：「按模块闭环」；非 Core 的 asmdef 并入阶段 2 之 2.1b~2.8 | 本文档 §7 | 主表 §7.1 连续 0→9 |

#### 阶段 2 · 框架 / 游戏分层 + 模块 asmdef

| 编号 | 任务 | 详细内容 | 验收 |
|------|------|----------|------|
| **2.1** ✅ | 迁出游戏 Panel | `Scripts/Game/UI/Panels/` | 主界面 Play 正常 |
| **2.1a** ✅ | Framework 目录 + 重命名 | §6.3 映射；`Consts.Paths.Framework.*` | 编译通过 |
| **2.1b M1** ✅ | UI 模块闭环 | `CMGM.UI`、`CMGM.UI.Editor` asmdef；`CmgmApplication.Quit`；Game 层 namespace 保留，**不建** `CMGM.Game` asmdef（§7.4） | ShowPanel / HidePanel 正常 |
| **2.2** ✅ | 迁出游戏存档结构 | `GameRuntimeData` → `Scripts/Game/Archive/`；`I_Saveable` → `Framework/Modules/Data/Archive/`；`ArchiveManager` 通用读写 | 读档 / 存档流程不变 |
| **2.3** ✅ | 迁出游戏配表 | `RoleInfoContainer` → `Scripts/Game/Config/`；`ExcelTool` 输出至 `Paths.Game.Config`；生成类带 `namespace CMGM.Game` | Editor 导表 + `LoadTable<RoleInfo>()` 正常 |
| **2.3b M2** ✅ | Data 模块闭环 | `CMGM.Data` + `CMGM.Data.Editor` asmdef；`namespace CMGM.Data` / `CMGM.Data.Editor`；Editor 收拢至 `Data/Editor/` | 编译 + 导表 + 存档 Init |
| **2.4** ✅ | ScenesManager 配置化 | `CmgmFrameSettings` 配置主场景 + 主 Panel；`GoToMainScene` 用字符串 `ShowPanel` / `LoadSceneAsync` | 换主 UI/主场景只改 Settings |
| **2.5** ✅ | GameBootstrap | `EnterGameplayAsync`：主界面→进游戏时加载配表/资源/音频；**不在** Logo→主界面链 | MainPanel「开始」可触发 |
| **2.5b M3** ✅ | Scene 模块闭环 | `CMGM.Scene` asmdef + `namespace CMGM.Scene`（目录 `Modules/Scene/`；曾用 Level）；Boot 暂留默认程序集，**最终**归 `CMGM.Bootstrap`（§6.5，3.5） | Logo → 框架 Init → 主场景 |
| **2.5c** | Scene · 进游戏 Loading | `ScenesManager` 扩展 + LoadingPanel；编排 `EnterGameplayAsync`、Addressables/切场景进度 | 主界面→进游戏有进度条 |
| **2.6 M4** | Lua 模块闭环 | `CMGM.Lua` + XLua 同单 | Lua 启动、`require`、C# 桥接无类型分裂错误 |
| **2.7 M5** | Audio + Input 闭环 | `CMGM.Audio`、`CMGM.Input` asmdef | 音频事件、输入 map 正常 |
| **2.8 M6** | Editor 模块闭环 | `_WorkSpace/Editor/` → `Framework/Editor/`；各 `Modules/*/Editor/` + `CMGM.Editor` asmdef（§6.2） | 路径检查、模板、导入向导可用 |
| **2.5c** | Scene · 进游戏 Loading | `Framework/Modules/Scene/`：`ScenesManager` 扩展、LoadingPanel、分段加载；包装 `EnterGameplayAsync` | 主界面→进游戏有进度条；角色表等在此时加载 |

#### 阶段 3 · Bootstrap 模块化

| 编号 | 任务 | 详细内容 | 验收 |
|------|------|----------|------|
| 3.1 | 模块接口 | 定义 `IGameModule`（`Order`、`InitAsync(CmgmInitContext)`、`Shutdown` 等）与 `CmgmInitContext`（共享服务访问） | 接口文档 + 空实现可编译 |
| 3.2 | UI / 资源 Module | `UIManager`、`AddressablesResMgr` 改为 Module；构造函数不做重活 | Init 只在 `InitAsync` |
| 3.3 | 存档 / Lua Module | `ArchiveManager`、`LuaManager` 同上 | Lua Init 可被 await |
| 3.4 | 音频 Module | `WwiseAudioManager`（及可选 Input）注册为 Module | 启动顺序可配置 |
| 3.5 | 组合根 | `CmgmFrameBoot` → `Framework/Bootstrap/`（`CMGM.Bootstrap` asmdef）；收集 Module 列表，按 `Order` 依次 `await InitAsync` | Boot 不污染 Core；不被 Module 反向引用 |
| 3.6 | 游戏注册 | `GameBootstrap` 向框架注册游戏专属 Module（进游戏加载清单、GameState 入口等） | 新项目只改 Game 层注册 |

#### 阶段 4 · 存档升级

| 编号 | 任务 | 详细内容 | 验收 |
|------|------|----------|------|
| 4.1 | 存档版本头 | 文件头：magic + version + chunk 数量 | 旧档可识别 |
| 4.2 | 分块接口 | `ISaveChunk` / chunk 注册表；每块独立序列化 | 游戏只增 Game 层 chunk |
| 4.3 | 替换 BinaryFormatter | JSON / MemoryPack / 自定义二进制（择一） | 安全、可版本迁移 |
| 4.4 | 迁移管线 | `ISaveMigrator`：vN → vN+1 | 样例迁移测试 |
| 4.5 | 运行时 API | `SaveSlot` / 异步写盘 / 校验 | 多存档槽正常 |
| 4.6 | 示例与文档 | 在 `GameRuntimeData` 演示新增字段如何加 chunk | 策划 / 程序可查 |

#### 阶段 5 · GameState 状态机

| 编号 | 任务 | 详细内容 | 验收 |
|------|------|----------|------|
| 5.1 | 状态接口 | `IGameState`：`Enter` / `Exit` / `Update`（可选） | 基础态可切换 |
| 5.2 | 状态机宿主 | `GameStateMachine`：Push / Pop / Replace | 日志可追踪栈 |
| 5.3 | 基础态 | `Boot`、`MainMenu`、`Gameplay`、**`Loading`**（态内调用 **Scene·进游戏 Loading（2.5c）** + `GameBootstrap.EnterGameplayAsync`） | 与 ScenesManager 协作 |
| 5.4 | 预留态 | `Pause`、`Cutscene`、`Battle` 空壳或最小实现 | JRPG / SRPG 可扩展 |
| 5.5 | 与 UI / 输入 | 状态切换时 UI 层、输入 map 切换策略 | 暂停时输入正确 |

#### 阶段 6 · 事件总线

| 编号 | 任务 | 详细内容 | 验收 |
|------|------|----------|------|
| 6.1 | 接口 | `IEventBus`：`Subscribe` / `Publish` / `Unsubscribe` | 线程 / 生命周期约定文档 |
| 6.2 | 实现 | 落地于 `OptionalSystem/EventSystem` | 无全局静态散落 |
| 6.3 | 替代直连 | 选 1~2 处 Manager 直连改为事件（如场景切换完成） | 行为不变 |
| 6.4 | 与 Module 集成 | Bootstrap 注册 EventBus 单例 | 游戏可发订阅游戏事件 |

#### 阶段 7 · 依赖抽象

| 编号 | 任务 | 详细内容 | 验收 |
|------|------|----------|------|
| 7.1 | 音频抽象 | `IAudioService` 包装 Wwise；Core 不直接引用 Wwise API | 换音频后端只改 Extension |
| 7.2 | Lua 桥注册 | `ILuaBridgeRegistry`：游戏注册 `Talk` 等 API，框架不写空实现 | Lua 调 C# 游戏逻辑 |
| 7.3 | Odin 降级 | 框架 asmdef 去掉 Odin 硬依赖或 `UNITY_EDITOR` 条件编译 | 无 Odin 也能编 Core |
| 7.4 | URP / RP | 文档或 `IRenderPipeline` 薄抽象 | 换 RP 有检查清单 |
| 7.5 | 模块清单 | `CmgmModuleManifest`：Core vs Modules、模块间依赖、默认勾选集 | 新项目可读 manifest 知要带哪些文件夹 |

#### 阶段 8 · 内容扩展（按需）

| 编号 | 任务 | 详细内容 | 验收 |
|------|------|----------|------|
| 8.1 | 对话系统 | Lua / 配表驱动对话 UI 与分支 | 样例对话可跑 |
| 8.2 | 场景持久化 | 场景内对象 Save/Load 钩子 | 进出场景状态保留 |
| 8.3 | 配表类型扩展 | 多键表、嵌套结构、本地化列 | ExcelTool 支持 |
| 8.4 | Lua 懒加载 + **E** | 按需 `require`；扫描 `HotRes/` 生成路径常量 | 大包体启动更快 |
| 8.5 | MUG | 节拍、判定、谱面与 `MusicSyncTool` 整合 | 一曲可玩 |
| 8.6 | SRPG 接口 | 网格 / 回合 / 技能接口预留 | 与 GameState Battle 衔接 |
| 8.7 | 模块导入向导 | Editor 勾选 `Framework/Modules/*`；输出依赖报告 + 缺失 asmdef 提示 | 复制框架到新工程时可按需裁剪 |

#### 阶段 9 · 网游预埋（远期）

| 编号 | 任务 | 详细内容 | 验收 |
|------|------|----------|------|
| 9.1 | 存档分层 | LocalSave vs ServerSync 接口分离 | 单机不受影响 |
| 9.2 | 网络层 | 连接、心跳、消息编解码占位 | 可 mock 服务器 |
| 9.3 | 战斗重放 | 输入序列 + 确定性 tick 记录 | 回放一致 |
| 9.4 | Cloud save | 与 4.x 存档格式兼容的上传 / 合并策略 | 文档 + 伪代码 |

#### 模块闭环索引（M1~M6）

| 闭环 | 对应步骤 | 内容 |
|------|----------|------|
| **M1** | 2.1b | UI（`CMGM.UI`）；Game 层示例代码无 asmdef |
| **M2** | 2.3b | Data（`CMGM.Data`） |
| **M3** | 2.5b | Scene（`CMGM.Scene`） |
| **M4** | 2.6 | Lua（`CMGM.Lua`） |
| **M5** | 2.7 | Audio + Input |
| **M6** | 2.8 | Editor asmdef（最后） |
| **K** | **8.K** | **CmgmGameKits**（低优先级，框架完成后独立补充，§6.4） |

#### 阶段 8.K · CmgmGameKits（低优先级，与主线独立）

| 编号 | 任务 | 详细内容 | 验收 |
|------|------|----------|------|
| **8.K1** | 目录与 asmdef | `Scripts/CmgmGameKits/`；`CMGM.GameKits` asmdef；`namespace CMGM.GameKits` | 引用 Framework Modules 编译通过 |
| **8.K2** | RoleControl | 2D / 3D 通用角色控制器模板 | 示例场景可跑 |
| **8.K3** | MapTriggers | 可继承的场景触发器基类 + 常用变体 | 与 Scene `ScenesManager` 切场景无耦合 |
| **8.K4** | Camera | 跟随 / 边界等相机控制（按需） | 可选 |
| **8.K5** | 发布 | 与框架同仓库或同 UPM 包组；新项目可整包删除 | 文档 §6.4 |

---

### 7.4 废止计划附录

> **用途：** 记录已放弃的旧编号与决策背景；**不写入 §7.1 主表**，避免主路线图被插入说明打断。  
> **标记：** `*原1.x` = 已废止的原阶段 1 编号（`*` 前缀 + `原`）。聊天里若只说「旧 1.3」，一般指 `*原1.3`，不是当前有效的 **1.3**（路线图修订）。

**背景：** 原策略为「阶段 1 先给全部模块加 asmdef → 阶段 2 再分层」。该策略在 `*原1.3` 实践与回退后废止；`*原1.4`、`*原1.5` 未单独执行。有效 **1.3** 改为「按模块闭环」，asmdef 工作并入 §7.1 之 **2.1b~2.8**。

**记录时间：** 2026-06-16 08:27（UTC+8）

| 废止编号 | 原内容 | 结果 | 工作并入（§7.1 有效编号） |
|----------|--------|------|---------------------------|
| `*原1.3` | 批量给 `GameUI`、`GameData`、`LuaCore` 加 namespace + asmdef | ⏸ 曾尝试，已回退 | **2.1b** M1、**2.3b** M2、**2.6** M4 |
| `*原1.4` | 给 `AudioSystem`、`GameInput`、`GameLevel` 加 namespace + asmdef | 未执行，废止 | **2.5b** M3、**2.7** M5 |
| `*原1.5` | `_WorkSpace/Editor` 加 `CMGM.Editor` namespace + asmdef | 未执行，废止 | **2.8** M6 |

**与当前有效 1.3 的区别：**

| 编号 | 含义 |
|------|------|
| **1.3**（有效） | 文档策略修订：改为「按模块闭环」，asmdef 写入阶段 2 的 2.1b~2.8 |
| `*原1.3`（废止） | 旧计划：在分层前批量创建 GameUI / GameData / LuaCore 程序集 |

**延后决策（非废止，勿提前执行）：**

| 议题 | 当前做法 | 正式执行时机 | 说明 |
|------|----------|--------------|------|
| `I_Saveable` 归属 Archive 模块 | **已迁** `Archive/I_Saveable.cs`（`namespace CMGM.Data`）；**已建** `CMGM.Data` asmdef | — | 2.3b ✅ |
| **`CMGM.Game` asmdef** | **框架不创建**；`Scripts/Game/` 示例代码进默认 `Assembly-CSharp`，保留 `namespace CMGM.Game` | **各游戏项目自定** | 框架主迭代 `Framework/*` 程序集；JRPG / SRPG 等可自建 Game asmdef |
| `IGameFlowHandler` / GameFlow | 曾尝试，**已废止**（2.4 改为 Settings 字符串） | — | — |
| ~~独立 `Modules/Loading` + 2.9 M7~~（**已复活**，见下 2026-06-19） | — | — | — |
| 框架层 `Game*` 类名 | **2.5 起废止**新命名 | — | `CmgmFrameBoot`、`ArchiveManager`、`ConfigTableManager`；游戏层保留 `GameBootstrap` 等 |

**记录时间：** 2026-06-15（`I_Saveable` 提前迁移；`CMGM.Game` asmdef 移除）；2026-06-16（2.4 废止 GameFlow；Loading 不单独建 Modules；框架层去 Game 命名）

---

### 7.5 决策记录 · 2026-06-19（Loading / Scene 重定位）

> 这两条已与用户确认；**§7.1 主路线图的整体重排另行讨论**（迭代结构「主干线性 + 枝叶并行」议题进行中），此处先固化结论防止遗失。

**决策 1：Scene 不再作为独立 asmdef 模块（撤销 `CMGM.Scene`）**

| 内容 | 去向 |
|------|------|
| 场景加载原语 `LoadSceneAsync` | **下沉 Core**：`AddressablesResMgr.LoadSceneAsync(sceneName, mode, progress)`（与 `LoadAssetAsync` 并列，纯资源原语，不碰 UI） |
| `ScenesManager.GoToMainScene` / `QuitGame` | **流程控制**，待 **阶段 5 GameState** 接管；当前临时留在 `Modules/Scene/ScenesManager.cs`（默认程序集），UI 摄像机叠加逻辑留此 |
| `CMGM.Scene.asmdef` | **已删除**；`ScenesManager` 回默认 `Assembly-CSharp` |
| 未来扩展位 | 若需 **Additive 多场景 / 流式分块 / 场景持久化 / 转场动画**，再扩为完整 Scene 模块（YAGNI：现在不预建） |

> 之前「Level→Scene 重命名 + `CMGM.Scene` 闭环（2.5b）」的程序集部分**就此回退**；命名 Scene 仍保留为目录/namespace。

**决策 2：Loading 复活为独立模块 `Modules/Loading`（`CMGM.Loading`），分步迭代**

定位：**通用加载服务**——任意位置可调用，可选「显示全屏面板 / 后台静默」，聚合多源进度。横切 UI / 资源 / 音频等，属编排层（不进 Core）。

迭代计划（**不一次做完**，每步可编译可跑；可作为「枝叶」独立于主干推进）：

| 子步 | 内容 | 学习点 |
|------|------|--------|
| **L1** | 模块骨架：`LoadingManager.Run(tasks)` + 一根进度条面板；先跑通「执行一组任务并显示进度」 | 模块 asmdef、接口基础 |
| **L2** | `ILoadTask`（加载步骤抽象）+ 加权进度聚合（`权重 × 段内进度`，无内部进度的任务直接跳段） | 接口/多态、进度算法 |
| **L3** | 接通「进游戏」加载点：替代 MainPanel 直接 await，由 Loading 编排 `GameBootstrap` 回调 | 模块协作、回调注入（Loading 不引用 Game） |
| **L4** | 加载点 Profile（每加载点一个 ScriptableObject 静态清单）+ 程序化动态补充任务 | 数据驱动、策划友好 |
| **L5+** | 后台静默加载、转场动画、动态拼任务（按敌人 ID 等） | 进阶 |

**进度模型（L2 起）：** 每个 `ILoadTask` 带 `Weight`；总进度 = `Σ(已完成权重) + 当前任务权重 × 当前任务内部进度`。Addressables/场景用真实 `PercentComplete`；Bank/Init 等无中间进度者完成即跳段（必要时加假进度补间防卡顿感）。显示推荐「单条 + 当前阶段文案」，不展示多条并行子进度。

**配置形态（L4）：** 不做全局大表；**每个加载点一个 `.asset`（ScriptableObject）** 配静态资源，运行时按上下文（敌人 ID 等）程序化追加 `ILoadTask`。统一的是「执行器」，分散的是「清单」。

**记录时间：** 2026-06-19（Scene asmdef 撤销 + `LoadSceneAsync` 下沉 Core；Loading 复活为独立模块，定 L1~L5 分步计划）

---

## 8. 关键约定

### 配表管线

```
Excel（Excels/）
  → ExcelTool 导出
  → *Container.cs（行类 *Row + 容器类 *）
  → StreamingAssets/TableConfig/*.cmgm
  → ConfigTableManager.LoadTable<T>() / GetTable<T>()
```

- 容器类必须有 `Dictionary<K, VRow> dataDic` 字段
- `ConfigTableManager` 通过反射读 `dataDic` 泛型参数推断行类型

### 存档管线

```
GameRuntimeData（I_Saveable，Scripts/Game/Archive/）
  → ArchiveManager 序列化（Framework/Modules/Data/Archive/）
  → persistentDataPath/Archives/
```

### Game 层目录约定（Archive / Config 并列）

| 路径常量 | 目录 | 内容 |
|----------|------|------|
| `Paths.Game.Archive` | `Scripts/Game/Archive/` | 运行时存档结构（可变） |
| `Paths.Game.Config` | `Scripts/Game/Config/` | Excel 导出的配表 Container（只读） |
| `Paths.Framework.DataModule.Archive` | `Framework/Modules/Data/Archive/` | 存档框架（`ArchiveManager`、`I_Saveable`） |
| `Paths.Framework.DataModule.Config` | `Framework/Modules/Data/Config/` | 配表框架（`ConfigTableManager`） |
| `Paths.Framework.DataModule.Editor` | `Framework/Modules/Data/Editor/` | Data 模块 Editor（`ExcelTool`、`ArchiveEditor`） |

**不**把 Config 嵌套在 Archive 下：二者生命周期不同（配表 vs 存档）。

### UI 管线

```
ShowPanel<T>() → Addressables 加载 HotRes/UI/Panels/{T}.prefab
  → 挂到对应 E_UILayer 层 Canvas
```

- **主界面 / 主场景（2.4 ✅）**：`CmgmFrameSettings.MAIN_PANEL_NAME`、`MAIN_SCENE_NAME`；`ScenesManager.GoToMainScene` 统一调用。游戏内其他 Panel 仍优先 `ShowPanel<T>()`。
- **D（AssetAddresses）**：Settings 字符串已够用；Address 键集中管理留待后续按需做。

### 资源加载分层（主界面 vs 进游戏）

| 阶段 | 负责 | 应加载 | 不应加载（示例） |
|------|------|--------|------------------|
| **Logo → 主界面** | `CmgmFrameBoot` + `ScenesManager.GoToMainScene` | 框架 Manager Init、主 Panel、主场景（轻量）、UI 包 | 角色配表、关卡场景、Wwise gameplay Bank |
| **主界面 → 进游戏** | **Scene·进游戏 Loading（2.5c）** + `GameBootstrap.EnterGameplayAsync` | `LoadTable`、关卡 Addressables、音频 Bank、Gameplay 场景 | — |
| **运行时懒加载** | `GetTable` / Addressables 按需 | 非关键表、可选资源 | 已在 Loading 阶段声明的必需项 |

- `ConfigTableManager.GetTable<T>()` 仍保留懒加载兜底，但**进游戏必需表**应在 Loading 阶段显式 `LoadTable`。
- 换项目时在 `GameBootstrap.EnterGameplayAsync` 维护「进游戏加载清单」。

### 框架 / 游戏层命名（2.5 起）

| 侧 | 约定 | 示例 |
|----|------|------|
| **框架层**（`Framework/`） | 类型名**避免** `Game*` 前缀（与「游戏层」混淆）；可用 `Cmgm*`、`Archive*`、`ConfigTable*` 等 | `CmgmFrameBoot`、`ArchiveManager`、`ConfigTableManager` |
| **游戏层**（`Scripts/Game/`，`namespace CMGM.Game`） | 保留 `Game*` 当业务语义需要时 | `GameBootstrap`、`GameRuntimeData` |
| **Unity 引擎 API** | 不改动 | `GameObject`、`GamePlayActions`（Input 生成名） |
| **StreamingAssets** | 配表输出目录 **`TableConfig/`**（原 `GameConfig/`） | `Consts.Paths.ConfigData` |

### Lua 管线

```
CmgmFrameSettings.ROOT_LUA_URI（如 main.lua.txt）
  → ExecuteLua → GetLuaContent / Loader 链
  → LuaBridge 暴露 C# API 给 Lua
  → util.async_to_sync 包装异步回调
```

---

## 9. 文档维护

- 每完成一步，更新 **§7.1** 主表对应行的**状态列**（改为 ✅），**不要合并或删减已完成步骤的描述**
- **§7.1 保持 0→9 连续完整表**；策略变更、废止旧编号、历史回退记录写入 **§7.4**，不要在主表前插入分节
- 更新 **§3 模块清单**、**§5 耦合点**（如有变化）
- 新增模块时在 §3 登记；发现新耦合点在 §5 补充
- 重大架构决策记录在 §7.4 或 PR 描述中

---

*下一步：**2.5c** — Scene · 进游戏 Loading：`ScenesManager` 扩展 + LoadingPanel，编排 `EnterGameplayAsync` 进度。*
