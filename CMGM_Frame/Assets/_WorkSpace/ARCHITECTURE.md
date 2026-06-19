# CMGM 框架架构说明

> 本文档是框架化改造的长期参考（「北极星」）。  
> 目标：将当前工程从「带 Demo 的原型项目」逐步改造为「可跨项目迁移的框架」。  
> 最后更新：2026-06-19 04:28 — 迭代计划重排为「主线任务 + 支线任务」（§7）；旧线性路线图（0→9）已归档至 `ARCHITECTURE_DEPRECATED.md`。

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
├── ARCHITECTURE.md          ← 本文档（当前有效计划）
├── ARCHITECTURE_DEPRECATED.md ← 过时归档（旧线性路线图、废止条目）
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
├── Editor/                  ⚠ 待「编译边界2.8」迁入 `Framework/Editor/`（非 Core 内，见 §6.2）
└── Scripts/
    ├── Game/                游戏专属（已分层 ✅）
    │   ├── UI/Panels/
    │   ├── Archive/         GameRuntimeData（已迁 ✅）
    │   └── Config/          配表 Container（已迁 ✅）
    ├── Framework/           框架（已分层 ✅）
    │   ├── Core/            CMGM.Core asmdef、Consts.Paths
    │   └── Modules/
    │       ├── UI/          原 GameUI（CMGM.UI ✅）
    │       ├── Data/        原 GameData（CMGM.Data ✅）
    │       │   ├── Archive/ ArchiveManager、I_Saveable
    │       │   ├── Config/  ConfigTableManager
    │       │   └── Editor/  ExcelTool、ArchiveEditor（CMGM.Data.Editor）
    │       ├── Scene/       原 GameLevel / Level（asmdef 已撤销，见 §7.5）
    │       ├── Lua/         原 LuaCore
    │       ├── Audio/       原 AudioSystem
    │       ├── Input/       原 GameInput
    │       ├── Optional/    原 OptionalSystem
    │       └── Utils/
    └── GameBattle/          占位（未纳入 Framework，按需处理）
```

> **已完成**：`RoleInfoContainer` → `Scripts/Game/Config/`；`Consts.Paths.Framework` / `.Game` 分层。

### 路径常量入口

所有路径由 **`Consts.Paths`**（`Framework/Core/Consts.Paths.cs`）统一定义：

- **`WorkSpace`** — 从 `CmgmFrameSettings.WORK_SPACE_ROOT` 读取（默认 `Assets/_WorkSpace`）
- **共享**：`HotRes`、`ARCHIVE_PATH`、`ConfigData` 等
- **`Paths.Framework.*`** — 框架目录（`Core`、`Modules`）
- **`Paths.Game.*`** — 游戏层（`UI_Panels`、`Archive`、`Config`）
- **`Paths.Framework.DataModule.*`** — 框架 Data 模块（`Archive`、`Config`）

换项目时：改 Settings 里的根路径即可，不必改代码里的字符串。

---

## 2b. 路径与资源引用优化线

除主线/支线任务外，路径/地址相关改进按下列步骤穿插推进：

| 代号 | 内容 | 计划归属 | 状态 |
|------|------|----------|------|
| **A** | 合并 `Consts` 为单文件，去掉 partial | 已完成基线 | ✅ |
| **C** | `WorkSpace` 根路径迁入 `CmgmFrameSettings` | 已完成基线 | ✅ |
| **B** | 拆 `Consts.Paths.Framework` 与 `Consts.Paths.Game` | 已完成基线 | ✅ |
| **D** | Addressables 加载键独立为 `AssetAddresses` 或 Label 分组 | **Loading系统1.4** 同期（集中各加载点 Address 键）/ 按需 | 待做 |
| **F** | 关键 Prefab/SO 改用 `AssetReference`，减少字符串路径 | **Loading系统1.4** 同期（加载清单 `AssetReference` 化）/ 按需 | 按需 |
| **E** | 扫描 `HotRes/` 自动生成路径常量（代码生成） | Lua 部分见 **Lua系统1.3**；全量 `HotRes` 扫描属**内容扩展支线** / 远期 | 远期 |

---

## 3. 模块清单

### 3.1 核心模块（`Framework/Core/`，`CMGM.Core`）

| 组件 | 文件 | 职责 | 成熟度 |
|------|------|------|--------|
| 单例基类 | `Singleton/` | 懒汉 / Mono / AutoMono 三种单例 | ★★★ |
| 资源管理 | `ResourceManagement/AddressablesResMgr` | AB 加载、引用计数、防重复、预加载、**场景加载原语 `LoadSceneAsync`** | ★★★★ |
| 对象池 | `ResourceManagement/ObjectPool/` | 基础对象池 | ★★ |
| 帧设置 | `CmgmFrameSettings` | LOG、Lua 热重载、**工作区根路径**、根脚本路径 | ★★★ |
| 路径常量 | `Consts.Paths` | 由 Settings 派生的目录与管线路径 | ★★★ |
| 日志 | `CoreUtils/CmgmLog` | 分级日志；Error 不受 LOG 开关影响 | ★★★ |
| Mono 调度 | `MonoManager/` | Update 委托挂载 | ★★ |

### 3.2 UI（`Framework/Modules/UI/`，`CMGM.UI`）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `UIManager` | 分层 Canvas、异步加载 Panel、Hide 中途取消容错 | ★★★★ |
| `BasePanel` | Panel 基类 | ★★★ |
| `Panels/MainPanel` 等 | 已迁至 `Scripts/Game/UI/Panels/` ✅ | — |
| Editor 工具 | Panel 模板创建、快速搜索 | ★★★ |

### 3.3 数据（`Framework/Modules/Data/`，`CMGM.Data`）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `ConfigTableManager` | 读取 `.cmgm` 二进制配表（反射 + 解密） | ★★★★ |
| `ExcelTool`（Editor） | Excel → Container.cs + 二进制 | ★★★★ |
| `RoleInfoContainer` 等 | 游戏配表（`Scripts/Game/Config/`）✅ | — |
| `ArchiveManager` | 存档元数据 + 运行时数据读写 | ★★★ |
| `GameRuntimeData` | 游戏存档结构（`Scripts/Game/Archive/`）✅ | — |
| `CipherTool` | 配表 / 存档加解密 | ★★★ |

### 3.4 Lua（契约 `Core/ILuaService`；实现 `Framework/Integrations/Lua/`）

| 组件 | 职责 | 程序集 | 成熟度 |
|------|------|--------|--------|
| `ILuaService` | Lua 服务契约（执行脚本 / 桥接注册等） | `CMGM.Core`（asmdef） | 规划 |
| `LuaManager` | LuaEnv 生命周期、Loader 链、脚本执行；实现 `ILuaService` | `Assembly-CSharp` | ★★★ |
| `LuaBridge` | C# ↔ Lua 桥接（Talk / Wait / DebugLog）；保留全局 namespace（`main.lua` 调 `CS.LuaBridge`） | `Assembly-CSharp` | ★★ |

> **现状（2026-06-19 决策，见 §7.5）：** Lua **不单独建 asmdef**。XLua 退回官方 master（无 asmdef、待在 `Assembly-CSharp`）；契约 `ILuaService` 放 `CMGM.Core`，实现放 `Framework/Integrations/Lua/`（框架级第三方桥接，随 XLua 落 `Assembly-CSharp`）；`CmgmFrameBoot` 创建 `LuaManager` 并注册为 `ILuaService`。被 asmdef 封装的 Module 只依赖契约，不碰 XLua（依赖倒置）。

**Lua 加载策略：**

| 模式 | 预加载 | require / ExecuteLua |
|------|--------|----------------------|
| 热重载（Editor） | 跳过 `LoadLuaMapper` | Loader 1 / `GetLuaContent` 直读磁盘 |
| 正式包体 | `LoadLuaMapper` → `luaMapper` | Loader 3 查内存字典 |

### 3.5 启动编排（`CmgmFrameBoot`，过渡位置见 §6.5）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `CmgmFrameBoot` | InitScene 上的 **组合根**：Logo + 各框架 Manager Init + 调 `ScenesManager.GoToMainScene` | ★★ |

> **不归 Core、也不语义上属于 Scene**——它横切 UI / Data / Lua / Audio / Scene 等 Modules；**不能**放进 `CMGM.Core`（否则 Core 反向依赖 Modules，见 §6.5）。  
> **当前（过渡）：** `Framework/CmgmFrameBoot.cs`——无 namespace、无 asmdef；**主线「启动组合根3.1/3.2」**再定 `CMGM.Bootstrap` 分层。

### 3.6 场景 / 流程（`Framework/Modules/Scene/`）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `ScenesManager` | 场景切换、回主界面；调用 Core 的 `LoadSceneAsync` 原语 | ★★ |

> **现状（2026-06-19 决策，见 §7.5）：** `CMGM.Scene` asmdef **已撤销**，`ScenesManager` 回默认 `Assembly-CSharp`；`LoadSceneAsync` 原语已**下沉 Core**（`AddressablesResMgr`）。  
> **`GoToMainScene` / `QuitGame`** 属流程控制，待**支线「GameState系统」**接管。  
> **进游戏 Loading** 改由**支线「Loading系统」**编排（不再绑在 Scene 模块）。  
> **未来若需** Additive 多场景 / 流式分块 / 场景持久化 / 转场动画，再扩为完整 Scene 模块（YAGNI：现在不预建）。

### 3.7 可选模块

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `WwiseAudioManager` | Wwise 通用音频（Bank / 播放 / 音量）；**编译边界2.7a 起去节拍化** | ★★★ |
| ~~`MusicSyncTool`~~ | MUG 节拍同步——**编译边界2.7b 迁出**至 `CmgmGameKits/MusicGame/BeatSync`（不再属 Audio 模块） | ★★★ |
| `InputManager` | Input System 封装（GamePlay / UI 两套 action map；含 `Input→UI` 依赖，见 2.8） | ★★ |
| `OptionalSystem/EventSystem` | **未实现**（仅 .meta；支线「事件总线系统」落地） | ☆ |
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
        ├─ WwiseAudioManager.Init()           ← 框架：Init 宿主；gameplay Bank 进游戏再载
        │
        _gameInitFinished = true
        │
        └─ ScenesManager.GoToMainScene()      ← 主界面必要：主 Panel + 主场景
              ├─ ShowPanel(Settings.MAIN_PANEL_NAME)
              └─ LoadSceneAsync(Settings.MAIN_SCENE_NAME)   ← 调 Core 原语

主界面 → 进游戏（点击「开始」等，**非** Logo 链）：
    MainPanel / GameState.MainMenu
        └─ 支线「Loading系统」编排进度（Loading系统1.3 接通）
              └─ GameBootstrap.EnterGameplayAsync()（游戏层加载清单）
                    ├─ LoadTable<RoleInfo> 等配表
                    ├─ 预载关卡场景 / Addressables
                    └─ Wwise Bank 等
        └─ 进入 Gameplay 场景 / GameState.Gameplay（GameState系统）
```

> **资源分层（§8）**：Logo→主界面尽量轻；角色表、关卡资源、音频 Bank 在「进游戏 Loading」阶段加载。  
> **待优化**：启动链中 `PreloadAssetsAsync(MAIN_SCENE_NAME)` 是否保留仅主界面体量，Loading系统落地后再收敛。

### 已知生命周期问题（待「启动组合根3.1」解决）

- 部分 Manager 在**构造函数**里做重活（`UIManager`、`ArchiveManager`），`Init()` 反而是空的
- `LuaManager.Init()` 内部 fire-and-forget，`CmgmFrameBoot` 不 await Lua 真正就绪
- 无统一模块注册 / 依赖顺序 / 失败回滚

---

## 5. 耦合点（框架化的主要障碍）

以下代码属于**游戏层**，但目前放在框架 Scripts 中，迁移新项目时必须改框架源码：

| 耦合点 | 位置 | 问题 |
|--------|------|------|
| 主界面 Panel | `CmgmFrameSettings.MAIN_PANEL_NAME` → `ShowPanel(name)` ✅ | 换项目改 Settings |
| 主场景名 | `CmgmFrameSettings.MAIN_SCENE_NAME` ✅ | 换场景改 Settings |
| 存档数据结构 | `GameRuntimeData`（博物、任务、背包…） | 游戏专属字段 |
| 配表容器 | `RoleInfo` 等（`Scripts/Game/Config/`）✅ | 游戏专属表结构 |
| Lua 桥接 | `LuaBridge.Talk` | 空实现，且写死在框架里 |
| 第三方绑定 | Wwise / Odin / URP 直接引用 | 换音频 / RP 需改 Core |

### 其他技术债

| 问题 | 位置 | 计划归属 |
|------|------|----------|
| namespace / asmdef | `GameCore`/`UI`/`Data` 已闭环；`Lua`/`Audio`/`Input`/`Editor` 待主线「编译边界2.6~2.8」 | §7.2 |
| `BinaryFormatter` 序列化 | `ArchiveManager` | 支线「存档升级系统」 |
| 无 GameState 状态机 | — | 支线「GameState系统」 |
| 无事件总线 | `OptionalSystem/` | 支线「事件总线系统」 |

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

### 目标目录（逐步落地，远期 UPM 化）

```
Assets/_WorkSpace/
  HotRes/  Excels/
  Scripts/
    Game/                          游戏专属（Panel、RuntimeData、配表 Container…）
      UI/Panels/
      Archive/                     运行时存档结构
      Config/                      配表 Container
      Bootstrap/                   GameBootstrap（进游戏加载入口）
    CmgmGameKits/                  可选游戏工具包（与框架同发，§6.4；**非** Framework/Modules）
      RoleControl/                 2D/3D 角色控制器模板
      MapTriggers/                 场景触发器模板
      Camera/                      （规划）相机控制
    Framework/
      CmgmFrameBoot.cs             过渡：无 asmdef；启动组合根3.x → Bootstrap/
      Core/                        必选（原 GameCore 内容直接在此，无 GameCore 子目录）
        …/ILuaService.cs           Lua 服务契约（实现在 Integrations/Lua）
      Modules/                     可选，按项目勾选（§6.1、§6.3）；每个子目录 = 一个 asmdef 封装包
        UI/                        原 GameUI
        Data/                      原 GameData（CMGM.Data）
          Archive/                 ArchiveManager、I_Saveable
          Config/                  ConfigTableManager
          Editor/                  ExcelTool、ArchiveEditor
        Bootstrap/                 启动组合根；`CmgmFrameBoot`（§6.5）
        Scene/                     ScenesManager（asmdef 已撤销）；**不含** Boot / 玩法工具
        Loading/                   支线「Loading系统」：CMGM.Loading
        Audio/                     原 AudioSystem
        Input/                     原 GameInput
        Optional/                  原 OptionalSystem（事件总线等）
        Utils/
      Integrations/                框架级第三方桥接：无 asmdef，随第三方库落 Assembly-CSharp（§7.5）
        Lua/                       LuaManager / LuaBridge（XLua 官方 master，原 LuaCore）
      Editor/                      框架级 Editor（编译边界2.8）；路径检查、模板、模块导入向导
  Resources/CmgmFrameSettings.asset

Packages/（远期）
  com.cmgm.core/                   由 Framework/Core 导出
  com.cmgm.modules.*/              由 Framework/Modules/* 按需拆包
```

### 6.1 框架模块分级（Core / 可选 Modules）

| 层级 | 目录 | 模块 id | 默认 | 说明 |
|------|------|---------|------|------|
| **Core** | `Framework/Core/` | `Core` | **必选** | 原 `GameCore/`；`CMGM.Core` asmdef ✅ |
| **Modules** | `Framework/Modules/UI/` | `UI` | 推荐 | 原 `GameUI/`；`CMGM.UI` ✅ |
| **Modules** | `…/Data/` | `Data` | 推荐 | 原 `GameData/`；`CMGM.Data` ✅ |
| **Modules** | `…/Scene/` | `Scene` | 推荐 | 原 `GameLevel/`；曾用 `CMGM.Scene`（asmdef 已撤销，§7.5） |
| **Modules** | `…/Loading/` | `Loading` | 可选 | 支线「Loading系统」；`CMGM.Loading` |
| **Integrations** | `Framework/Integrations/Lua/` | `Lua` | 可选 | 原 `LuaCore/`；**不建 asmdef**，契约 `ILuaService` 入 Core、实现随 XLua(master) 落 `Assembly-CSharp`（§7.5） |
| **Modules** | `…/Audio/` | `Audio` | 可选 | 原 `AudioSystem/`；`CMGM.Audio`（编译边界2.7） |
| **Modules** | `…/Input/` | `Input` | 可选 | 原 `GameInput/`；`CMGM.Input`（编译边界2.7） |
| **Modules** | `…/Optional/` | `Optional` | 可选 | 原 `OptionalSystem/`；事件总线等 |
| **Modules** | `…/Utils/` | `Utils` | 按需 | 通用工具 |

**跨项目导入（后续迭代）：**

| 归属 | 内容 |
|------|------|
| **已完成基线** | 物理目录 + 去 `Game*` 前缀；UI / Data asmdef 闭环 |
| **主线 编译边界2.6~2.8** | Lua / Audio / Input / Editor 各模块 asmdef |
| **主线 启动组合根3.2** | `CmgmModuleManifest`（声明模块 id、依赖链、是否必选） |
| **内容扩展支线** | Editor「新项目 / 模块导入」向导：勾选 Modules，生成 asmdef 引用与目录检查清单 |

最小 JRPG 示例：`Core` + `UI` + `Data` + `Scene` + `Lua`  
最小 MUG 示例：`Core` + `UI` + `Audio` + `Input`（+ `Scene` 若走统一 Init）

### 6.2 Editor 目录分层（Runtime 与 Editor 分离）

| 位置 | 内容 | 归属 | 说明 |
|------|------|------|------|
| `Framework/Core/` | Runtime | 已完成基线 | 原 `Scripts/GameCore/` 内容；**不含** Editor；`CMGM.Core` |
| `Framework/Modules/*/Editor/` | 模块 Editor | 各模块闭环 | 如 `ExcelTool`→Data、`Edt_CreateUIPanelAction`→UI |
| `Framework/Editor/` | 框架级 Editor | **编译边界2.8** | 由原 `_WorkSpace/Editor/` 迁入：路径检查、通用模板、导入向导 |
| `Scripts/Game/Editor/` | 游戏 Editor（按需） | 远期 | 仅本项目策划/关卡工具，不随框架复制 |

**为何不放进 `Framework/Core/`？**

1. **程序集边界**：`CMGM.Core` 是 Runtime；Editor 需独立 `CMGM.*.Editor` asmdef，且常 `includePlatforms: Editor`。
2. **依赖范围**：工作区 Editor 常横切多个 Modules（UI 模板 + 配表路径 + manifest），放在 Core 下易让人误以为「只依赖 Core Runtime」。
3. **可选模块导入**：导入向导勾选 Modules 时，`Framework/Editor/` 作为框架壳层保留；各 `Modules/*/Editor/` 随模块一并勾选或跳过。

**结论：** `_WorkSpace/Editor/` → **`Framework/Editor/`**（与 `Core/`、`Modules/` **同级**），**不要**塞进 `Framework/Core/`。

### 6.3 模块目录重命名（已完成基线，去 `Game*` 前缀）

与 `Scripts/Game/` 游戏层区分，迁入 `Framework/` 时**统一去掉 `Game` 前缀**（`AudioSystem`→`Audio` 等同步缩短）。namespace / asmdef 在对应模块闭环步骤与目录对齐。

| 现目录 | 框架目标 | 计划 namespace / asmdef |
|--------|----------|-------------------------|
| `GameCore/` | `Framework/Core/` | `CMGM.Core` ✅ |
| `GameUI/` | `Framework/Modules/UI/` | `CMGM.UI` ✅ |
| `GameData/` | `Framework/Modules/Data/` | `CMGM.Data` ✅ |
| `GameLevel/` | `Framework/Modules/Scene/` | 曾 `CMGM.Scene`（已撤销） |
| `LuaCore/` | `Framework/Modules/Lua/` | `CMGM.Lua`（编译边界2.6） |
| `AudioSystem/` | `Framework/Modules/Audio/` | `CMGM.Audio`（编译边界2.7） |
| `GameInput/` | `Framework/Modules/Input/` | `CMGM.Input`（编译边界2.7） |
| `OptionalSystem/` | `Framework/Modules/Optional/` | `CMGM.Optional` |
| `Utils/` | `Framework/Modules/Utils/` | 随模块闭环 |

> **Core 无 `GameCore` 子文件夹**：原 `GameCore/` 内文件直接进入 `Framework/Core/`，不再嵌套一层 `GameCore/`。

### 6.4 CmgmGameKits（可选游戏工具包）

与 **`Framework/` 框架**并列、计划**与框架一起发布**，但**不属于** `Framework/Modules`，也**不参与**模块闭环主线。

| 项 | 说明 |
|----|------|
| **目录** | `Scripts/CmgmGameKits/`（与 `Scripts/Game/`、`Scripts/Framework/` 同级） |
| **定位** | 跨项目可复用的**游戏层工具模板**（比框架 Modules 更贴近玩法，比 `Scripts/Game/` 更通用） |
| **示例内容** | `RoleControl/`（2D/3D 角色控制器）、`MapTriggers/`（场景触发器）、`Camera/`（相机控制，规划）、`MusicGame/`（音游工具包，下设 `BeatSync/` 等子模块，见 §6.4a） |
| **依赖** | 引用已选 Framework Modules（UI、Scene、Input 等）；**框架不反向依赖 GameKits** |
| **与 `Scripts/Game/`** | `Game/` = 本项目独有（`MainPanel`、`RoleInfo`…）；GameKits = 可抄可删的模板库 |
| **asmdef** | 远期 `CMGM.GameKits`（支线「GameKits」，见 §7.3/§7.4）；框架主线完成后独立推进 |
| **优先级** | **低**；与框架主线**解耦**，框架可单独发布，GameKits **最后补充** |

> **记录：** 2026-06-16 用户新建 `CmgmGameKits/`，自原 `Level/` 占位迁出 `RoleControl/`、`MapTriggers/`。  
> **记录：** 2026-06-19 新建 `CmgmGameKits/MusicGame/`，承接编译边界2.7b 从 Audio 剥离的音游节拍代码。

#### 6.4a MusicGame 子模块规划（音游工具包内部分层）

`MusicGame/` 不是单一工具，而是一组音游能力，**在其下按子模块再分文件夹**（当前仅 `BeatSync` 落地，其余为规划占位，按需再做）：

| 子模块 | 职责 | 状态 |
|--------|------|------|
| **BeatSync/** | 节拍同步与判定窗口、节拍图（`BeatEvtList`）加载、判定状态机；含 `Editor/` 节拍图生成器 | 编译边界2.7b 落地 |
| **Beatmap/** | 正式谱面数据格式、解析、加载（当前 `BeatEvtList` 是其简化前身）；谱面编辑器 | 规划 |
| **Judgement/** | 命中判定、连击、分数、评级（现混在 `BeatSync` 内，后续可独立） | 规划 |
| **Note/**（或 Track/） | 音符 / 轨道的可视与生命周期（下落式 / 点击式） | 规划 |
| **AudioVisualization/** | 频谱 / 波形 / 律动特效 | 规划 |
| **RhythmInput/** | 音游输入采集与判定窗口接入（呼应"窗口应放输入侧"的注释） | 规划 |

> **依赖方向：** 以上子模块均可引用 Framework Modules（如 `CMGM.Audio`、`CMGM.Input`），**框架不反向依赖 MusicGame**。整包随框架发布、可整体删除。

### 6.5 Bootstrap 与组合根（`CmgmFrameBoot` 放哪）

**问题：** `CmgmFrameBoot` 要调 `UIManager`、`ArchiveManager`、`LuaManager`、`WwiseAudioManager`、`ScenesManager`——若放进 **`CMGM.Core`**，Core 必须 `references` 各 Module 程序集，**依赖倒置**，与 §6.1「Modules 依赖 Core」冲突。

**结论：Boot 不能归 Core（在直接引用具体 Manager 的前提下）。**

| 层级 | 放什么 | 程序集依赖方向 |
|------|--------|----------------|
| **Core** | `IGameModule`、`CmgmInitContext`、接口与基础设施 | 不引用 UI / Data / Scene / Lua / Audio |
| **Modules** | 各 `*Manager` 实现 `IGameModule`（启动组合根3.1） | Module → Core |
| **Bootstrap（组合根）** | `CmgmFrameBoot`、模块注册表 / Manifest 解析 | Bootstrap → Core + **已选** Modules |
| **Game / GameKits** | 业务与可选模板 | → Bootstrap 或 Modules，不被 Core 引用 |

```
Modules ──► Core          ✅ 正确
Core ──► Modules          ❌ 禁止（Boot 若进 Core 且直接调 Manager 会触发）
Bootstrap ──► Core + Modules   ✅ 组合根例外：允许「知道一切」，但不被 Core/Modules 引用
```

**主线「启动组合根」目标：**

1. **启动组合根3.1** Core 定义 `IGameModule`；Boot **只**持有模块列表并 `await InitAsync`，不 spread 具体类型到 Core。各 Manager 改为 Module，构造函数不再做重活。`CmgmFrameBoot` 迁至 `Framework/Bootstrap/`，新建 **`CMGM.Bootstrap`** asmdef（引用当前项目启用的 Modules）。InitScene 仍挂同一 MonoBehaviour；改的是**目录 + 程序集**，不是场景逻辑。
2. **启动组合根3.2** `CmgmModuleManifest` 声明模块 id / 依赖链 / 默认勾选；游戏项目在 `GameBootstrap` 注册自己的 Module。

**当前过渡：** Boot 在 `Framework/CmgmFrameBoot.cs`，无 namespace/asmdef；**不是**最终归属。主线「启动组合根」再迁入 `Framework/Bootstrap/`（`CMGM.Bootstrap`）。

---

## 7. 迭代计划（主线 / 支线）

> **模型（2026-06-19 重排）：** 旧的单一线性路线图（阶段 0→9）已**归档至 `ARCHITECTURE_DEPRECATED.md`**。现拆为两类：
> - **主线任务**：改动「编译边界 / 启动契约」的地基工作，牵动全局，**必须线性按序**。
> - **支线任务**：各功能系统，满足**解锁条件**后**可并行 / 按需推进**；每条支线内部线性。
>
> **命名约定：**
> - 主线：**功能性章节名 + 连续编号**（`编译边界2.6`、`启动组合根3.1`）。
> - 支线：**系统名 + 编号**（`Loading系统1.1`、`存档升级系统1.2`）。
> - 少用 `L1`/`M3` 等字母缩写（旧缩写仅在归档文档保留）。

### 7.0 迭代原则（模块闭环，2026-06-15 修订）

原方案「先给全部模块加 asmdef → 再框架/游戏分层」在实践中暴露问题：**框架与游戏代码仍混在同一目录时拆程序集**，会引发跨程序集引用、XLua Gen/Runtime 分裂、以及为凑编译而改业务逻辑等连锁错误。

**修订后：按模块闭环**——对每个模块（除已稳定的 Core 外），按固定顺序做完再进入下一模块：

```
① 框架 / 游戏分离（该迁的游戏代码迁到 Scripts/Game/）
② 加 namespace（可与 ① 同步）
③ 创建 asmdef（边界干净后再建）
④ Unity 编译 + 进 Play 验证
```

**硬性约定：**

| 约定 | 说明 |
|------|------|
| Core 可先 asmdef | `CMGM.Core` 边界清晰，已完成 |
| 其余模块后建 asmdef | 不在分层完成前给混合目录建程序集 |
| 禁止为修编译改业务 | 不得删改 Panel 按钮逻辑、场景跳转等；边界问题用迁移 / 接口 / 引用解决 |
| 动功能前先确认 | 任何可能影响运行时行为的改法，先与用户确认 |
| XLua 不建 asmdef | 官方 `feature/asmdef` 已被回滚（PR#1067 加、PR#1068 删），XLua 留官方 master、待在 `Assembly-CSharp`；Lua 模块改走「契约入 Core / 实现入 `Integrations`」，不给 XLua 套 asmdef（§7.5） |
| 小步验证 | 每模块闭环后编译 + 主流程 Play 一次，再开下一模块 |

> 主线「编译边界2.6~2.8」即按此四步推进。

### 7.1 已完成基线（截至 2026-06-19 04:28）

| 领域 | 已落地 |
|------|--------|
| Core 程序集 | `CMGM.Core` asmdef + `namespace CMGM.Core`；`Consts.Paths` 单文件；`WorkSpace` 根入 `CmgmFrameSettings` |
| 框架/游戏分层 | `Framework/Core`、`Framework/Modules`、`Scripts/Game` 三分；模块去 `Game*` 前缀 |
| UI 模块 | `CMGM.UI` + `CMGM.UI.Editor` 闭环 |
| Data 模块 | `CMGM.Data` + `CMGM.Data.Editor` 闭环；存档结构 / 游戏配表迁 `Scripts/Game` |
| 进游戏入口 | `ScenesManager` 配置化（`MAIN_SCENE_NAME` / `MAIN_PANEL_NAME`）；`GameBootstrap.EnterGameplayAsync` |
| 场景加载下沉 | `LoadSceneAsync` 下沉 `Core`（`AddressablesResMgr`）；**撤销** `CMGM.Scene` asmdef，`ScenesManager` 回默认程序集（2026-06-19 决策，见 §7.5） |
| Lua 收口（编译边界2.6） | XLua 退官方 master（核心回 `Assembly-CSharp`）；`ILuaService` 入 Core；`LuaManager`/`LuaBridge` 迁 `Integrations/Lua`（方案 C，见 §7.5） |

> 完整的旧线性步骤、验收表与废止记录见 `ARCHITECTURE_DEPRECATED.md`。

### 7.2 主线任务（地基，线性按序）

| 步骤编号 | 名称 | 解锁条件 | 状态 |
|----------|------|----------|------|
| **编译边界2.6** | Lua 模块收口（**方案 C**）：XLua 退官方 master；Core 加 `ILuaService` 契约；`LuaManager` 实现 `ILuaService`；`LuaManager`/`LuaBridge` 迁 `Framework/Integrations/Lua`（`Assembly-CSharp`）。注：`CMGM.Lua` asmdef 随 XLua 回退已消失；服务**注册/注入**留待 启动组合根3.1（当前 Boot 仍直接 `LuaManager.Instance`） | 已完成基线 ✅ | **完成 ✅（2026-06-19，Unity 编译 + Play 验证通过）** |
| **编译边界2.7** ✅ | Audio 模块解耦与闭环（拆 2.7a/b/c，见 §7.2a）：先把音游节拍剥离到 `CmgmGameKits/MusicGame`，再给纯净 Audio 上 `CMGM.Audio` | 编译边界2.6 完成 ✅ | **已完成** |
| **编译边界2.8** | Input 模块闭环（`CMGM.Input`）；附带体检音游输入是否需剥离、`Input→UI` 跨模块依赖处理 | 编译边界2.7 完成 ✅ | **可做** |
| **编译边界2.9** | Editor 闭环（`_WorkSpace/Editor` → `Framework/Editor`，`CMGM.Editor`） | 编译边界2.8 完成 | 🔒 |
| **启动组合根3.1** | `IGameModule` + `CmgmInitContext`（Core）；各 Manager 改 Module（构造函数不做重活）；`CmgmFrameBoot` → `Framework/Bootstrap`（`CMGM.Bootstrap`），按 Order await | 编译边界2.9 完成 | 🔒 |
| **启动组合根3.2** | 模块清单 `CmgmModuleManifest`（模块 id / Core·Modules 分级 / 依赖链 / 默认勾选）；游戏层在 `GameBootstrap` 注册自有 Module | 启动组合根3.1 完成 | 🔒 |

> 主线推到 **启动组合根3.2**，框架地基冻结（契约不再大改），支线可放心并行。

#### 7.2a 编译边界2.7 展开（Audio 解耦三步：先断依赖 → 再挪位置 → 后上 asmdef）

> **背景：** 当前 `WwiseAudioManager`（通用音频）与 `MusicSyncTool`（音游节拍）**循环依赖**——管理器的 `PlayCommonBgm/StopCommonBgm` 调节拍工具，节拍工具又反向读管理器的 `good/great/perfectWindow`。直接挪文件会让**框架反向依赖 GameKit**（违反 §6.4）。故必须先断依赖。唯一外部调用者是 `_TestSpace/SimpleTest.cs`（测试），业务影响极小。

| 子步 | 做什么 | 关键动作 | 验收 | 学习点 |
|------|--------|----------|------|--------|
| **编译边界2.7a** | 断循环依赖（文件不挪位置） | ① 判定窗口 `good/great/perfectWindow` 从 `WwiseAudioManager` 移入 `MusicSyncTool` 自持；② `PlayCommonBgm` 改纯播放、管理器**自存** `CurBgmPlayingId`，`StopCommonBgm` 停自己的 id，删除对 `MusicSyncTool` 的所有引用；③ 音游"带节拍同步播放"逻辑暂置 `MusicSyncTool.PlayBgmWithBeatSync`（内部调通用原语 `PlayWwiseEventWithCallback` + `ActiveMusicBeatSync`）；④ 改 `SimpleTest.cs` | 编译 + Play；依赖变单向 `MusicSyncTool → WwiseAudioManager` | 打破循环依赖、原语 vs 组合 |
| **编译边界2.7b** | 音游剥离到 GameKit | ① `MusicSyncTool` / `BeatEvtListData` / `Editor/DefaultRhythmMapGenerator` + 播放助手 迁 `CmgmGameKits/MusicGame/BeatSync/`（暂 `Assembly-CSharp`，asmdef 留 GameKits1.x）；② `Consts.Paths.RhythmMap_Path` 从 Core 迁到该包 | 编译 + Play；依赖 `MusicGame → CMGM.Audio` 单向合法 | 框架/工具包边界、`GameKit → Module` 合法方向 |
| **编译边界2.7c** ✅ | Audio 模块闭环（asmdef） | 瘦身后 Audio 加 `namespace CMGM.Audio` + `CMGM.Audio.asmdef`（references `CMGM.Core` + `AK.Wwise.Unity.API` + `AK.Wwise.Unity.API.WwiseTypes`；2.7a 已去 Odin 故无需引用；`Audio/Editor` 已空，无 `*.Editor` 子程序集）；调用方 `CmgmFrameBoot` / `MusicSyncTool` 补 `using CMGM.Audio;` | 编译 + Play | asmdef 第三方引用（Wwise 有 asmdef 故可引用，区别于 XLua） |

> 完成 2.7c 后：Audio = 只含通用音频的干净可选模块；音游节拍以 `MusicGame/BeatSync` 工具包独立存在（谁做音游谁勾）。

### 7.3 支线任务（功能，解锁后并行 / 按需）

| 支线系统 | 起步编号 | 解锁条件 | 状态 | 一句话 |
|----------|----------|----------|------|--------|
| **Loading系统** | Loading系统1.1 | Core ✅ + UI ✅ | **已解锁** | 通用加载服务：任意处可调、可选面板/后台、聚合多源进度 |
| **存档升级系统** | 存档升级系统1.1 | Data ✅ | **已解锁** | 版本头 + 分块 + 替换 `BinaryFormatter` + 迁移 |
| **Lua系统** | Lua系统1.1 | 编译边界2.6 完成 ✅ | **已解锁** | 桥接注册、懒加载、路径生成 |
| **Audio系统** | Audio系统1.1 | 编译边界2.7 完成 | 🔒 | 通用音频：Bank 加载策略、`IAudioService` 抽象（节拍/音游归 GameKit MusicGame） |
| **GameState系统** | GameState系统1.1 | 启动组合根3.1 完成 | 🔒 | 状态机基础态；接管 `GoToMainScene` / `QuitGame` |
| **事件总线系统** | 事件总线系统1.1 | 启动组合根3.1 完成 | 🔒 | `IEventBus` 落地 Optional 模块 |
| **依赖抽象系统** | 依赖抽象系统1.1 | 编译边界2.8 完成（程序集边界稳定后再解耦第三方） | 🔒 | 去 Odin 硬依赖、URP/RP 抽象（音频/Lua 抽象见各自支线） |
| **常量与配置体系** | 常量体系1.1 | 编译边界2.9 完成（程序集/Editor 边界稳定后再统一入口） | 🔒 | 统一常量入口；厘清 框架/游戏、Editor/runtime 常量归属（治理 `Consts` 与 `MusicGameConsts` 等分散） |
| **GameKits** | GameKits1.1 | 建议启动组合根3.1 后（按需） | 🔒 | RoleControl / MapTriggers / Camera 模板（§6.4） |
| **网游预埋** | 网游预埋1.1 | 存档升级系统 + GameState系统 完成 | 🔒（远期） | LocalSave/ServerSync、网络层、重放、Cloud save |

### 7.4 支线展开（各线内部线性）

#### Loading系统（已解锁）

定位：**通用加载服务**——任意位置可调用，可选「显示全屏面板 / 后台静默」，聚合多源进度。横切 UI / 资源 / 音频等，属编排层（不进 Core）。

| 子步 | 内容 | 学习点 |
|------|------|--------|
| **Loading系统1.1** | 模块骨架：`Modules/Loading`（`CMGM.Loading`）+ `LoadingManager.Run(tasks)` + 单条进度面板；跑通「执行一组任务并显示进度」 | 模块 asmdef、接口基础 |
| **Loading系统1.2** | `ILoadTask`（加载步骤抽象）+ 加权进度聚合（`权重 × 段内进度`，无内部进度的任务直接跳段） | 接口/多态、进度算法 |
| **Loading系统1.3** | 接通「进游戏」加载点：替代 MainPanel 直接 await，由 Loading 编排 `GameBootstrap` 回调（Loading **不**引用 Game） | 模块协作、回调注入 |
| **Loading系统1.4** | 加载点 Profile（每加载点一个 ScriptableObject 静态清单）+ 程序化动态补充任务 | 数据驱动、策划友好 |
| **Loading系统1.5+** | 后台静默加载、转场动画、动态拼任务（按敌人 ID 等） | 进阶 |

> **进度模型（1.2 起）：** 每个 `ILoadTask` 带 `Weight`；总进度 = `Σ(已完成权重) + 当前任务权重 × 当前任务内部进度`。Addressables/场景用真实 `PercentComplete`；Bank/Init 等无中间进度者完成即跳段（必要时加假进度补间防卡顿感）。显示推荐「单条 + 当前阶段文案」，不展示多条并行子进度。  
> **配置形态（1.4）：** 不做全局大表；**每个加载点一个 `.asset`（ScriptableObject）** 配静态资源，运行时按上下文（敌人 ID 等）程序化追加 `ILoadTask`。统一的是「执行器」，分散的是「清单」。

#### 存档升级系统（已解锁）

| 子步 | 内容 | 验收 |
|------|------|------|
| **存档升级系统1.1** | 存档版本头（magic + version + chunk 数量） | 旧档可识别 |
| **存档升级系统1.2** | 分块接口 `ISaveChunk` / chunk 注册表；每块独立序列化 | 游戏只增 Game 层 chunk |
| **存档升级系统1.3** | 替换 `BinaryFormatter`（JSON / MemoryPack / 自定义二进制择一） | 安全、可版本迁移 |
| **存档升级系统1.4** | 迁移管线 `ISaveMigrator`：vN → vN+1 | 样例迁移测试 |
| **存档升级系统1.5** | 运行时 API：`SaveSlot` / 异步写盘 / 校验 | 多存档槽正常 |
| **存档升级系统1.6** | 示例与文档：演示新增字段如何加 chunk | 策划 / 程序可查 |

#### Lua系统（已解锁）

| 子步 | 内容 | 验收 |
|------|------|------|
| **Lua系统1.1** | `ILuaBridgeRegistry`：游戏注册 `Talk` 等 API，框架不写死空实现（承接旧「依赖抽象·Lua 桥」） | Lua 调 C# 游戏逻辑不改框架源码 |
| **Lua系统1.2** | Lua 懒加载：按需 `require`，去掉启动期全量加载 | 大包体启动更快 |
| **Lua系统1.3** | 扫描 `HotRes/Lua` 生成路径常量（§2b **E**） | 路径不再手写字符串 |

#### Audio系统（🔒 编译边界2.7 后）

| 子步 | 内容 | 验收 |
|------|------|------|
| **Audio系统1.1** | `IAudioService` 包装 Wwise；Core / Modules 不直接引用 Wwise API（承接旧「依赖抽象·音频」） | 换音频后端只改 Extension |
| **Audio系统1.2** | Bank 加载 / 卸载策略：gameplay Bank 进游戏按需载、退出卸载 | gameplay Bank 不进启动链 |
| ~~Audio系统1.3~~ | **已移出本支线**：节拍 / 判定 / 谱面（MUG）归 GameKit `MusicGame`（编译边界2.7b 起 `BeatSync` 落地，后续 `Beatmap/Judgement` 见 §6.4a） | — |

#### GameState系统（🔒 启动组合根3.1 后）

| 子步 | 内容 | 验收 |
|------|------|------|
| **GameState系统1.1** | `IGameState`：`Enter` / `Exit` / `Update`（可选） | 基础态可切换 |
| **GameState系统1.2** | `GameStateMachine`：Push / Pop / Replace | 日志可追踪栈 |
| **GameState系统1.3** | 基础态 `Boot` / `MainMenu` / `Gameplay` / `Loading`（态内调 Loading系统 + `GameBootstrap.EnterGameplayAsync`）；接管原 `GoToMainScene` / `QuitGame` | 与 ScenesManager 协作 |
| **GameState系统1.4** | 预留态 `Pause` / `Cutscene` / `Battle` 空壳或最小实现 | JRPG / SRPG 可扩展 |
| **GameState系统1.5** | 与 UI / 输入：状态切换时 UI 层、输入 map 切换策略 | 暂停时输入正确 |

#### 事件总线系统（🔒 启动组合根3.1 后）

| 子步 | 内容 | 验收 |
|------|------|------|
| **事件总线系统1.1** | `IEventBus`：`Subscribe` / `Publish` / `Unsubscribe` + 线程/退订生命周期约定 | 规则文档 + 空实现可编译 |
| **事件总线系统1.2** | 落地 `OptionalSystem/EventSystem`；Bootstrap 注册单例 | 无全局静态散落 |
| **事件总线系统1.3** | 选 1~2 处 Manager 直连改事件（如场景切换完成） | 行为不变、解耦 |

#### 依赖抽象系统（🔒 编译边界2.8 后 / 按需）

> 承接旧「阶段7 依赖抽象」中**未并入其它支线**的部分。其中音频抽象 `IAudioService` 见 **Audio系统1.1**、Lua 桥抽象 `ILuaBridgeRegistry` 见 **Lua系统1.1**；此处只放渲染/编辑器第三方解耦。

| 子步 | 内容 | 验收 |
|------|------|------|
| **依赖抽象系统1.1** | Odin 降级：框架 asmdef 去掉 Odin 硬依赖，或 `#if ODIN_INSPECTOR` 条件编译 | 无 Odin 也能编 Core/Modules |
| **依赖抽象系统1.2** | URP / RP 抽象：`IRenderPipeline` 薄封装或「换 RP 检查清单」文档 | 换渲染管线有章可循 |

#### 内容扩展（按需，多数依赖对应系统）

| 子步 | 内容 | 验收 |
|------|------|------|
| **内容扩展1.1** | 对话系统：Lua / 配表驱动对话 UI 与分支 | 样例对话可跑 |
| **内容扩展1.2** | 场景持久化：进出场景对象 Save/Load 钩子（依赖完整 Scene 模块） | 进出场景状态保留 |
| **内容扩展1.3** | 配表类型扩展：多键表、嵌套结构、本地化列 | ExcelTool 支持 |
| **内容扩展1.4** | SRPG 接口：网格 / 回合 / 技能预留（与 GameState Battle 衔接） | 与 Battle 态衔接 |
| **内容扩展1.5** | Editor 模块导入向导：勾选 `Framework/Modules/*`，输出依赖报告 + 缺失 asmdef 提示（依赖 启动组合根3.2 Manifest） | 复制到新工程可裁剪 |

> **去向说明：** 旧 8.4「Lua 懒加载」已移至 **Lua系统1.2**；旧 8.5「MUG」节拍部分改入 GameKit **`MusicGame/BeatSync`**（编译边界2.7b 起），Audio系统支线只保留通用音频（Bank / `IAudioService`）。

#### GameKits（🔒 建议启动组合根3.1 后，低优先级）

| 子步 | 内容 | 验收 |
|------|------|------|
| **GameKits1.1** | 目录与 asmdef：`Scripts/CmgmGameKits/`；`CMGM.GameKits`；`namespace CMGM.GameKits` | 引用 Framework Modules 编译通过 |
| **GameKits1.2** | RoleControl：2D / 3D 通用角色控制器模板 | 示例场景可跑 |
| **GameKits1.3** | MapTriggers：可继承的场景触发器基类 + 常用变体 | 与 `ScenesManager` 切场景无耦合 |
| **GameKits1.4** | Camera：跟随 / 边界等相机控制（按需） | 可选 |
| **GameKits1.5** | 发布：与框架同仓库或同 UPM 包组；新项目可整包删除 | 文档 §6.4 |
| **GameKits1.6** | MusicGame：音游工具包（`BeatSync` 已由编译边界2.7b 落地为 `Assembly-CSharp` 代码；本步将其纳入 `CMGM.GameKits` asmdef + namespace，并按 §6.4a 扩展 `Beatmap/Judgement/Note/AudioVisualization/RhythmInput`） | 节拍判定可跑；与 Audio 模块单向依赖 |

#### 网游预埋（🔒 远期，存档升级 + GameState 完成后）

| 子步 | 内容 | 验收 |
|------|------|------|
| **网游预埋1.1** | 存档分层：LocalSave vs ServerSync 接口分离 | 单机不受影响 |
| **网游预埋1.2** | 网络层：连接 / 心跳 / 消息编解码占位 | 可 mock 服务器 |
| **网游预埋1.3** | 战斗重放：输入序列 + 确定性 tick 记录 | 回放一致 |
| **网游预埋1.4** | Cloud save：与存档格式兼容的上传 / 合并策略 | 文档 + 伪代码 |

#### 常量与配置体系（🔒 编译边界2.9 后）

> **动因（2026-06-19 用户提出）：** 常量入口分散（`Core.Consts.Paths` 与 `MusicGameConsts` 等），且未厘清「框架 vs 游戏」「Editor vs runtime」的归属。目标：统一入口、明确新增常量的落点规则，让常量用起来不再到处找。

| 子步 | 内容 | 验收 |
|------|------|------|
| **常量体系1.1** | 盘点与分类：列出现有常量（`Consts.Paths` / `MusicGameConsts` / 各处散落字符串），按「框架 vs 游戏」「Core / Module / GameKit」「Editor vs runtime」三维归类 | 产出分类清单 |
| **常量体系1.2** | 入口与归属规范：定义框架常量入口（`Core.Consts`）与模块/包私有常量的边界；Game 层新增常量的归属（如 `Game.Consts`）；减少散落 | 新增常量有唯一规范落点 |
| **常量体系1.3** | Editor / runtime 区分：runtime 路径与 Editor-only 路径（导表 / 生成器输出等）分离，配合 asmdef Editor 边界 | runtime 程序集不含 Editor-only 常量 |
| **常量体系1.4**（按需） | 路径自动生成 / 校验：扫描 `HotRes` 生成路径常量、缺失目录检查（与 Lua系统1.3 / 内容扩展 **E** 衔接，避免重复造轮子） | 路径不手写、缺失可报 |

### 7.5 跨线解锁关系 + 设计决策

**跨线依赖（绝大多数是「支线依赖主线」，反向极少）：**

| 任务 | 依赖方向 | 说明 |
|------|----------|------|
| GameState系统 | 依赖主线「启动组合根3.1」 | 要接管 Scene 流程方法，须等 `IGameModule` 契约定好 |
| Loading系统1.3 | 软依赖主线「启动组合根3.1」 | 接通进游戏会调各 Manager；若契约改动可能小幅返工（风险低） |
| 网游预埋 | 依赖**支线**（存档升级 + GameState） | 「远期主线 / 收尾任务依赖支线完成」的典型例子 |

**设计决策记录 · 2026-06-19（Loading / Scene 重定位）**

| 决策 | 结论 |
|------|------|
| **Scene 不再独立 asmdef** | 场景加载原语 `LoadSceneAsync` **下沉 Core**（`AddressablesResMgr`，与 `LoadAssetAsync` 并列，纯资源原语，不碰 UI）；`CMGM.Scene.asmdef` **已删除**，`ScenesManager` 回默认 `Assembly-CSharp`；`GoToMainScene` / `QuitGame` 待 GameState系统 接管；命名 `Scene` 保留为目录/namespace。若未来需 Additive / 流式 / 持久化 / 转场，再扩为完整 Scene 模块（YAGNI）。 |
| **Loading 复活为独立模块** | `Modules/Loading`（`CMGM.Loading`）作为通用加载服务，分步迭代（见 §7.4 Loading系统1.1~1.5+）。之前「Loading 不单独建 Modules」的延后结论就此推翻。 |

> 旧的「Level→Scene 重命名 + `CMGM.Scene` 闭环」程序集部分已回退；相关旧编号映射见 `ARCHITECTURE_DEPRECATED.md`。

**设计决策记录 · 2026-06-19（XLua / Lua 模块定位，编译边界2.6）**

| 项 | 结论 |
|------|------|
| **背景** | 此前接入的是 XLua 官方 `feature/asmdef` 分支（`Xlua.Core.asmdef`），但该分支在官方已被 **Revert**（PR#1067 加入、PR#1068/commit d919198 撤销）。原因非运行时不稳定，而是「Gen 代码须与核心同程序集」「hotfix 须核心在 `Assembly-CSharp`」两条约束与 asmdef 冲突，官方放弃维护（Issue #1174 至今 open）。 |
| **结论：方案 C** | Lua **不单独建 asmdef**。① XLua 退回官方 **master**（无 asmdef，回 `Assembly-CSharp`）；② 契约 `ILuaService` 放 `CMGM.Core/Services/`（仅暴露 XLua 无关成员，不含 `LuaEnv`）；③ 实现 `LuaManager`/`LuaBridge` 迁 `Framework/Integrations/Lua/`，随 XLua 落 `Assembly-CSharp`，`LuaManager` 实现 `ILuaService`；④ 服务**注册/注入**留待主线「启动组合根3.1」随 `IGameModule` 体系接入——当前框架仍是 `Singleton.Instance` 模式，Boot 直接调 `LuaManager.Instance`；⑤ `CMGM.Lua` / `CMGM.Lua.Editor` asmdef 已随 XLua 回退一并消失，无需另删。 |
| **为何不放 GameKit** | 按**职责**分类：Lua 是**框架基础设施**（所有用 Lua 的项目都要），非业务玩法工具；GameKit 专放业务层小工具（角色控制器 / 触发器 / 相机）。故另设 `Integrations/`（框架级第三方桥接，因第三方约束不能封装、随库落 `Assembly-CSharp`），与 `CmgmFrameBoot` 同类。 |
| **取舍** | 放弃「Lua 独立 asmdef 封装包」的强制边界（现阶段几乎用不上：依赖方向多为 Boot→Lua、Lua→业务），换回**官方主线可升级 + hotfix 之门重开 + wrap 生成走 happy-path**，消除「绑死回滚版本」长期风险。被封装 Module 仍通过 `ILuaService` 契约保持分层与可迁移性。 |
| **正交提醒** | XLua **交互模式**（反射 codeless ↔ 生成 wrap）与程序集归属无关：反射模式的性能/GC、IL2CPP 裁剪问题，**出包前**仍需以 `link.xml` / `[ReflectionUse]` 或生成 wrap 处理；本决策与之独立。 |
| **hotfix** | 维持**关闭**；核心回 `Assembly-CSharp` 后门已重开，真要 C# 级热更（远期网游化）时在 XLua Hotfix / HybridCLR 间再评估（单机 JRPG/SRPG 阶段不需要）。 |

> 旧「编译边界2.6 = `CMGM.Lua` asmdef + XLua Gen 同单」及相关 asmdef 文件已废弃，迁入 `ARCHITECTURE_DEPRECATED.md`。

**设计决策记录 · 2026-06-19（编译边界章节重排 + Audio 解耦 + MusicGame 工具包）**

| 项 | 结论 |
|------|------|
| **编译边界拆细** | 原 2.7（Audio+Input）+ 2.8（Editor）重排为 **2.7 Audio / 2.8 Input / 2.9 Editor**；`启动组合根3.1` 解锁条件顺延至「2.9 完成」。 |
| **2.7 再拆三步** | `2.7a 断循环依赖 → 2.7b 音游剥离 → 2.7c Audio 上 asmdef`（见 §7.2a），遵循「先断依赖、再挪位置、后 asmdef」原则。 |
| **Audio 解耦动因** | `WwiseAudioManager`（通用）与 `MusicSyncTool`（音游）循环依赖；判定窗口误置于通用管理器（原作者注释已自承）。必须先断依赖，否则剥离会导致框架反向依赖 GameKit（违反 §6.4）。 |
| **音游归属** | 节拍/判定/谱面等音游能力 = **GameKit `CmgmGameKits/MusicGame`**（非框架 Audio 模块、非 Audio系统支线）。MusicGame 下按子模块分层（§6.4a：`BeatSync`(now)/`Beatmap`/`Judgement`/`Note`/`AudioVisualization`/`RhythmInput`）。Audio系统支线只保留通用音频（Bank / `IAudioService`），原 `Audio系统1.3 MUG` 移出。 |
| **RhythmMap_Path** | 从 `Core/Consts.Paths` 迁至 MusicGame 包（2.7b），Core 不再持音游路径。 |
| **Input 待办（2.8）** | Input 当前干净（仅 `InputManager`）；2.8 需复检 `InputActions_Main` 无音游专属 action map，并处理 `InputManager → CMGM.UI` 跨模块依赖。 |
| **「Input系统」支线** | **暂不立支线**（YAGNI）；音游输入归 `MusicGame/RhythmInput`。列入下方「潜在完善候选」，待框架成熟后主动提醒。 |

**潜在完善候选（backlog，未立支线；当用户问「还能怎样进一步完善框架」时主动提醒）**

| 候选 | 方向 | 触发时机 |
|------|------|----------|
| **Input系统拓展** | 键位重绑定、键鼠/手柄/触屏设备切换、输入缓冲、`InputManager→UI` 解耦（走事件总线） | 主线/主要支线收尾、框架趋于稳定时 |

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

- **主界面 / 主场景 ✅**：`CmgmFrameSettings.MAIN_PANEL_NAME`、`MAIN_SCENE_NAME`；`ScenesManager.GoToMainScene` 统一调用。游戏内其他 Panel 仍优先 `ShowPanel<T>()`。
- **D（AssetAddresses）**：Settings 字符串已够用；Address 键集中管理留待后续按需做。

### 资源加载分层（主界面 vs 进游戏）

| 阶段 | 负责 | 应加载 | 不应加载（示例） |
|------|------|--------|------------------|
| **Logo → 主界面** | `CmgmFrameBoot` + `ScenesManager.GoToMainScene` | 框架 Manager Init、主 Panel、主场景（轻量）、UI 包 | 角色配表、关卡场景、Wwise gameplay Bank |
| **主界面 → 进游戏** | **Loading系统** + `GameBootstrap.EnterGameplayAsync` | `LoadTable`、关卡 Addressables、音频 Bank、Gameplay 场景 | — |
| **运行时懒加载** | `GetTable` / Addressables 按需 | 非关键表、可选资源 | 已在 Loading 阶段声明的必需项 |

- `ConfigTableManager.GetTable<T>()` 仍保留懒加载兜底，但**进游戏必需表**应在 Loading 阶段显式 `LoadTable`。
- 换项目时在 `GameBootstrap.EnterGameplayAsync` 维护「进游戏加载清单」。

### 框架 / 游戏层命名（已确立）

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

- **本文档（`ARCHITECTURE.md`）只记当前有效计划。** 内容废弃时，迁入 `ARCHITECTURE_DEPRECATED.md` 并标注**废弃日期 + 时分（UTC+8）**。
- 完成主线/支线某步后，更新 **§7.2 / §7.3 / §7.4** 对应行的**状态**，并在 **§7.1 已完成基线**补一句浓缩记录。
- 重大架构决策记录在 **§7.5 设计决策**（含日期）。
- 模块结构变化同步更新 **§3 模块清单**、**§5 耦合点**、**§6 目标架构**。
- 报告「下一步」时按偏好：列**所有主线 + 支线**一张表，每条线只列**最前节点**；未解锁项标解锁条件，可做项给步骤编号 + 预计变更量 + 教学难度。

---

*下一步候选：主线 **编译边界2.6**（Lua 模块闭环）或支线 **Loading系统1.1**（模块骨架）。详见 §7。*
