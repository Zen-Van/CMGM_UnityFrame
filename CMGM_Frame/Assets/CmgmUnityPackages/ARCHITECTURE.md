# CMGM 框架架构说明

> 本文档是框架化改造的长期参考（「北极星」）。  
> 目标：将当前工程从「带 Demo 的原型项目」逐步改造为「可跨项目迁移的框架」。  
> 最后更新：2026-06-19 — **当前聚焦 §7.7 启动竖切大表**（Loading / GameFlow / Editor / 3.4）；完成前**暂停**其它支线新功能。§6.5b 增补 **Loading 调用纪律**（方案 A，不采用 `Loading` 宏观态）。

---

## 1. 项目定位

| 维度 | 说明 |
|------|------|
| 游戏类型 | 单机独立，主线 20~50 小时；JRPG / SRPG / MUG 等 |
| 当前状态 | 约 30% 完成度的基础运行时 + 工具链骨架 |
| 远期目标 | 框架可插拔、业务层可替换；未来网游扩展时不推倒重来 |

**一句话：** C# 管引擎 / UI / 资源，Lua 管剧情 / 关卡 / 事件；Excel 管数值配表。

---

## 2. 目录结构

### 2.1 当前（**项目脚手架1.4** ✅）

```
Assets/
├── CmgmUnityPackages/              随框架复制/更新；框架仓改动贴回此 subtree
│   ├── CmgmFramework/
│   │   ├── Resources/              Settings、UI、Logo、字体
│   │   ├── Editor/                 框架 Editor、脚手架
│   │   └── Runtime/                运行时代码 ✅
│   │       ├── CmgmInitializer.cs    InitScene 薄组合根（**启动编排3.4** 自 Bootstrap/ 迁入）
│   │       ├── Core/
│   │       ├── Modules/
│   │       └── Integrations/
│   └── CmgmGameKits/
├── _WorkSpace/                     **业务层**：脚本、HotRes、Excels（无 Resources）
├── _TestSpace/                     **测试层**
└── …（第三方）

Assets/_WorkSpace/                  （业务层 · Workspace）
├── GAME_WORKSPACE.md
├── Excels/
├── HotRes/
│   ├── Lua/
│   ├── Scenes/
│   ├── UI/Panels/
│   └── RhythmMap/                  Demo 专有
└── Scripts/
    ├── Bootstrap/
    ├── UI/Panels/
    ├── Archive/
    └── _Generated/
        └── Config/

Assets/_TestSpace/                  （测试层；脚手架仅建顶层空目录，子目录用户自建）
```

> **三层划分：** `CmgmUnityPackages` = 框架与拓展包（含 **Resources 内置**）；`_WorkSpace` = 业务层增量；`_TestSpace` = 测试层。  
> **CmgmFramework 三分：** `Resources/` + `Editor/` + `Runtime/`。**业务层最简模板**由脚手架 **1.2b ✅** 写入 `_WorkSpace`。

### 2.2 远期（按需）

```
Packages/（项目脚手架1.6 远期 UPM）
  com.cmgm.core/
  com.cmgm.modules.*/
```

### 路径常量入口

所有路径由 **`Consts.Paths`**（`CmgmUnityPackages/CmgmFramework/Runtime/Core/Consts.Paths.cs`）统一定义：

- **`WorkSpace`** — 业务层（Workspace）根（`CmgmFrameSettings.WORK_SPACE_ROOT`，默认 `Assets/_WorkSpace`）
- **`TestSpace`** — 测试层根（`Assets/_TestSpace`）
- **`ScriptsPath`** / **`TestScriptsPath`** — 各层脚本根
- **共享**：`HotRes`、`ARCHIVE_PATH`、`ConfigData` 等
- **`Paths.Package.*`** — `CmgmUnityPackages` 包根（`Framework`、`GameKits`）
- **`Paths.Framework.*`** — 框架目录（`Resources`、`Editor`、**`Runtime`**、`Core`、`Modules`…）
- **`Paths.WorkSpaceScripts.*`** — 业务层脚本子目录
- **`Paths.Framework.DataModule.*`** — 框架 Data 模块（`Archive`、`Config`）

> **文档分工：** `ARCHITECTURE.md` 随**框架**（将来随 CmgmFramework 或框架仓库）；`GAME_WORKSPACE.md` 随**每个游戏项目**的 `_WorkSpace`。

换项目时：改 Settings 里的根路径即可，不必改代码里的字符串。

---

## 2b. 路径与资源引用优化线

除主线/支线任务外，路径/地址相关改进按下列步骤穿插推进：

| 代号 | 内容 | 计划归属 | 状态 |
|------|------|----------|------|
| **A** | 合并 `Consts` 为单文件，去掉 partial | 已完成基线 | ✅ |
| **C** | `WorkSpace` 根路径迁入 `CmgmFrameSettings` | 已完成基线 | ✅ |
| **B** | 拆 `Consts.Paths.Framework` 与 `Paths.WorkSpaceScripts`（原 `.Game`） | 已完成基线 | ✅ |
| **D** | Addressables 加载键独立为 `AssetAddresses` 或 Label 分组 | **Loading系统1.4** 同期（集中各加载点 Address 键）/ 按需 | 待做 |
| **F** | 关键 Prefab/SO 改用 `AssetReference`，减少字符串路径 | **Loading系统1.4** 同期（加载清单 `AssetReference` 化）/ 按需 | 按需 |
| **E** | 扫描 `HotRes/` 自动生成路径常量（代码生成） | Lua 部分见 **Lua系统1.3**；全量 `HotRes` 扫描属**内容扩展支线** / 远期 | 远期 |

---

## 3. 模块清单

### 3.1 核心模块（`Runtime/Core/`，`CMGM.Core`）

| 组件 | 文件 | 职责 | 成熟度 |
|------|------|------|--------|
| 单例基类 | `Singleton/` | **3.2 ✅** `LazySingleton` / `BootSingleton`；Mono 基类留 **Audio系统** | ★★★ |
| 资源管理 | `ResourceManagement/AddressablesResMgr` | AB 加载、引用计数、防重复、预加载、**场景加载原语 `LoadSceneAsync`** | ★★★★ |
| 对象池 | `ResourceManagement/ObjectPool/` | 基础对象池 | ★★ |
| 帧设置 | `CmgmFrameSettings` | LOG、Lua 热重载、**工作区根路径**、根脚本路径 | ★★★ |
| 路径常量 | `Consts.Paths` | 由 Settings 派生的目录与管线路径 | ★★★ |
| 日志 | `CoreUtils/CmgmLog` | 分级日志；Error 不受 LOG 开关影响 | ★★★ |
| Mono 调度 | `MonoManager/` | Update 委托挂载 | ★★ |

### 3.2 UI（`Runtime/Modules/UI/`，`CMGM.UI`）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `UIManager` | 分层 Canvas、异步加载 Panel、Hide 中途取消容错 | ★★★★ |
| `BasePanel` | Panel 基类 | ★★★ |
| `Panels/MainPanel`、`LoadingPanel` 等 | `_WorkSpace/Scripts/UI/Panels/` ✅ | — |
| Editor 工具 | Panel 模板创建、快速搜索 | ★★★ |

### 3.3 数据（`Runtime/Modules/Data/`，`CMGM.Data`）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `ConfigTableManager` | 读取 `.cmgm` 二进制配表（反射 + 解密） | ★★★★ |
| `ExcelTool`（Editor） | Excel → Container.cs + 二进制 | ★★★★ |
| `RoleInfoContainer` 等 | 游戏配表（`_WorkSpace/Scripts/_Generated/Config/`）✅ | — |
| `ArchiveManager` | 存档元数据 + 运行时数据读写 | ★★★ |
| `GameRuntimeData` | 游戏存档结构（`_WorkSpace/Scripts/Archive/`）✅ | — |
| `CipherTool` | 配表 / 存档加解密 | ★★★ |

### 3.4 Lua（契约 `Runtime/Core/ILuaService`；实现 `Runtime/Integrations/Lua/`）

| 组件 | 职责 | 程序集 | 成熟度 |
|------|------|--------|--------|
| `ILuaService` | Lua 服务契约（执行脚本 / 桥接注册等） | `CMGM.Core`（asmdef） | 已实现 ✅ |
| `LuaManager` | LuaEnv 生命周期、Loader 链、脚本执行；实现 `ILuaService` | `Assembly-CSharp` | ★★★ |
| `LuaBridge` | C# ↔ Lua 桥接（Talk / Wait / DebugLog）；保留全局 namespace（`main.lua` 调 `CS.LuaBridge`） | `Assembly-CSharp` | ★★ |

> **现状（2026-06-19 决策，见 §7.5）：** Lua **不单独建 asmdef**。XLua 退回官方 master（无 asmdef、待在 `Assembly-CSharp`）；契约 `ILuaService` 放 `CMGM.Core`，实现放 `Runtime/Integrations/Lua/`；`LuaManager.InitAsync()` 在 **Loading `StartupFramework` Profile** 中执行（§6.5）。被 asmdef 封装的 Module 只依赖契约，不碰 XLua（依赖倒置）。

**Lua 加载策略：**

| 模式 | 预加载 | require / ExecuteLua |
|------|--------|----------------------|
| 热重载（Editor） | 跳过 `LoadLuaMapper` | Loader 1 / `GetLuaContent` 直读磁盘 |
| 正式包体 | `LoadLuaMapper` → `luaMapper` | Loader 3 查内存字典 |

### 3.5 启动三分工（`CmgmInitializer` + **Loading** + **GameFlow**，见 §6.5）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| **`CmgmInitializer`** | InitScene **薄组合根**：Logo + **保证能显示东西**（至少 `UIManager.InitAsync`）→ 交 **GameFlow** | ★（**3.4 待做**；运行时过渡仍为 `CmgmFrameBoot`） |
| **`LoadingManager`** | **一切可编排异步准备**：`ILoadTask`（Manager Init / 资源 / 场景 / 业务回调）+ Profile + 可选进度 UI | ☆（支线 Loading系统） |
| **`GameFlow`** | **何时跑哪份 Loading Profile**；宏观态切换（`Startup` / `MainMenu` / `Gameplay` / …）；**Editor** 下 `DirectToTest`（见 **Editor测试系统**） | ☆（支线 GameFlow系统） |

> **过渡（3.3 ✅）：** 业务层仍有 **`GameBootstrap.cs`** 与 `MainPanel` 直调；**Loading系统1.4b** 废止该文件，清单并入 **`EnterGameplay` LoadingProfile**（§7.4）。  
> **目标位置：** `CmgmFramework/Runtime/CmgmInitializer.cs`（**无**独立 `Bootstrap/` 目录、**无**单独 namespace；`Assembly-CSharp` 组合根）。  
> **设计决策（2026-06-19 修订）：** 废止「双 Boot 组合根」目标形态。除 **`CmgmInitializer`** 外，原 `CmgmFrameBoot` 内 Init / 加载列表 **全部迁入 Loading Profile**；**`GameBootstrap.cs` 计划废止**（清单 = **`EnterGameplay` Profile SO**，见 **Loading系统1.4b**）。**`BootSingleton` 仍保留**（生命周期 guard），但 **`InitAsync` 只允许**在 `CmgmInitializer`（最小集）或 **Loading 任务**中调用。

### 3.6 场景加载与流程（**不建 Scene 模块**；`ScenesManager` 为临时宿主）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `AddressablesResMgr.LoadSceneAsync` | **Core 资源原语**（场景 Addressables 加载） | ★★★★ |
| `ScenesManager` | **临时**流程胶水：`GoToMainScene` / `QuitGame`、切场景后 UI 摄像机叠加 | ★★ |

> **决策（§7.5，YAGNI）：** **不**把 Scene 当作框架可选 Module 维护（无 `CMGM.Scene` asmdef、无 Scene 模块支线）。  
> - **已下沉 Core：** `LoadSceneAsync`（与 `LoadAssetAsync` 并列，纯资源原语）。  
> - **`ScenesManager` 为何还在：** 流程职责（回主界面、清 UI/存档、Quit）尚未迁入 **GameFlow系统**；当前仅为过渡代码，**GameFlow系统1.3 接管后应缩退或删除**，而非扩成完整 Scene 模块。  
> - **进游戏 Loading：** 由 **Loading系统** 编排，不绑 Scene；**谁有权触发、何时 `SwitchTo`** 见 **§6.5b**。  
> - **按需再建：** 仅当项目需要 Additive 多场景 / 流式分块 / 场景持久化 / 转场动画等，再评估是否新增 Scene 能力（届时可能落在 GameFlow / Loading / 业务层，而非预建 `Modules/Scene/`）。

### 3.7 可选模块

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `WwiseAudioManager` | Wwise 通用音频（Bank / 播放 / 音量）；**编译边界2.7a 起去节拍化** | ★★★ |
| ~~`MusicSyncTool`~~ | 编译边界2.7b 已从 Audio 剥离至 `CmgmUnityPackages/CmgmGameKits/`（目录暂存，非 GameKits 正式化） | — |
| `InputManager` | Input System 封装（GamePlay / UI 两套 action map）；**编译边界2.8** 上 `CMGM.Input`；Cancel→HidePanel 已迁至 `UIManager.Init`（UI→Input 单向） | ★★ |
| `OptionalSystem/EventSystem` | **未实现**（仅 .meta；支线「事件总线系统」落地） | ☆ |
| `OptionalSystem/CommandSystem` | **未实现**（仅 .meta） | ☆ |

---

## 4. 启动流程

### 4.1 目标形态（**启动编排3.4** + Loading / GameFlow 落地后）

```
InitScene（CmgmInitializer.Awake）
    │
    ├─ Logo（Video / Texture）
    ├─ await UIManager.InitAsync()          ← 唯一必须在 Initializer 内完成的 Manager
    │
    └─ GameFlow.Start() / Startup 态
            │
            └─ LoadingManager.Run(StartupFrameworkProfile, silent: true)   ← 可与 Logo 并行
                    ├─ ManagerInitTask: ArchiveManager, LuaManager, …
                    ├─ AssetPreloadTask: 主场景 / UI 包
                    ├─ Wwise 壳子 Init（gameplay Bank 不在此）
                    └─ GoToMainSceneTask（或 GameFlow → MainMenu.Enter）
            │
            └─ GameFlow.MainMenu          ← 正式包体 / 非 Editor 测试路径

主界面 → 进游戏：
    GameFlow.SwitchTo(Gameplay)          ← 宏观态切换；**不**单独 `SwitchTo(Loading)`（§6.5b）
        └─ GameplayState.Enter()
                └─ await LoadingManager.Run(EnterGameplayProfile, showProgress: true)
                        ├─ LoadTable / 关卡 Addressables / Bank / Gameplay 场景 …（**Profile SO 清单**，无 `GameBootstrap.cs`）
                └─ Enter 收尾（输入 map、HUD 等）
```

### 4.3 Editor 测试启动（**Editor测试系统** + **GameFlow.DirectToTest**）

> **目标：** 在 Editor 中从**任意已打开场景** Play，仍先走 **`CmgmInitializer`**，但**不强制** `GoToMainScene` / MainMenu；Startup 结束后 **直接进入 Editor 启动场景**。

```
Editor：用户打开 TestScene.unity → 菜单「从当前场景 Play」/ Play Mode Start Scene = InitScene
    │
    ├─ EditorPlayRequest 写入：TargetScenePath、SkipLogo、StartupProfile 裁剪、可选追加 Profile
    │
InitScene（CmgmInitializer）
    ├─ await UIManager.InitAsync()
    └─ GameFlow.Start()  ── Editor 分支 ──► DirectToTest（或 Startup 态内检测 EditorPlayRequest）
            │
            └─ Run(StartupFrameworkProfile, silent, editorTrim: true)   ← 可跳过 GoToMainScene 任务
            │
            └─ LoadSceneAsync(EditorPlayRequest.TargetScenePath)        ← **不**加载 MainScene
            │
            └─ （可选）Run(EnterGameplay / EnterBattle / 场景内 TestSceneEntry 声明的 Profile)
```

| 项 | 结论 |
|----|------|
| **能否跳过 MainScene** | **可以**；`StartupFramework` 在 Editor 测试路径**不包含** `GoToMainScene` / `MAIN_SCENE` 预载（或整 Profile 换用 **`StartupFrameworkEditorTrim`**） |
| **落点场景** | **`EditorPlayRequest.TargetScenePath`** = 点 Play 前 **Editor 当前打开场景** 的路径 |
| **进场景后再 Loading** | **支持**；由场景上可选 **`TestSceneEntry`**（`_TestSpace` / 业务层）或菜单勾选「追加 EnterGameplay Profile」在 `LoadSceneAsync` **之后**再 `Run` |
| **与正式路径关系** | 共用 Initializer + Loading + GameFlow；仅 **GameFlow 分支**与 Profile 裁剪不同 |

### 4.2 当前运行时（过渡 · **启动编排3.3 ✅**）

```
InitScene（CmgmFrameBoot.Awake）          ← 待 3.4 更名为 CmgmInitializer 并瘦身
    │
    ├─ 显示 Logo（Video / Texture）
    │
    └─ InitGame()  ── UniTask 并行 ──┐
                                      │
        ├─ PreloadAssetsAsync(MAIN_SCENE)
        ├─ await UIManager.InitAsync()
        ├─ await ArchiveManager.InitAsync()
        ├─ await LuaManager.InitAsync()
        ├─ WwiseAudioManager.Init()
        │
        └─ ScenesManager.GoToMainScene()

主界面 → 进游戏：
    MainPanel 直接 await GameBootstrap.EnterGameplayAsync()   ← Loading系统1.3 待接通
```

> **资源分层（§8）**：Logo→主界面尽量轻；角色表、关卡、gameplay Bank 在 **EnterGameplay Profile**。  
> **收敛项（3.4 / Loading 1.4）：** 上段长 Init 链迁入 `StartupFrameworkProfile`；`CmgmFrameBoot` 仅保留 Logo + `UIManager`。

### 已知生命周期问题（启动编排3.2 已解决 · 纯 C#）

- ✅ 纯 C# 单例已拆 **`LazySingleton` / `BootSingleton`**；旧 `Singleton<T>` 已移除
- ✅ Boot 型：`UIManager` / `ArchiveManager` / `LuaManager` → **`await InitAsync()`**
- **Mono 单例**（`WwiseAudioManager`、`InputManager` 等）：**Audio系统支线**再定

---

## 5. 耦合点（框架化的主要障碍）

以下代码属于**业务层**，但目前放在框架 Scripts 中，迁移新项目时必须改框架源码：

| 耦合点 | 位置 | 问题 |
|--------|------|------|
| 主界面 Panel | `CmgmFrameSettings.MAIN_PANEL_NAME` → `ShowPanel(name)` ✅ | 换项目改 Settings |
| 主场景名 | `CmgmFrameSettings.MAIN_SCENE_NAME` ✅ | 换场景改 Settings |
| 存档数据结构 | `GameRuntimeData`（博物、任务、背包…） | 游戏专属字段 |
| 配表容器 | `RoleInfo` 等（`_WorkSpace/Scripts/_Generated/Config/`）✅ | 游戏专属表结构 |
| Lua 桥接 | `LuaBridge.Talk` | 空实现，且写死在框架里 |
| 第三方绑定 | Wwise / Odin / URP 直接引用 | 换音频 / RP 需改 Core |

### 其他技术债

| 问题 | 位置 | 计划归属 |
|------|------|----------|
| namespace / asmdef | Core/UI/Data/Audio/Input/Editor 已闭环；Lua / `CmgmInitializer`（过渡 `CmgmFrameBoot`）/ Scene 临时宿主 无 asmdef | §7.2 ✅ |
| ~~`BinaryFormatter` 序列化~~ | ~~`ArchiveManager`~~ | **存档格式优化1.2 ✅**（`JsonArchiveSerializer`） |
| 配表 payload 读写分散 | ~~`ExcelTool` / `ConfigTableManager`~~ | **存档格式优化1.2b ✅** |
| 无 GameFlow 状态机 | — | 支线「GameFlow系统」 |
| 无事件总线 | `OptionalSystem/` | 支线「事件总线系统」 |

---

## 6. 目标架构（三层）

```
┌─────────────────────────────────────────────────┐
│  YourGame.Runtime（每个项目独有）                  │
│  GameRuntimeData / 配表 Container / Panel /       │
│  EnterGameplay LoadingProfile + Workspace LoadTask 类           │
└───────────────────────┬─────────────────────────┘
                        │ 依赖
┌───────────────────────▼─────────────────────────┐
│  CMGM.Modules（可选框架模块）                      │
│  UI / Data / Lua / Audio / Input / Loading / GameFlow …      │
└───────────────────────┬─────────────────────────┘
                        │ 依赖
┌───────────────────────▼─────────────────────────┐
│  CMGM.Core（框架核心，跨项目复用）                  │
│  单例 / 资源 / LoadSceneAsync / 服务契约 …         │
└───────────────────────┬─────────────────────────┘
                        │ 组合（Core + 已选 Modules + Integrations）
┌───────────────────────▼─────────────────────────┐
│  CmgmInitializer（薄组合根；§6.5；Assembly-CSharp）  │
│  InitScene：Logo + UIManager → GameFlow            │
└─────────────────────────────────────────────────┘
```

### 包体目录（**项目脚手架1.4** ✅）

```
Assets/CmgmUnityPackages/
  CmgmFramework/
    Resources/
    Editor/
    Runtime/
      CmgmInitializer.cs          （目标；过渡：Bootstrap/CmgmFrameBoot.cs）
      Core/
      Modules/
      Integrations/
  CmgmGameKits/

Assets/_WorkSpace/
  GAME_WORKSPACE.md
  HotRes/  Excels/
  Scripts/
    Bootstrap/                    ← 过渡；Loading 1.4b 后改 LoadingProfiles/ 或废止
    UI/Panels/
    Archive/
    _Generated/
      Config/

Packages/（远期，见 项目脚手架1.6）
  com.cmgm.core/
  com.cmgm.modules.*/
```

<details>
<summary>搬迁前布局（归档，1.4 前）</summary>

```
Assets/_WorkSpace/Scripts/
  Framework/ …                    （1.4 前，已迁至 CmgmUnityPackages）
  CmgmGameKits/ …                 （1.4 前）
  Game/ …                         （已取消，脚本提升至 Scripts/ 直下）
```

</details>

### 6.1 框架模块分级（Core / 可选 Modules）

| 层级 | 目录 | 模块 id | 默认 | 说明 |
|------|------|---------|------|------|
| **Core** | `CmgmFramework/Runtime/Core/` | `Core` | **必选** | `CMGM.Core` asmdef ✅ |
| **Modules** | `CmgmFramework/Runtime/Modules/UI/` | `UI` | 推荐 | `CMGM.UI` ✅ |
| **Modules** | `…/Data/` | `Data` | 推荐 | `CMGM.Data` ✅ |
| **—** | `Modules/Scene/` | — | **不建模块** | **临时** `ScenesManager`（`Assembly-CSharp`）；流程待 GameFlow 接管（§3.6） |
| **Modules** | `…/Loading/` | `Loading` | 可选 | 支线「Loading系统」；`CMGM.Loading` |
| **Modules** | `…/GameFlow/` | `GameFlow` | 推荐 | 支线「GameFlow系统」；`CMGM.GameFlow`；触发 Loading Profile |
| **Integrations** | `CmgmFramework/Runtime/Integrations/Lua/` | `Lua` | 可选 | **不建 asmdef**，契约 `ILuaService` 入 Core（§7.5） |
| **Modules** | `…/Audio/` | `Audio` | 可选 | `CMGM.Audio`（编译边界2.7） |
| **Modules** | `…/Input/` | `Input` | 可选 | `CMGM.Input`（编译边界2.8） |
| **Modules** | `…/Optional/` | `Optional` | 可选 | 原 `OptionalSystem/`；事件总线等 |
| **Modules** | `…/Utils/` | `Utils` | 按需 | 通用工具 |

**跨项目导入（后续迭代）：**

| 归属 | 内容 |
|------|------|
| **已完成基线** | 物理目录 + 去 `Game*` 前缀；UI / Data / Audio / Input / Editor asmdef 闭环 |
| **主线 编译边界2.6~2.9** ✅ | Lua（Integrations，**无** asmdef）/ Audio / Input / Editor 有 asmdef；`CmgmInitializer` / Scene 临时宿主 / Integrations 无 asmdef |
| **主线 启动编排3.3** ✅ | `CmgmFrameBoot` → `Runtime/Bootstrap/`（过渡）；**3.4** 迁 `CmgmInitializer` 并废止 Bootstrap 目录 |
| **内容扩展1.5 / 项目脚手架1.6** | Editor 模块导入向导（远期） |

最小 JRPG 示例：`Core` + `UI` + `Data` + `Lua`（+ `Integrations`）  
最小 MUG 示例：`Core` + `UI` + `Audio` + `Input`

### 6.2 Editor 目录分层（Runtime 与 Editor 分离）

| 位置 | 内容 | 归属 | 说明 |
|------|------|------|------|
| `Runtime/Core/` | Runtime | 已完成基线 | 原 `Scripts/GameCore/` 内容；**不含** Editor；`CMGM.Core` |
| `Runtime/Modules/*/Editor/` | 模块 Editor | 各模块闭环 | 如 `ExcelTool`→Data、`Edt_CreateUIPanelAction`→UI |
| `CmgmFramework/Editor/` | 框架级 Editor | **编译边界2.9 ✅** | 路径检查、脚手架、通用模板 |
| `_WorkSpace/Scripts/Editor/` | 游戏 Editor（按需） | 远期 | 仅本项目策划/关卡工具，不随框架复制 |

**为何不放进 `Runtime/Core/`？**

1. **程序集边界**：`CMGM.Core` 是 Runtime；Editor 需独立 `CMGM.*.Editor` asmdef，且常 `includePlatforms: Editor`。
2. **依赖范围**：工作区 Editor 常横切多个 Modules（UI 模板 + 配表路径 + manifest），放在 Core 下易让人误以为「只依赖 Core Runtime」。
3. **可选模块导入**：导入向导勾选 Modules 时，`Framework/Editor/` 作为框架壳层保留；各 `Modules/*/Editor/` 随模块一并勾选或跳过。

**结论：** `_WorkSpace/Editor/` → **`CmgmFramework/Editor/`**（与 **`Runtime/`**、**`Resources/`** 同级），**不要**塞进 `Runtime/Core/`。

### 6.3 模块目录重命名（已完成基线，去 `Game*` 前缀）

与 `_WorkSpace/Scripts` 业务层区分，迁入 `CmgmFramework` 时**统一去掉历史模块目录 `Game*` 前缀**（`AudioSystem`→`Audio` 等）。**例外：** 宏观流程模块 **`GameFlow`**（`CMGM.GameFlow`）保留 `Game` 前缀。namespace / asmdef 在对应模块闭环步骤与目录对齐。

| 现目录 | 框架目标 | 计划 namespace / asmdef |
|--------|----------|-------------------------|
| `GameCore/` | `Runtime/Core/` | `CMGM.Core` ✅ |
| `GameUI/` | `Runtime/Modules/UI/` | `CMGM.UI` ✅ |
| `GameData/` | `Runtime/Modules/Data/` | `CMGM.Data` ✅ |
| `GameLevel/` | `Runtime/Modules/Scene/` | **不建模块**（临时 `ScenesManager`，§3.6） |
| `LuaCore/` | `Framework/Integrations/Lua/` | 无 asmdef；`ILuaService` 在 Core ✅ |
| `AudioSystem/` | `Runtime/Modules/Audio/` | `CMGM.Audio`（编译边界2.7） |
| `GameInput/` | `Runtime/Modules/Input/` | `CMGM.Input`（编译边界2.8） |
| `OptionalSystem/` | `Runtime/Modules/Optional/` | `CMGM.Optional` |
| `Utils/` | `Runtime/Modules/Utils/` | 随模块闭环 |

> **Core 无 `GameCore` 子文件夹**：原 `GameCore/` 内文件直接进入 `Runtime/Core/`，不再嵌套一层 `GameCore/`。

### 6.4 CmgmGameKits（【仅作参考】· 非可靠计划）

> **状态：** 下列为历史草案与讨论占位，**不是当前承诺的迭代步骤**；细节待你主动发起讨论后再定。  
> **工程现状：** `CmgmUnityPackages/CmgmGameKits/`（1.4 ✅）；2.7b 曾将音游相关代码**目录暂存**于此，**未**做 asmdef/namespace 正式化。

| 项 | 草案说明（参考） |
|----|------------------|
| **定位** | 与 `Framework/` 并列的可选「业务层工具模板」目录（RoleControl、MapTriggers、MusicGame…） |
| **与 `_WorkSpace/Scripts`** | 业务层 = 本项目独有；GameKits = 可抄可删模板（若将来做） |
| **搬迁** | **项目脚手架1.4 ✅** 已迁入 `CmgmUnityPackages/CmgmGameKits/`（与功能计划无关，仅是物理位置） |
| **详细子步** | 见 §7.4「GameKits【仅作参考】」 |

### 6.5 启动三分工：`CmgmInitializer` · Loading · GameFlow

**组合根（Composition Root）** 在本框架中**收窄为单一薄入口**：**`CmgmInitializer`**（`Runtime/CmgmInitializer.cs`，无独立目录 / namespace）。  
其余「谁先 Init、加载什么资源」**不再**维护第二、第三个 Boot 文件，统一为 **Loading Profile** 内的 **`ILoadTask`** 列表，由 **GameFlow** 决定在何时 `LoadingManager.Run(profile)`。

```
GameFlow（何时、处于哪一宏观态）
    → LoadingManager.Run(Profile, options)
        → ILoadTask[]（ManagerInit / 资源 / 场景 / 业务回调）
    → 目标态 Enter / Exit
```

| 层 | 回答的问题 | 典型产物 |
|----|------------|----------|
| **`CmgmInitializer`** | 进程如何 Awake？谁能**先显示 UI**？ | Logo + `await UIManager.InitAsync()` → `GameFlow.Start()` |
| **Loading** | 本阶段跑哪些异步任务？进度如何？ | `ILoadTask`、`LoadingProfile` SO、`LoadingManager` |
| **GameFlow** | 现在在主菜单还是进游戏中？何时触发哪份 Profile？ | `IGameFlowState`、`GameFlowMachine` |

**Initializer 不能放进 `CMGM.Core`：** 与旧 Boot 相同，Core 若直接引用 `UIManager` 等具体类型会 **Core → Modules 依赖倒置**（§6.1 禁止）。`CmgmInitializer` 落 **`Assembly-CSharp`**，与 Integrations / Scene 临时宿主同级。

#### 6.5a Manager 单例双基类（启动编排3.2 ✅ · **仅纯 C#**）

> **范围（2026-06-19）：** **`LazySingleton<T>` / `BootSingleton<T>`**（替代旧 `Singleton<T>`）。类名暂保留 `BootSingleton`（表示须显式 Init、未就绪 guard）；**调用点**从「Boot 文件」改为 **Initializer 最小集** 或 **Loading 的 `ManagerInitLoadTask`**。  
> **Mono 单例**（`WwiseAudioManager`、`InputManager` 等）策略见 **Audio系统支线**。

| 基类 | 显式 Init | 业务使用 |
|------|-----------|----------|
| **`LazySingleton<T>`** | 默认不写 | `XxxManager.Instance` |
| **`BootSingleton<T>`** | **`await XxxManager.InitAsync()`**（仅 Initializer 或 Loading 任务） | `InitAsync` 完成后 `Instance` 可用 |

- **私有 ctor 不写业务逻辑**；重活在 **`protected virtual UniTask OnInitAsync()`**。
- **Lua**（Boot 型）：`OnInitAsync` 内 `LoadLuaMapper` / `ExecuteLua`——在 **`StartupFramework` Profile** 的 `ManagerInitLoadTask` 中 `await LuaManager.InitAsync()`。

**纪律（3.4 目标；3.2 已落地 InitAsync 机制）：**

- **`BootSingleton`**：只允许 **`CmgmInitializer`（`UIManager` 等最小集）** 或 **Loading `ManagerInitLoadTask`** 调用 `InitAsync()`。
- **禁止**在 Panel / 场景脚本里 Init `BootSingleton`。
- **`LazySingleton`**：不进 Profile，首次 `Instance` 使用。

#### Loading Profile（取代原「双 Boot 清单」）

| Profile（计划名） | 触发方（GameFlow） | UI | 典型 `ILoadTask` |
|-------------------|-------------------|-----|------------------|
| **`StartupFramework`** | `Startup` 态（可与 Logo 并行，**静默**） | 无条或仅 Logo | `ArchiveManager`、`LuaManager`、`PreloadAssetsAsync(主场景)`、Wwise 壳子、`GoToMainScene`… |
| **`EnterGameplay`** | `MainMenu` → `StartGame` | **进度条** | `LoadTable`、Gameplay 场景、gameplay Bank…（**`_WorkSpace` 内 `EnterGameplay` Profile SO + `ILoadTask` 实现类**） |
| **（按需）** `EnterBattle` 等 | `Gameplay` → `Battle` | 可配置 | 战斗资源、战斗 Manager… |

> **业务层清单：** **不**再维护 `GameBootstrap.cs`（**Loading系统1.4b** 废止）。进游戏任务 = **`_WorkSpace/.../LoadingProfiles/EnterGameplay.asset`**（及同目录下 `*LoadTask.cs`）；框架 Loading 经 **`IWorkspaceLoadRegistrar`**（Core 契约，Workspace 实现）注册任务，**不**引用 `CMGM.Workspace` 具体类型。

#### 6.5b Loading 调用纪律（GameFlow + Profile · **方案 A**）

> **决策（2026-06-19）：** Loading **不是**独立宏观态，**不**为每次场景切换 `Push(LoadingState)`。进度与 `LoadScene` 均在 **当前宏观态的过渡方法** 或 **目标态 `Enter`** 内 `await LoadingManager.Run(Profile)`；GameFlow 栈顶始终表示 **玩法阶段**（`Startup` / `MainMenu` / `WorldMap` / `Battle` …），不表示「正在读条」。

**三层分工**

| 层 | 组件 | 职责 |
|----|------|------|
| **原语** | `AddressablesResMgr.LoadSceneAsync` 等 | **仅**出现在 `ILoadTask` 实现内部；业务 / 场景脚本 **不直接调用** |
| **编排** | `LoadingManager.Run(Profile, options)` | 顺序执行 Profile 内 `ILoadTask` 列表；可选 `LoadingPanel` 进度 |
| **策略** | `GameFlowMachine` + 各 `IGameFlowState` | **何时**跑哪份 Profile、是否 `SwitchTo` / `Push(Pause)`；栈顶宏观态不变时由 **当前态的过渡 API** 触发 Loading |

**两条硬规矩（防乱）**

1. **允许触发 `LoadingManager.Run` 的入口（收敛到极少数）**
   - **`GameFlowMachine`** 上对外暴露的过渡 API（如各业务 **`XxxState.Enter`**、**`WorldMapState.TravelTo`**、**`SwitchTo` 后目标态 `Enter`** 等）；实现上 **只** 传入已定义的 **`LoadingProfile` SO**（或 Profile 工厂），**不**在调用方现场 `new` 零散 `ILoadTask` 列表。
   - 各 **`IGameFlowState` 实现类**（框架层 + `_WorkSpace` 业务层）中 **命名清楚的过渡方法**（`Enter`、`TravelTo`、`LoadChapter`、`ReturnToMainMenu` …），且上述方法内部 **只** 调 `LoadingManager.Run` + Profile，不散落其它加载原语。

2. **禁止**
   - 场景 Trigger、Collider、关卡脚本、**Panel 按钮回调** 等 **直接** `LoadingManager.Run(...)` 或 **`LoadSceneAsync`**。
   - 为「读条」单独维护 **`Loading` 宏观态**，或为同态区域切换 **`Push(LoadingState)`**（与本节方案 A 冲突）。
   - **例外（须文档化、仅限 Editor / 测试）：** **`TestSceneEntry`** 等可在进场景后声明追加 Profile（§4.3、**Editor测试系统1.4**）；正式包体路径仍遵守上两条。

**场景 / UI 只表达意图，不执行编排**

```text
Portal.OnEnter     → GameFlowMachine.Current（如 WorldMapState）.TravelTo("World_B")
MainPanel 点开始   → GameFlowMachine.SwitchTo(Gameplay)   // GameplayState.Enter 内 Run EnterGameplay
```

**何时 `SwitchTo`，何时保持栈顶宏观态**

| 场景 | 栈顶宏观态 | 谁 `Run(Profile)` |
|------|------------|-------------------|
| 启动 → 主菜单 | `Startup` → `MainMenu` | `StartupState.Enter` → `StartupFramework` |
| 主菜单 → 进游戏 | `MainMenu` → `Gameplay`（或 `WorldMap`） | 目标态 `Enter` → `EnterGameplay` |
| **大世界探索 A → B（同探索阶段）** | **保持 `WorldMap`（不 `SwitchTo`）** | **`WorldMapState.TravelTo(target)`** → 区域 Profile（含 `LoadSceneTask`） |
| 探索 → 战斗 | `WorldMap` → `Battle` / `PreBattle` | 目标态 `Enter` → `EnterBattle` 等 |
| 战斗暂停 | `Push(Pause)`，通常 **不** Loading | `PauseState.Enter`（Panel / 输入 map） |
| 同宏观态下一关战斗 | 仍 `Battle` | `BattleState.LoadChapter(n)` 等同态过渡 API |

**同态换场景示例（SRPG 大世界）**

```text
栈：[WorldMapState]   // 全程不变

玩家进入传送门
  → WorldPortal（业务层）调用 WorldMapState.TravelTo("World_B")
  → await LoadingManager.Run(WorldRegionTransitionProfile.For("World_B"), showProgress: true)
       └─ ILoadTask：可选卸载、预载、LoadSceneTask("World_B")、UI 摄像机叠加 …
  → 仍在 WorldMapState；新场景内 WorldSceneEntry 做本地初始化（刷怪点、小地图等）
```

**与废止 Scene 模块的关系：** 废 `CMGM.Scene` / 缩退 `ScenesManager` **不是**「只有 `SwitchTo` 才能 Loading」，而是 **不再有第二套场景流程中心**；`GoToMainScene` / `QuitGame` 迁入 **`MainMenuState` 等 GameFlow 态**；`LoadScene` 统一进 **Profile 的 `ILoadTask`**，由 **§6.5b 允许的入口** 触发。

**`CmgmInitializer` 最小集（3.4 验收）：**

```csharp
// Runtime/CmgmInitializer.cs — 无 namespace 或 CMGM 根下薄类
await UIManager.InitAsync();   // 保证 Loading 进度 Panel、主菜单 Panel 可 Show
GameFlowMachine.Start();       // Startup 态内 Run(StartupFrameworkProfile)
```

#### 程序集边界（废止独立 `Bootstrap/` 目录）

| 方案 | 位置 | 本框架 |
|------|------|--------|
| **A. `Assembly-CSharp` + `Runtime/CmgmInitializer.cs`** | 组合根与 Integrations 同程序集 | **目标（3.4）** |
| **B. 独立 asmdef** | 曾为 `CMGM.Bootstrap` | **不做**（Lua / 显式 Init 与 asmdef 冲突，见 3.3 回退记录） |
| **C. 仅引 Core + Registry** | 远期可选 | **模块启动Registry系统** |

#### Manifest / 模块裁剪（Loading Profile 下）

| 手段 | 做法 |
|------|------|
| **Editor 向导（推荐）** | 勾选 Modules → 生成 / 改写 **`LoadingProfile` SO** 与 `ManagerInitLoadTask` 列表（**内容扩展1.5** / **项目脚手架1.6**） |
| **`#if CMGM_MODULE_XXX`** | 向导按勾选注入 Profile 条目 |
| **运行时 Registry** | Profile 任务数 **≥10** 且难维护时再评估（§7.2b） |

#### 模块数量 → Registry 阈值（改为 Profile 任务数）

| Loading Profile 内任务数（含 ManagerInit） | 建议 |
|------------------------------------------|------|
| **&lt; 10** | **Profile SO + 显式任务列表** 足够 |
| **≥ 10** | 评估 **模块启动Registry系统** 或 Editor 生成 Profile |
| **≥ 12** | 强烈建议生成器或 Registry |
| **≥ 15** | 应上 Registry（或等价生成器） |

> 计数含各 Profile 中出现的 ManagerInit、资源预载、Bank、场景等；纯 `LazySingleton` 不计入。

#### 主线「启动编排」

| 步骤 | 内容 | 状态 |
|------|------|------|
| **启动编排3.1** ✅ | 文档约定 Init 纪律 + 三档时机（已被 6.5 修订吸收） | 完成 |
| **启动编排3.2** ✅ | `LazySingleton` / `BootSingleton` + `InitAsync`；UI / Archive / Lua 迁移 | 完成 |
| **启动编排3.3** ✅ | `CmgmFrameBoot` → `Runtime/Bootstrap/`（过渡）；Play 验收 | 完成 |
| **启动编排3.4** | `CmgmFrameBoot` → **`Runtime/CmgmInitializer.cs`**；废止 `Bootstrap/`；长链迁入 **`StartupFramework` Profile**；仅保留 Logo + `UIManager` | **待做**（依赖 Loading系统 **≥1.2**） |

**当前运行时：** 仍为 **3.3** 过渡版 `CmgmFrameBoot`；**目标契约** 见 §4.1、本节。

<details>
<summary>归档：原「双 Boot 组合根」（2026-06-19 前，见 DEPRECATED）</summary>

曾采用 `CmgmFrameBoot` + `GameBootstrap` 两文件显式 `InitAsync` 链。已由 **Initializer + Loading Profile** 取代；`GameBootstrap.cs` 待 **Loading系统1.4b** 删除。
</details>

---

## 7. 迭代计划（主线 / 支线）

> **模型（2026-06-19 重排）：** 旧的单一线性路线图（阶段 0→9）已**归档至 `ARCHITECTURE_DEPRECATED.md`**。现拆为两类：
> - **主线任务**：改动「编译边界 / 启动契约」的地基工作，牵动全局，**必须线性按序**。
> - **支线任务**：各功能系统，满足**解锁条件**后**可并行 / 按需推进**；每条支线内部线性。
>
> **命名约定：**
> - 主线：**功能性章节名 + 连续编号**（`编译边界2.6`、`启动编排3.1`）。
> - 支线：**系统名 + 编号**（`Loading系统1.1`、`项目脚手架1.4`）。  
> - **编号仅在本支线内有效**：不同支线的 `1.4` 互不相关（例如 **项目脚手架1.4** = 包体搬迁；**Loading系统1.4** = 加载 Profile）。  
> - **最前节点**：每条线（主线或支线）**有且仅有一个**——该行内第一个**未完成**的步骤编号（必须带支线前缀，如 `项目脚手架1.2`）。**不是**把同一条线里多个未做步骤都标成「最前」。已全线完成的线标 **—**。🔒 未解锁的线也要列出最前节点，并写解锁条件。报告下一步时见 **§7.6**（含变更量、教学难度）。
> - 少用 `L1`/`M3` 等字母缩写（旧缩写仅在归档文档保留）。

### 7.0 迭代原则（模块闭环，2026-06-15 修订）

原方案「先给全部模块加 asmdef → 再框架/业务分层」在实践中暴露问题：**框架与游戏代码仍混在同一目录时拆程序集**，会引发跨程序集引用、XLua Gen/Runtime 分裂、以及为凑编译而改业务逻辑等连锁错误。

**修订后：按模块闭环**——对每个模块（除已稳定的 Core 外），按固定顺序做完再进入下一模块：

```
① 框架 / 游戏分离（游戏代码在 `_WorkSpace/Scripts/`，框架在 `CmgmUnityPackages/`）
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
| **asmdef 按需、不强迫** | asmdef 是编译边界工具，**不是**每个目录的必选项。若导致组合根注册桥接、破坏显式 Init、或与第三方（XLua）冲突，则**保持 `Assembly-CSharp` + namespace 分层**（§7.5） |
| 小步验证 | 每模块闭环后编译 + 主流程 Play 一次，再开下一模块 |

> 主线「编译边界2.6~2.8」即按此四步推进。

### 7.1 已完成基线（截至 2026-06-19，含主线验收）

| 领域 | 已落地 |
|------|--------|
| Core 程序集 | `CMGM.Core` asmdef + `namespace CMGM.Core`；`Consts.Paths` 单文件；`WorkSpace` 根入 `CmgmFrameSettings`；**`LoadSceneAsync` 原语** |
| 框架/业务分层 | `CmgmUnityPackages/` + `_WorkSpace/Scripts/`；Panel / 配表 / 存档在业务层脚本目录 |
| UI / Data / Audio / Input / Editor | 各模块 asmdef 闭环（Editor 含 `CMGM.Editor`、`CMGM.UI.Editor`、`CMGM.Data.Editor`） |
| Lua | `Integrations/Lua/` + `ILuaService`（**无** Lua asmdef，方案 C） |
| 启动编排 | `LazySingleton` / `BootSingleton` + `InitAsync`；过渡 `CmgmFrameBoot`；目标 **`CmgmInitializer`** + Loading Profile（§6.5、**3.4 待做**） |
| 项目脚手架1.4 | Framework + GameKits → `CmgmUnityPackages`；`Paths.Package` 收口 ✅ |
| Lua Boot 修复 | 根脚本 Init 阶段不走 `LuaBridge.Instance`，内部 `CompleteCurrentExecution` |
| 进游戏入口 | `EnterGameplay` LoadingProfile（**Loading系统1.4b** 废止 `GameBootstrap`） |
| 项目脚手架1.1 | `CmgmUnityPackages/` 占位 + README |
| 项目脚手架1.3 | `_WorkSpace/GAME_WORKSPACE.md` 业务层文档 ✅ |
| Editor 四分法 | `ProjectSetup` / `AssetTemplates` / `QuickSearch` / `Tools`；`project_layer.manifest` ✅ |
| 项目脚手架1.2 | `Edt_ProjectLayerSetup` 菜单 ✅ |
| 项目脚手架1.2b | `ProjectSetup/Seeds` 模板 + 业务层种子；TestSpace 仅顶层 ✅ |
| 项目脚手架1.2c | `project_layer.manifest` 补全 + 重命名 ✅ |
| 项目脚手架1.5 | 空工程迁移验证 ✅ |
| **存档格式优化1.1 / 1.1b** | `CmgmFileFormat` 统一壳；Archive / Config 双侧 `Pack` / `Unpack` ✅ |
| **存档格式优化1.2** | `JsonArchiveSerializer` + `com.unity.nuget.newtonsoft-json` ✅ |
| **存档格式优化1.2b** | `IConfigTableCodec` + `ExcelBinaryConfigTableCodec` ✅ |
| **存档格式优化1.3 / 1.3b** | 读侧 **fail-fast**：无 CMGM 头或 version/kind 不匹配即报错；**不**兼容无头旧档；开发期清档 / 重导表 ✅ |
| 框架 Resources + Runtime 三分 | Settings/UI/Logo/Font → `Resources/`；代码 → `Runtime/` ✅ |
| 迭代模型 | 主线/支线重排；旧 0→9 归档 |

> 完整的旧线性步骤、验收表与废止记录见 `ARCHITECTURE_DEPRECATED.md`。

### 7.2 主线任务（地基，线性按序）

| 步骤编号 | 名称 | 解锁条件 | 状态 |
|----------|------|----------|------|
| **编译边界2.6** | Lua 模块收口（**方案 C**）：XLua 退官方 master；Core 加 `ILuaService` 契约；`LuaManager` 实现 `ILuaService`；`LuaManager`/`LuaBridge` 迁 `Framework/Integrations/Lua`（`Assembly-CSharp`） | 已完成基线 ✅ | **完成 ✅（2026-06-19）** |
| **编译边界2.7** ✅ | Audio 模块解耦与闭环（2.7a/b/c，见 §7.2a） | 编译边界2.6 完成 ✅ | **已完成** |
| **编译边界2.8** ✅ | Input 模块闭环（`CMGM.Input`） | 编译边界2.7 完成 ✅ | **已完成** |
| **编译边界2.9** ✅ | Editor 闭环（`CMGM.Editor`） | 编译边界2.8 完成 ✅ | **已完成** |
| **启动编排3.1** ✅ | 文档约定：两组合根 + Init 纪律 + 三档时机 + Boot 三方案对比 + Registry 阈值（§6.5、§7.2b）；**不改运行时** | 编译边界2.9 完成 ✅ | **完成 ✅（2026-06-19）** |
| **启动编排3.2** ✅ | **仅纯 C#**：`LazySingleton` / `BootSingleton` + `InitAsync`；迁移 UI / Archive / Lua；Mono 单例留 Audio 支线 | 启动编排3.1 完成 ✅ | **完成 ✅（已验收）** |
| **启动编排3.3** ✅ | `CmgmFrameBoot` → `Runtime/Bootstrap/`（过渡）；显式 InitAsync（无 Bootstrap asmdef） | 启动编排3.2 完成 ✅ | **完成 ✅（已验收）** |
| **启动编排3.4** | `CmgmInitializer`：`Runtime/CmgmInitializer.cs`；废止 `Bootstrap/`；长 Init 链 → `StartupFramework` Profile | Loading系统 **≥1.2** ✅ | **待做** |

> 主线 **启动编排3.3** 已完成（过渡运行时）；**3.4** 与 **Loading / GameFlow** 衔接，见 §6.5。Registry 非主线，见 §7.2b。

#### 7.2b 启动编排 · Registry 阈值与远期支线

> **原则：** 默认 **Loading Profile + 显式任务列表**（§6.5）；Registry 是 Profile 任务过多后的**可选升级**。

| Profile 内任务数（含 ManagerInit） | 动作 |
|----------------------------------|------|
| **&lt; 10** | 维持 Profile SO + `ManagerInitLoadTask` / 资源任务显式列表 |
| **≥ 10** | 复盘可维护性；评估「模块启动Registry系统」 |
| **≥ 12** | 强烈建议 Registry **或** Editor 从 Manifest **生成 Profile** |
| **≥ 15** | 应上 Registry（或等价生成器） |

**远期支线「模块启动Registry系统」**（非主线；解锁：启动编排3.4 完成 **且** Profile 任务 ≥10 **或** 导入向导需要运行时动态裁剪）：

| 子步 | 内容 |
|------|------|
| **模块启动Registry系统1.1** | Core：`IGameModule` + `InitPhase`（`StartupFramework` / `EnterGameplay`）+ `CmgmModuleRegistry` |
| **模块启动Registry系统1.2** | 各 Module 适配器 + Register；`LoadingManager` 从 Registry 拼 `ILoadTask` |
| **模块启动Registry系统1.3** | `CmgmModuleManifest` ScriptableObject + Editor 勾选 ↔ Registry 条目 |
| **模块启动Registry系统1.4** | 仅引 Core 的薄组合根 + Registry 可选评估 |

#### 7.2a 编译边界2.7 展开（Audio 解耦三步：先断依赖 → 再挪位置 → 后上 asmdef）

> **背景：** `WwiseAudioManager`（通用音频）与 `MusicSyncTool`（音游）曾循环依赖；必须先断依赖再拆文件。唯一外部调用者 `_TestSpace/SimpleTest.cs`（测试）。

| 子步 | 做什么 | 关键动作 | 验收 | 学习点 |
|------|--------|----------|------|--------|
| **编译边界2.7a** ✅ | 断循环依赖（文件不挪位置） | ① 判定窗口移入 `MusicSyncTool`；② `WwiseAudioManager` 纯播放；③ 音游组合逻辑暂留 `MusicSyncTool`；④ 改 `SimpleTest.cs` | 编译 + Play | 打破循环依赖 |
| **编译边界2.7b** ✅ | 音游代码暂存目录 | ① 相关文件迁至 `Scripts/CmgmGameKits/`（工程内 WIP，**非** GameKits 正式落地）；② `RhythmMap_Path` 迁出 Core | 编译 + Play | 框架/Audio 解耦 |
| **编译边界2.7c** ✅ | Audio 模块闭环（asmdef） | `namespace CMGM.Audio` + `CMGM.Audio.asmdef` | 编译 + Play | Wwise asmdef 引用 |

> 完成 2.7c 后：Audio = 只含通用音频的干净可选模块；音游相关代码目录暂存于 `Scripts/CmgmGameKits/`（2.7b，**非** GameKits 支线正式落地）。

### 7.3 支线任务索引

| 支线系统 | 最前节点 | 解锁条件 | 状态 |
|----------|----------|----------|------|
| **Loading系统** | Loading系统1.1 | Core ✅ + UI ✅ | 已解锁 |
| **存档格式优化** | — | Data ✅；**全线完成 ✅** | 已解锁 |
| **存档升级系统** | 存档升级系统1.1 | **存档格式优化** 全线完成 | 已解锁 |
| **Lua系统** | Lua系统1.1 | 编译边界2.6 ✅ | 已解锁 |
| **Audio系统** | Audio系统1.1 | 编译边界2.7 ✅ | 已解锁 |
| **GameFlow系统** | GameFlow系统1.1 | 启动编排3.3 ✅ | 已解锁 |
| **事件总线系统** | 事件总线系统1.1 | 启动编排3.3 ✅ | 已解锁 |
| **依赖抽象系统** | 依赖抽象系统1.1 | 编译边界2.8 ✅ | 已解锁（按需） |
| **项目脚手架与包体迁移** | **项目脚手架1.6**（远期） | 1.5 ✅ | 已解锁 |
| **GameKits** | GameKits1.1 | 未定（草案写 **项目脚手架1.4** 后，**非可靠**） | 🔒【仅作参考】 |
| **Editor测试系统** | Editor测试系统1.1 | GameFlow系统1.1 进行中 **或** Loading系统1.1 ✅ | 已解锁 |
| **模块启动Registry系统** | 模块启动Registry系统1.1 | 启动编排3.4 ✅ **且** Profile 任务 ≥10 | 🔒 远期 |
| **网游预埋** | 网游预埋1.1 | **存档升级系统** 完成 **且** GameFlow 完成 | 🔒 远期 |
| **内容扩展** | 内容扩展1.1 | 按需 | 按需 |

> 各线内部步骤、验收与学习点见 **§7.4**；带变更量与难度的「下一步」总表见 **§7.6**。

### 7.4 支线展开（各线内部线性）

#### Loading系统（已解锁）

定位：**通用加载编排**——`ILoadTask` 执行器 + 加权进度 + Profile 清单；**取代原双 Boot 长链**（§6.5）。横切 UI / 资源 / Manager Init，属编排层（不进 Core）。**不引用** `CMGM.Workspace` 具体类型（业务任务用回调注入）。

| 子步 | 内容 | 学习点 |
|------|------|--------|
| **Loading系统1.1** | 模块骨架：`Modules/Loading`（`CMGM.Loading`）+ `LoadingManager.Run(tasks)` + 单条进度面板；跑通「执行一组任务并显示进度」 | 模块 asmdef、接口基础 |
| **Loading系统1.2** | `ILoadTask` + 加权进度；**`ManagerInitLoadTask`**（包装 `BootSingleton.InitAsync`） | 接口/多态、Manager 与资源统一任务模型 |
| **Loading系统1.3** | 接通「进游戏」：`GameFlow` / MainPanel → `EnterGameplay` Profile；过渡期可暂用 `DelegateTask` 包装旧 `GameBootstrap` | 模块协作、Profile 驱动 |
| **Loading系统1.4** | **`LoadingProfile` SO**：`StartupFramework` / `EnterGameplay` 静态清单 + 动态追加；**吸收原 `CmgmFrameBoot` Init 链**（配合 **启动编排3.4**） | 数据驱动、与 Initializer 分工 |
| **Loading系统1.4b** | **废止 `GameBootstrap.cs`**：业务进游戏清单迁入 **`EnterGameplay` Profile** + Workspace 侧 `ILoadTask` 实现；`IWorkspaceLoadRegistrar` 注册；更新脚手架种子 / `project_layer.manifest`（**不再**生成 `Scripts/Bootstrap/GameBootstrap.cs`） | 业务清单与框架编排统一 |
| **Loading系统1.5+** | 后台静默加载、转场动画、动态拼任务（`EnterBattle` 等） | 进阶 |

> **进度模型（1.2 起）：** 每个 `ILoadTask` 带 `Weight`；总进度 = `Σ(已完成权重) + 当前任务权重 × 当前任务内部进度`。  
> **Profile（1.4）：** 每个加载点一个 `.asset`；**`StartupFramework`** 默认静默（可与 Logo 并行）；**`EnterGameplay`** 显示进度条。  
> **与 Initializer 分工：** 仅 **`UIManager.InitAsync`** 留在 `CmgmInitializer`；其余 Manager Init 走 **`ManagerInitLoadTask`**。  
> **`GameBootstrap`：** 过渡文件；**1.4b** 删除后，改维护 **`EnterGameplay` Profile**（`_WorkSpace` 内 SO，不进 `CmgmFramework`）。

#### 存档格式优化（✅ 全线完成 · **存档升级系统前置**）

> **动因：** ① `BinaryFormatter` 过时；② 当前 **存档** 与 **配表** 虽同扩展名 `.cmgm`，但磁盘格式不一致（BF blob vs Excel 自定义二进制），无法自检类型、难以统一演进。  
> **目标：** `.cmgm` = **统一容器头** + **分类型 payload**；头里标识 `Archive` / `Config`，各自走自己的编解码器。  
> **范围：** 不含 `ISaveChunk`、完整 Migrator 链（属 **存档升级系统**）。配表 **payload 语义**（行/列二进制布局）本阶段可保持不变，只统一「外壳 + 编解码入口」。

**统一容器头 v1（`CmgmFileFormat`）**

| 字段 | 说明 |
|------|------|
| `magic` | 固定四字节 `CMGM` |
| `version` | `uint32` LE，容器格式版本（当前运行时 `ContainerVersion = 1`） |
| `kind` | `uint8`：`0=Archive`，`1=Config` |
| `payload` | 正文；**Archive** → `JsonArchiveSerializer`；**Config** → `ExcelBinaryConfigTableCodec` |

**读侧策略（1.1 / 1.1b / 1.3 / 1.3b 已落地）：**

| 情况 | 行为 |
|------|------|
| 非 CMGM（无魔数 / 头 truncated / kind 非法） | `CmgmLog.fError` + 抛异常，**不**读 payload |
| `version ≠ ContainerVersion` | `CmgmLog.fWarning`（可能解码错误）→ **仍读 payload** → 解码后 `CmgmFileVersionDebugLog` 打印预览供核对 |
| `kind` 与调用方期望不符 | `CmgmLog.fError` + 抛异常 |
| 无头旧档 / BF 裸 blob | **不支持**；开发期删 `Archives/` 或 **重新导表** |

读写顺序：`Pack` → `CipherTool` → 写盘；读盘反向 → `Unpack(expectedKind, out header)` → 各 Codec。

| 子步 | 内容 | 验收 | 状态 |
|------|------|------|------|
| **存档格式优化1.1** | 统一容器头 + Archive 接入 + `IArchiveSerializer` | 新存档带 `CMGM` 头且 `kind=Archive` | ✅ |
| **存档格式优化1.1b** | Config 侧 `ExcelTool` / `ConfigTableManager` 同壳 | 导表与运行时读表一致 | ✅ |
| **存档格式优化1.2** | Archive payload：`JsonArchiveSerializer`（Newtonsoft UTF-8 JSON） | 无 `BinaryFormatter`；`ArchiveMeta` / `GameRuntimeData` 读写正常 | ✅ |
| **存档格式优化1.2b** | Config payload：`IConfigTableCodec` + `ExcelBinaryConfigTableCodec` 收口读写 | 导表 + LoadTable 经 Codec | ✅ |
| **存档格式优化1.3** | Archive 旧档策略：**fail-fast**，开发期清档 | 无头档直接报错 | ✅ |
| **存档格式优化1.3b** | Config 旧档策略：**fail-fast**，开发期重导表 | 无头配表直接报错 | ✅ |

> **子步顺序建议：** 全线 ✅ → 可开 **存档升级系统**。

#### 存档升级系统（已解锁）

> **定位：** 在 v1 序列化稳定后的**拓展线**——分块、版本迁移、运行时 API 增强。原「阶段 4」中除换序列化外的能力均在此线。

| 子步 | 内容 | 验收 |
|------|------|------|
| **存档升级系统1.1** | 分块 `ISaveChunk` / chunk 注册表；文件头扩展（如 chunk 数量） | 业务层只增 chunk，不改编解码器入口 |
| **存档升级系统1.2** | `ISaveMigrator`：vN → vN+1 迁移管线 | 样例迁移测试 |
| **存档升级系统1.3** | 运行时 API：`SaveSlot` / 异步写盘 / 校验 | 多存档槽与写盘体验 |
| **存档升级系统1.4** | 示例与文档：演示新增字段如何加 chunk | 策划 / 程序可查 |

#### Lua系统（已解锁）

| 子步 | 内容 | 验收 |
|------|------|------|
| **Lua系统1.1** | `ILuaBridgeRegistry`：游戏注册 `Talk` 等 API，框架不写死空实现（承接旧「依赖抽象·Lua 桥」） | Lua 调 C# 游戏逻辑不改框架源码 |
| **Lua系统1.2** | Lua 懒加载：按需 `require`，去掉启动期全量加载 | 大包体启动更快 |
| **Lua系统1.3** | 扫描 `HotRes/Lua` 生成路径常量（§2b **E**） | 路径不再手写字符串 |

#### Audio系统（已解锁）

> **与启动编排衔接：** `WwiseAudioManager`、`InputManager` 等 Mono 单例策略在 **Audio系统1.1** 定案；是否放入 **`StartupFramework` Profile**（非 `CmgmInitializer`）一并写入 Profile 文档。

| 子步 | 内容 | 验收 |
|------|------|------|
| **Audio系统1.1** | `IAudioService` 包装 Wwise；Core / Modules 不直接引用 Wwise API（承接旧「依赖抽象·音频」） | 换音频后端只改 Extension |
| **Audio系统1.2** | Bank 加载 / 卸载策略：gameplay Bank 进游戏按需载、退出卸载 | gameplay Bank 不进启动链 |
| ~~Audio系统1.3~~ | **已废止**：节拍 / MUG 不归 Audio 支线；2.7b 仅将代码暂存 `Scripts/CmgmGameKits/` | — |

#### GameFlow系统（已解锁）

> **模块：** `Runtime/Modules/GameFlow/`（`namespace CMGM.GameFlow`）；编排游戏宏观流程，**不引用** `CMGM.Workspace` 业务类型。

| 子步 | 内容 | 验收 |
|------|------|------|
| **GameFlow系统1.1** ✅ | `IGameFlowState`：`Enter` / `Exit` / `Update`（可选） | 基础态可切换 |
| **GameFlow系统1.2** ✅ | `GameFlowMachine`：Push / Pop / SwitchTo | 栈操作日志可追踪 |
| **GameFlow系统1.3** | 基础态 **`Startup`** / `MainMenu` / `Gameplay`（及按需 `WorldMap` / `Battle`）；**在态 `Enter` / 同态过渡 API 内触发 Loading Profile**（§6.5b，**无** `Loading` 宏观态）；接管 `GoToMainScene` / `QuitGame`（`ScenesManager` 缩退） | 流程 + Loading 衔接 |
| **GameFlow系统1.3b** | **Editor：`DirectToTest`** — `CmgmInitializer` 完成后若存在 **`EditorPlayRequest`**，**跳过 MainMenu / MainScene**，`LoadSceneAsync(目标场景)`；可选追加 Profile（与 **Editor测试系统1.2~1.3** 同期） | Editor 与 Runtime 分支 |
| **GameFlow系统1.4** | 预留态 `Pause` / `Cutscene` / `Battle` 空壳或最小实现 | JRPG / SRPG 可扩展 |
| **GameFlow系统1.5** | 与 UI / 输入：状态切换时 UI 层、输入 map 切换策略 | 暂停时输入正确 |

> **与 Loading：** `Startup.Enter` → `Run(StartupFramework)` → `MainMenu`（正式路径）。**Editor 测试：** `DirectToTest` → `Run(StartupFramework, editorTrim)` → **目标场景**（§4.3）。  
> **宏观态名 `Startup`：** 避免与已废止的 Boot 组合根混淆。

#### Editor测试系统（已解锁）

> **定位：** **框架 Editor**（`CmgmFramework/Editor/`）提供跨项目通用的 Play 测试钩子；**不**把具体战斗/关卡测试逻辑放进 Core。领域可复用辅助放 **GameKits**（远期）；本工程实验场景放 **`_TestSpace`**。

| 子步 | 内容 | 验收 |
|------|------|------|
| **Editor测试系统1.1** | **`EditorPlayRequest`**（Editor 静态/Session）：`TargetScenePath`、`SkipLogo`、`EditorTrimStartup`、`AppendProfiles`；文档约定 **Play Mode Start Scene = InitScene** | 任意场景 Play 先进 InitScene |
| **Editor测试系统1.2** | 菜单 **「草木句萌 / 从当前场景 Play」**（`CMGM.Editor`）：写入 `EditorPlayRequest` → 进入 Play | 不打开 InitScene 也能从当前场景测 |
| **Editor测试系统1.3** | 与 **GameFlow系统1.3b** 对接：`DirectToTest` 在 Startup Profile 后 **`LoadSceneAsync(TargetScenePath)`**，**不** `GoToMainScene` | 空场景 / 测试场景可达 |
| **Editor测试系统1.4** | 可选 **`TestSceneEntry`**（`_TestSpace` 或业务层 MonoBehaviour）：`Start` 时声明本场景追加的 Profile（如 `EnterGameplay` / `EnterBattle`） | 进场景后再 Loading |
| **Editor测试系统1.5** | **`StartupFrameworkEditorTrim` Profile**（或 Profile 变体）：Editor 下跳过 MainScene 预载 / Logo 等 | 迭代更快 |

> **分层：** **框架** = `EditorPlayRequest` + 菜单 + `GameFlow.DirectToTest`；**GameKits** = 可复用玩法测试模板（远期）；**`_TestSpace`** = 本项目 `TableTest`、测试场景与 `TestSceneEntry` 实例。

#### 事件总线系统（已解锁）

| 子步 | 内容 | 验收 |
|------|------|------|
| **事件总线系统1.1** | `IEventBus`：`Subscribe` / `Publish` / `Unsubscribe` + 线程/退订生命周期约定 | 规则文档 + 空实现可编译 |
| **事件总线系统1.2** | 落地 `OptionalSystem/EventSystem`；在 **`StartupFramework` Profile** 或懒加载 Init（见 §6.5） | 无全局静态散落 |
| **事件总线系统1.3** | 选 1~2 处 Manager 直连改事件（如场景切换完成） | 行为不变、解耦 |

#### 依赖抽象系统（已解锁 / 按需）

> 承接旧「阶段7 依赖抽象」中**未并入其它支线**的部分。其中音频抽象 `IAudioService` 见 **Audio系统1.1**、Lua 桥抽象 `ILuaBridgeRegistry` 见 **Lua系统1.1**；此处只放渲染/编辑器第三方解耦。

| 子步 | 内容 | 验收 |
|------|------|------|
| **依赖抽象系统1.1** | Odin 降级：框架 asmdef 去掉 Odin 硬依赖，或 `#if ODIN_INSPECTOR` 条件编译 | 无 Odin 也能编 Core/Modules |
| **依赖抽象系统1.2** | URP / RP 抽象：`IRenderPipeline` 薄封装或「换 RP 检查清单」文档 | 换渲染管线有章可循 |

#### 内容扩展（按需，多数依赖对应系统）

| 子步 | 内容 | 验收 |
|------|------|------|
| **内容扩展1.1** | 对话系统：Lua / 配表驱动对话 UI 与分支 | 样例对话可跑 |
| **内容扩展1.2** | 场景持久化：进出场景对象 Save/Load 钩子（按需；**非**预建 Scene 模块） | 进出场景状态保留 |
| **内容扩展1.3** | 配表类型扩展：多键表、嵌套结构、本地化列 | ExcelTool 支持 |
| **内容扩展1.4** | SRPG 接口：网格 / 回合 / 技能预留（与 GameFlow Battle 衔接） | 与 Battle 态衔接 |
| **内容扩展1.5** | Editor 模块导入向导：勾选 `Runtime/Modules/*` → 生成 **`LoadingProfile` SO** 任务列表 + 依赖报告（与 **项目脚手架1.6** 合并） | 复制到新工程可裁剪 |

> **去向说明：** 旧 8.4「Lua 懒加载」→ **Lua系统1.2**；旧 8.5「MUG」→ 2.7b 目录暂存（非 GameKits 正式计划）。

#### 项目脚手架与包体迁移（已解锁）

> **动因：** 框架代码与游戏内容混在 `_WorkSpace/Scripts` 不便跨项目拷贝；常量入口分散（`Consts.Paths` / `MusicGameConsts` 等）；新项目缺少标准业务层目录。  
> **目标形态：** 可移植代码 → `Assets/CmgmUnityPackages/{CmgmFramework,CmgmGameKits}`；业务层 → `_WorkSpace` + `_TestSpace`；框架文档 → `ARCHITECTURE.md`（随框架）；业务层约定 → `_WorkSpace/GAME_WORKSPACE.md`（随项目）。  
> **节奏：** 1.1 ✅ → **1.4 ✅** → **1.3 ✅** → **1.2 ✅** → **1.2b ✅** → **1.2c ✅** → **1.5 ✅**…；**本线最前节点 = 1.6（远期）**。

| 子步 | 内容 | 验收 |
|------|------|------|
| **项目脚手架1.1** ✅ | `CmgmUnityPackages/` 占位 + ARCHITECTURE 目标结构 | 占位目录存在 |
| **项目脚手架1.4** ✅ | Framework + `CmgmGameKits` **目录**物理搬迁 → `CmgmUnityPackages`；`Consts.Paths.Package` 收口 | 编译 + Play |
| **项目脚手架1.3** ✅ | `_WorkSpace/GAME_WORKSPACE.md` 模板 | 游戏文档与框架文档分离 |
| **项目脚手架1.2** ✅ | WorkSpace 脚手架 Editor（`草木句萌/脚手架/` 菜单）；**仅目录 + GAME_WORKSPACE.md** | 空工程可建骨架；PathCheck 通过 |
| **项目脚手架1.2b** ✅ | 业务层种子 + `project_layer.manifest`；模板源 `Editor/ProjectSetup/Seeds/` | 脚手架可写最简闭环 |
| **项目脚手架1.2c** ✅ | manifest 补全（`GAME_WORKSPACE.md`、`_Generated/Config/`、`GameBootstrap.cs`）；`work_space_scaffold` → **`project_layer.manifest`** | PathCheck 与菜单一致 |
| **项目脚手架1.5** ✅ | 空工程迁移验证 | 可复制 |
| **项目脚手架1.6**（远期） | Manifest 驱动勾选 → 生成 **LoadingProfile** 任务（+ 可选 asmdef） | 按勾选裁剪 |
| **项目脚手架1.7**（按需） | 路径扫描自动生成 / 校验（与 **Lua系统1.3** / §2b **E** 衔接） | 路径少手写 |

> **废止说明：** 独立支线「常量与配置体系」已并入本支线（1.1 盘点、1.4 路径收口、1.7 自动生成）；详见 `ARCHITECTURE_DEPRECATED.md` **归档块 D**。

**项目脚手架1.2b（✅ 2026-06-20）**

> **原则：** 业务层最简模板 **不进 `CmgmFramework/Runtime` 或 `Resources`**；脚手架从 `Editor/ProjectSetup/Seeds/` 复制到 `_WorkSpace`。

| 类别 | `project_layer.manifest` 写入 `_WorkSpace`（缺失则创建） |
|------|----------------------------------------------------------|
| 顶层 | `_TestSpace/`、`_WorkSpace/`、`GAME_WORKSPACE.md` |
| 目录 | `Excels/`、`HotRes/`、`Scripts/_Generated/Config/` |
| 文本 | `HotRes/Lua/main.lua.txt`、`HotRes/BuildSource/RELEASE_NOTE.txt` |
| 场景 | `HotRes/Scenes/InitScene.unity`、`MainScene.unity` |
| UI | `HotRes/UI/Panels/MainPanel.prefab`、`LoadingPanel.prefab`；`Scripts/UI/Panels/MainPanel.cs`、`LoadingPanel.cs` |
| Boot（业务侧，**过渡**） | `Scripts/Bootstrap/GameBootstrap.cs`（**Loading系统1.4b** 废止 → **`LoadingProfiles/EnterGameplay.asset`**） |

| **GameBootstrap → Profile** | **1.2c 种子仍含 `GameBootstrap.cs`（过渡）**；**Loading系统1.4b** 改为种子 **`LoadingProfiles/EnterGameplay.asset`** + 示例 `ILoadTask`；自 `project_layer.manifest` **移除** `GameBootstrap.cs` |

#### 模块启动Registry系统（🔒 远期，见 §7.2b）

> 默认不做。Profile 任务 **≥10** 时评估；**≥15** 时建议必做。与 Loading Profile 显式列表二选一或并存。

| 子步 | 内容 |
|------|------|
| **模块启动Registry系统1.1** | `IGameModule` + `InitPhase` + `CmgmModuleRegistry` |
| **模块启动Registry系统1.2** | Module 适配器 + Register；`LoadingManager` 从 Registry 拼 Profile 任务 |
| **模块启动Registry系统1.3** | `CmgmModuleManifest` + Editor 勾选 |
| **模块启动Registry系统1.4** | 薄组合根 + Registry 可选评估 |

#### GameKits（🔒【仅作参考】· 非可靠计划）

> **不是当前执行队列。** 下列子步仅供日后讨论参考；2.7b / 1.4 已在 `CmgmUnityPackages/CmgmGameKits/` 做了**目录暂存**，不等于 GameKits1.1 已完成。

| 子步（参考） | 内容（草案） | 验收（草案） |
|--------------|--------------|--------------|
| **GameKits1.1** | 目录与 asmdef；`namespace CMGM.GameKits` | 引用 Framework Modules 编译通过 |
| **GameKits1.2** | RoleControl：2D / 3D 角色控制器模板 | 示例场景可跑 |
| **GameKits1.3** | MapTriggers：场景触发器基类 | 与流程层无硬耦合 |
| **GameKits1.4** | Camera：跟随 / 边界（按需） | 可选 |
| **GameKits1.5** | 发布 / 整包可删策略 | 与 **项目脚手架1.6** 衔接 |
| **GameKits1.6** | MusicGame / BeatSync 正式化（2.7b 暂存代码的后续，若做） | 待定 |

#### 网游预埋（🔒 远期，**存档升级系统** + GameFlow 完成后）

| 子步 | 内容 | 验收 |
|------|------|------|
| **网游预埋1.1** | 存档分层：LocalSave vs ServerSync 接口分离 | 单机不受影响 |
| **网游预埋1.2** | 网络层：连接 / 心跳 / 消息编解码占位 | 可 mock 服务器 |
| **网游预埋1.3** | 战斗重放：输入序列 + 确定性 tick 记录 | 回放一致 |
| **网游预埋1.4** | Cloud save：与存档格式兼容的上传 / 合并策略 | 文档 + 伪代码 |

#### ~~常量与配置体系~~（已并入 **项目脚手架与包体迁移**）

> 2026-06-19 起废止独立支线；原 1.1~1.4 映射见 **项目脚手架1.1 / 项目脚手架1.4 / 项目脚手架1.7**。

### 7.6 下一步一览（各线最前节点）

> **用法：** 日常推进以 **§7.7 启动竖切大表** 为准（**当前唯一活跃主线**）。本表保留全仓索引；非 §7.7 涉及支线在完成竖切前**仅修 bug，不新开功能**。

| 线 | 最前节点 | 状态 | 解锁条件 | 预估变更量 | 教学难度 |
|----|----------|------|----------|------------|----------|
| **★ 启动竖切（§7.7）** | **Loading系统1.2** | **#3 ✅** | 见 §7.7 | 见 §7.7 | ★★★☆ |
| **主线（编译边界 + 启动编排）** | **启动编排3.4** | 3.3 ✅；3.4 待做（**并入 §7.7 阶段 C**） | Loading **≥1.2** | **中** | ★★★☆ |
| **Loading系统** | Loading系统1.2 | **§7.7 #4 ← 当前** | Loading 1.1 ✅ | **中** | ★★★☆ |
| **GameFlow系统** | GameFlow系统1.3 | **§7.7 #8** | GameFlow 1.2 ✅ | **中** | ★★★☆ |
| **Editor测试系统** | Editor测试系统1.1 | **§7.7 阶段 D 前置** | Loading **1.1** ✅ | **小~中** | ★★☆ |
| **存档升级系统** | 存档升级系统1.1 | ⏸ 竖切完成后 | 存档格式优化 ✅ | **中~大** | ★★★★ |
| **Lua系统** | Lua系统1.1 | ⏸ 竖切完成后 | 2.6 ✅ | **小~中** | ★★★☆ |
| **Audio系统** | Audio系统1.1 | ⏸ 竖切完成后 | 2.7 ✅ | **中** | ★★★☆ |
| **事件总线系统** | 事件总线系统1.1 | ⏸ 竖切完成后 | 3.3 ✅ | **小~中** | ★★☆☆ |
| **项目脚手架与包体迁移** | **项目脚手架1.6**（远期） | 远期按需 | 1.5 ✅ | **大** | ★★★★ |
| **内容扩展** | 内容扩展1.1 | 按需 | 各子项依赖对系统 | **不一** | ★★~★★★★ |
| **GameKits** | GameKits1.1 | 🔒【仅作参考】 | **未定** | **未定** | **未定** |
| **模块启动Registry系统** | 模块启动Registry系统1.1 | 🔒 远期 | **启动编排3.4** ✅ **且** Profile 任务 ≥10 | **大** | ★★★★ |
| **网游预埋** | 网游预埋1.1 | 🔒 远期 | **存档升级系统** 完成 **且** GameFlow 完成 | **大** | ★★★★★ |

> **说明：** **存档格式优化 ✅** 已完成；**存档升级**等数据/抽象线在 §7.7 全线 ✅ 前不新开。

### 7.7 启动竖切 · 主流程大表（Loading + GameFlow + Editor + 3.4）

> **决策（2026-06-19）：** 在 **§7.7 全部步骤 ✅** 之前，**不新开**存档升级、Lua、Audio、事件总线、脚手架 1.6 等支线功能（紧急 bugfix 除外）。  
> **推进方式：** 按 **#** 顺序逐步验收；同一 **阶段** 内标注「可并行」的步骤可同时做，但 **# 更小者优先合入**（避免 Init 链冲突）。

#### 总览

```
阶段 A ──► Loading 1.1 ──► 1.2 ──► 1.3
              ∥ 并行        GameFlow 1.1 ──► 1.2
阶段 B ──► （A 全部 ✅ 后进入）
阶段 C ──► Loading 1.4 + 启动编排 3.4 + GameFlow 1.3        （同里程碑，一并验收）
阶段 D ──► Editor 1.1 ──► GameFlow 1.3b + Editor 1.2~1.3
阶段 E ──► Loading 1.4b（废止 GameBootstrap）
可选 ──► Editor 1.4~1.5、Loading 1.5+
```

#### 逐步明细

| # | 阶段 | 步骤 ID | 内容 | 前置 | 验收标准 | 主要产出 | 变更量 | 状态 |
|---|------|---------|------|------|----------|----------|--------|------|
| **1** | **A · 执行器** | **Loading系统1.1** | `Modules/Loading`（`CMGM.Loading` asmdef）+ `LoadingManager.Run(tasks)` + 单条进度 UI；用硬编码/假任务跑通 | Core ✅ + UI ✅ | Play 后能看到进度条走完一组任务 | `LoadingManager`、`LoadingPanel`（或复用 UI 层）、asmdef | **中** | **✅** |
| **2** | A · 并行 | **GameFlow系统1.1** | `IGameFlowState`：`Enter` / `Exit` / `Update`（可选） | 启动编排3.3 ✅ | 两个空态可手动切换，日志可追踪 | `Modules/GameFlow/`、`CMGM.GameFlow` asmdef | **中** | **✅** |
| **3** | A · 并行 | **GameFlow系统1.2** | `GameFlowMachine`：Push / Pop / SwitchTo | **#2** ✅ | 栈操作日志正确 | `GameFlowMachine.cs` | **小** | **✅** |
| **4** | **A · 任务模型** | **Loading系统1.2** | `ILoadTask` + `Weight` 加权进度；**`ManagerInitLoadTask`**（包 `BootSingleton.InitAsync`） | **#1** ✅ | 多任务加权进度正确；至少 1 个 Manager 经 Task 初始化 | `ILoadTask`、`ManagerInitLoadTask`、`DelegateTask`（过渡） | **中** | **待做 ← 当前** |
| **5** | **B · 业务竖切** | **Loading系统1.3** | 接通「进游戏」：`MainPanel` → `LoadingManager.Run(EnterGameplay…)`；过渡期 **`DelegateTask` 包旧 `GameBootstrap`** | **#4** ✅；（**#3** 建议 ✅） | 主界面点进游戏走 Loading + 进度条，行为与现 `GameBootstrap` 等价 | `MainPanel` 改调 Loading；临时 EnterGameplay 任务列表 | **中** | 待做 |
| **6** | **C · 启动重构** | **Loading系统1.4** | **`LoadingProfile` SO**：`StartupFramework` / `EnterGameplay` 静态清单 + 动态追加；清单吸收原 **`CmgmFrameBoot` Init 链**（Manager / 预载 / Wwise 壳等） | **#4** ✅ + **#5** ✅ | 启动任务可配在 SO；不再硬编码长链 | `LoadingProfile.cs`、`.asset` 资源 | **中** | 待做 |
| **7** | C · 同里程碑 | **启动编排3.4** | `CmgmFrameBoot` → **`Runtime/CmgmInitializer.cs`**；废止 `Bootstrap/`；Initializer **仅** Logo + `await UIManager.InitAsync()` → 交 GameFlow | **#6** 同步进行 | InitScene 上 Boot 脚本瘦身；长 Init 不在 Initializer 内 | `CmgmInitializer.cs`；删/废 `CmgmFrameBoot` | **中** | 待做 |
| **8** | C · 同里程碑 | **GameFlow系统1.3** | 宏观态 **`Startup` / `MainMenu` / `Gameplay`**（§6.5b）；`Startup.Enter` → `Run(StartupFramework)` → `MainMenu`；**接管** `GoToMainScene` / `QuitGame`（`ScenesManager` 缩退） | **#6** **#7** + **#3** ✅ | 正式包体：Init → Startup Profile → 主界面；无 FrameBoot 直调 Scene | `StartupState`、`MainMenuState` 等；`ScenesManager` 缩退 | **中** | 待做 |
| **9** | **D · Editor 前置** | **Editor测试系统1.1** | **`EditorPlayRequest`**（Session）：`TargetScenePath`、`SkipLogo`、`EditorTrimStartup`、`AppendProfiles`；约定 **Play Mode Start Scene = InitScene** | **#1** ✅ | 运行时/Editor 可读 Request；文档与 Project Settings 一致 | `EditorPlayRequest.cs`（Editor + 运行时可见的轻量 DTO） | **小** | 待做 |
| **10** | **D · Editor 路径** | **GameFlow系统1.3b** | **`DirectToTest`**：存在 `EditorPlayRequest` 时 **跳过 MainMenu / MainScene**，Startup Profile 后 `LoadSceneAsync(目标场景)` | **#8** ✅ + **#9** ✅ | Editor 分支不进 MainScene | `DirectToTest` 或 `StartupState` 内 Editor 分支 | **小~中** | 待做 |
| **11** | D · 同里程碑 | **Editor测试系统1.2** | 菜单 **「草木句萌 / 从当前场景 Play」**：写入 Request → 进入 Play | **#9** ✅ | 任意场景一键 Play，先进 InitScene | `Edt_PlayFromCurrentScene.cs` 等 | **小** | 待做 |
| **12** | D · 同里程碑 | **Editor测试系统1.3** | E2E：菜单 Play → Initializer → Startup（trim）→ **目标场景**（如 `_TestSpace/TableTest`） | **#10** **#11** ✅ | 测试场景可达；不强制 MainScene | 与 GameFlow 联调验收 | **小** | 待做 |
| **13** | **E · 清理** | **Loading系统1.4b** | **废止 `GameBootstrap.cs`**：清单迁入 **`EnterGameplay` Profile** + Workspace `ILoadTask`；**`IWorkspaceLoadRegistrar`**；更新脚手架种子 / `project_layer.manifest` | **#6** ✅ + **#8** ✅ | 仓库无 `GameBootstrap`；`MainPanel` 只认 Profile；新工程种子为 Profile | 删 Bootstrap 目录；Seeds / manifest | **中** | 待做 |

#### 阶段验收清单（里程碑 Definition of Done）

| 阶段 | 包含 # | 阶段 DoD（全部满足才算 ✅） |
|------|--------|------------------------------|
| **A** | 1~4 | Loading 能 Run 加权任务；GameFlow 空态机可切换 |
| **B** | 5 | 进游戏经 Loading 进度条（可仍包旧 Bootstrap 逻辑） |
| **C** | 6~8 | 正式启动：Initializer 薄 + StartupFramework Profile + GameFlow 到主界面 |
| **D** | 9~12 | Editor「从当前场景 Play」→ Init → 目标测试场景 |
| **E** | 13 | `GameBootstrap` 删除；业务只维护 Profile SO |

#### 竖切完成后可选（不在当前冻结范围）

| 步骤 ID | 内容 | 说明 |
|---------|------|------|
| **Editor测试系统1.4** | `TestSceneEntry`：进场景后声明追加 Profile | `_TestSpace` / 业务层 |
| **Editor测试系统1.5** | `StartupFrameworkEditorTrim` Profile | Editor 迭代加速 |
| **Loading系统1.5+** | 静默加载、转场、`EnterBattle` 动态拼任务 | 玩法扩展 |
| **GameFlow系统1.4~1.5** | `Pause` / `Battle` 空壳；UI / 输入 map | JRPG 扩展 |

#### 竖切完成后的解锁（⏸ → 可开）

| 支线 | 建议顺序 |
|------|----------|
| **存档升级系统** | 1.1 → 1.2 → … |
| **Lua系统 / Audio系统** | 1.1（Startup Profile 策略写入文档） |
| **事件总线系统** | 1.1 → 1.2（可挂 Startup Profile） |
| **项目脚手架1.6** | 按需 |

> **旧 §7.7 并行原则（存档/Lua/Audio 小线）** 在竖切冻结期**暂停**；竖切 ✅ 后恢复「1 条中线 + 1~2 小线」。

### 7.5 跨线解锁关系 + 设计决策

**跨线依赖（绝大多数是「支线依赖主线」，反向极少）：**

| 任务 | 依赖方向 | 说明 |
|------|----------|------|
| GameFlow系统 | 依赖 **Loading ≥1.2**（软）+ 启动编排3.3 ✅ | 1.3 通过 Profile 触发；接管 `GoToMainScene` |
| Loading系统1.4 / **启动编排3.4** | Loading **≥1.2** | Initializer 瘦身 + `StartupFramework` Profile |
| Loading系统1.3 | 软依赖 Loading **1.2** | `EnterGameplay` Profile（过渡可包一层旧 `GameBootstrap`） |
| Loading系统1.4b | Loading **1.4** | 废止 `GameBootstrap`；脚手架/manifest 更新 |
| Editor测试系统1.3 | 软依赖 **GameFlow系统1.3b** + Loading **1.1** | `DirectToTest` 跳过 MainScene |
| **项目脚手架1.2** | 依赖 **项目脚手架1.3 ✅** + **1.4 ✅** | 一键骨架应对准搬迁后路径 |
| **项目脚手架1.4** ✅ | 依赖 启动编排3.3 ✅ + **1.1 ✅** | 物理搬迁至 `CmgmUnityPackages`（勿与其他支线 **1.4** 混淆） |
| 网游预埋 | 依赖**支线**（**存档升级系统** + GameFlow） | 远期 |
| **存档升级系统** | 依赖 **存档格式优化** 全线完成（**含 1.1b~1.3b**） | chunk / Migrator 建立在统一 `.cmgm` v1 之上 |

**设计决策记录 · 2026-06-19（Loading / Scene 重定位）**

| 决策 | 结论 |
|------|------|
| **不建 Scene 模块** | 场景加载原语 `LoadSceneAsync` **下沉 Core**（`AddressablesResMgr`）；**不**维护 `CMGM.Scene` 模块/asmdef。`Modules/Scene/ScenesManager` 仅为**临时流程宿主**（`GoToMainScene` / `QuitGame`），待 **GameFlow系统1.3** 接管后缩退或删除。若未来需 Additive / 流式 / 持久化 / 转场，**按需**评估（可能落在 GameFlow / Loading / 业务层），YAGNI 不预建 Scene 模块。 |
| **Loading 复活为独立模块** | `Modules/Loading`（`CMGM.Loading`）作为通用加载服务，分步迭代（见 §7.4 Loading系统1.1~1.5+）。之前「Loading 不单独建 Modules」的延后结论就此推翻。 |

> 旧的「Level→Scene 重命名 + `CMGM.Scene` 闭环」程序集部分已回退；相关旧编号映射见 `ARCHITECTURE_DEPRECATED.md`。

**设计决策记录 · 2026-06-19（XLua / Lua 模块定位，编译边界2.6）**

| 项 | 结论 |
|------|------|
| **背景** | 此前接入的是 XLua 官方 `feature/asmdef` 分支（`Xlua.Core.asmdef`），但该分支在官方已被 **Revert**（PR#1067 加入、PR#1068/commit d919198 撤销）。原因非运行时不稳定，而是「Gen 代码须与核心同程序集」「hotfix 须核心在 `Assembly-CSharp`」两条约束与 asmdef 冲突，官方放弃维护（Issue #1174 至今 open）。 |
| **结论：方案 C** | Lua **不单独建 asmdef**；`ILuaService` 在 Core、实现在 `Integrations/Lua`；Boot **显式** `await LuaManager.InitAsync()`（§6.5） |
| **Integrations 定位** | 框架级第三方桥接（XLua 等），与 **`CmgmInitializer`** 同类，落 `Assembly-CSharp` |
| **取舍** | 放弃「Lua 独立 asmdef 封装包」的强制边界（现阶段几乎用不上：依赖方向多为 Boot→Lua、Lua→业务），换回**官方主线可升级 + hotfix 之门重开 + wrap 生成走 happy-path**，消除「绑死回滚版本」长期风险。被封装 Module 仍通过 `ILuaService` 契约保持分层与可迁移性。 |
| **正交提醒** | XLua **交互模式**（反射 codeless ↔ 生成 wrap）与程序集归属无关：反射模式的性能/GC、IL2CPP 裁剪问题，**出包前**仍需以 `link.xml` / `[ReflectionUse]` 或生成 wrap 处理；本决策与之独立。 |
| **hotfix** | 维持**关闭**；核心回 `Assembly-CSharp` 后门已重开，真要 C# 级热更（远期网游化）时在 XLua Hotfix / HybridCLR 间再评估（单机 JRPG/SRPG 阶段不需要）。 |

> 旧「编译边界2.6 = `CMGM.Lua` asmdef + XLua Gen 同单」及相关 asmdef 文件已废弃，迁入 `ARCHITECTURE_DEPRECATED.md`。

**设计决策记录 · 2026-06-19（编译边界2.7 Audio 解耦）**

| 项 | 结论 |
|------|------|
| **2.7 三步** | `2.7a` 断循环依赖 → `2.7b` 音游代码暂存 `Scripts/CmgmGameKits/` → `2.7c` Audio 上 asmdef |
| **Audio 解耦** | `WwiseAudioManager` 仅保留通用播放；音游逻辑从 Audio 剥离 |
| **2.7b 范围** | 断依赖后的**目录暂存**（2.7b 时在 `Scripts/CmgmGameKits/`，**项目脚手架1.4 ✅** 后 → `CmgmUnityPackages/CmgmGameKits/`）；**非** GameKits 支线正式落地 |
| **RhythmMap_Path** | 从 Core 迁出（2.7b） |
| **Input 2.8** | `InputActions_Main` 迁入 `Modules/Input/`；UI→Input 单向 |

**设计决策记录 · 2026-06-19（项目脚手架 / 包体迁移 / 常量体系并入）**

| 项 | 结论 |
|------|------|
| **CmgmUnityPackages** | 在 `Assets/` 下建 `CmgmUnityPackages/{CmgmFramework,CmgmGameKits}`，与 `_WorkSpace` 分离；框架更新时整块 sync 回框架仓。 |
| **搬迁时机** | 1.1 ✅；**1.4 ✅**（本仓先迁）；**1.3 → 1.2** 补文档与新工程工具 |
| **文档分工** | `ARCHITECTURE.md` 跟框架；每个游戏 `_WorkSpace/GAME_WORKSPACE.md` 描述业务层约定 |
| **常量体系** | 并入 **项目脚手架**（1.1 盘点、**项目脚手架1.4** 路径规范、**项目脚手架1.7** 自动生成） |

**设计决策记录 · 2026-06-19（CmgmFramework 目录三分：Resources / Editor / Runtime）**

| 项 | 结论 |
|------|------|
| **顶层** | `CmgmFramework/{Resources,Editor,Runtime}`；**不用**顶层 `Scripts` |
| **Runtime** | `CmgmInitializer.cs`、`Core/`、`Modules/`、`Integrations/` 均在 `Runtime/` 下（过渡：`Bootstrap/CmgmFrameBoot.cs` 待删） |
| **模块 Editor** | 仍在 `Runtime/Modules/*/Editor/`（asmdef 限定 Editor 平台） |
| **路径常量** | `Paths.Framework.Runtime`、`Integrations` 等指向 `Runtime/…`（`Paths.Framework.Bootstrap` 过渡保留至 **3.4**） |

> **说明：** 脚手架 **1.2b ✅** 后本线最前节点为 **1.5**。

**CmgmFramework/Editor 布局（四分法 · 2026-06-20）**

```
Editor/
├── ProjectSetup/           工程级：manifest、Seeds、路径检查、脚手架菜单
│   ├── Manifests/
│   ├── Seeds/
│   ├── Edt_CmgmEditorPaths.cs
│   ├── Edt_ProjectLayerSetup.cs
│   ├── Edt_ProjectLayerManifest.cs
│   ├── Edt_ProjectPathCheck.cs
│   └── Edt_ManifestPathUtil.cs
├── AssetTemplates/         右键 CMGM Create：脚本 / Excel / 空文件夹模板
│   └── Templates/
├── QuickSearch/            菜单跳转与打开系统目录
│   └── Edt_QuickSearchMenus.cs
└── Tools/                  独立小工具（**Editor测试**「从当前场景 Play」等）
    └── TMP/
```

> 模块专属 Editor（如 `ExcelTool`、`Edt_CreateUIPanelAction`）仍在 `Runtime/Modules/*/Editor/`；跨模块工程能力放 `CmgmFramework/Editor/`。

**设计决策记录 · 2026-06-20（`.cmgm` 统一容器 · Archive + Config）**

| 项 | 结论 |
|------|------|
| **统一壳** | 所有 `.cmgm` 文件共用 **同一文件头**（magic + version + **kind**）；扩展名不再隐含类型 |
| **分轨 payload** | `kind=Archive` → `IArchiveSerializer`；`kind=Config` → `IConfigTableCodec`（Excel 表二进制，语义可延续现网） |
| **共享模块** | 容器 `Pack`/`Unpack` 放 Data 模块（如 `CmgmFileFormat`）；`ArchiveManager`、`ExcelTool`、`ConfigTableManager` 只调壳 + 各自 Codec |
| **CipherTool** | 仍包裹 **整文件**（头+payload）；改加密算法须 bump `version` 或 kind 内子版本 |
| **1.1b** | 配表导出/读表与存档 **同步** 上头，避免只改一侧导致 `.cmgm` 仍两种形态 |
| **1.3b 默认** | Config 旧档：**重导表**；读侧 **fail-fast**，无 Legacy |
| **读侧 fail-fast** | 无 CMGM 头 → 报错；**不**兼容无头 / BF 裸 blob |
| **version 告警** | 头可读但 `version ≠ ContainerVersion` → **Warning** 后继续解码，并打印预览；正式迁移留 **存档升级系统** Migrator |

**设计决策记录 · 2026-06-20（存档 · 格式优化 vs 升级拓展）**

| 项 | 结论 |
|------|------|
| **拆线** | 原「存档升级系统」拆为两条：**存档格式优化**（最小包，前置）+ **存档升级系统**（拓展） |
| **格式优化** | 1.1~1.3（Archive）+ **1.1b~1.3b**（Config）；**不含** `ISaveChunk` |
| **升级拓展** | 原 1.2~1.6 重编号为 **存档升级系统1.1~1.4**；解锁 = 格式优化 **含 b 子步** 全线完成 |
| **旧档** | **不支持 Legacy**；开发期清 `Archives/` / 重导表；跨 version 迁移在 **存档升级系统1.2** Migrator |

**设计决策记录 · 2026-06-20（脚手架 · 业务层种子 vs 框架层）** ✅ 1.2b

| 项 | 结论 |
|------|------|
| **不进框架** | `main.lua`、`RELEASE_NOTE`、`InitScene`、`MainScene`、`MainPanel`、`LoadingPanel`（及对应脚本）**不**放在 `CmgmFramework/Resources` 或 `Runtime/` |
| **创建时机** | **脚手架**在 `_WorkSpace` 建目录时 **一并** 写入最简模板（文本 copy / 预制体从 Editor 模板导出） |
| **框架 Resources** | 仍仅：Settings、UI 基建、Logo、字体（**Initializer** 契约层） |
| **_TestSpace** | 脚手架 **只建顶层**空目录；子文件夹留给使用者自建 |
| **模板存放** | `Editor/ProjectSetup/Seeds/`（脚手架）；`Editor/AssetTemplates/Templates/`（右键新建） |
| **清单文件** | `Editor/ProjectSetup/Manifests/project_layer.manifest`（业务层/测试层）；`framework_path_check.manifest`（框架目录） |
| **GameBootstrap** | **废止**（**Loading系统1.4b**）→ **`EnterGameplay` LoadingProfile**（`_WorkSpace`） |

**设计决策记录 · 2026-06-19（CmgmInitializer + Loading Profile）**

| 项 | 结论 |
|------|------|
| **组合根** | 单一薄入口 **`CmgmInitializer`**（`Runtime/CmgmInitializer.cs`）；**废止**独立 `Bootstrap/` 目录与 `CMGM.Bootstrap` namespace |
| **Initializer 职责** | Logo + **`UIManager.InitAsync`**（保证能显示 UI）→ **`GameFlow.Start()`** |
| **其余编排** | 全部 **Loading Profile** + `ILoadTask`（含 **`ManagerInitLoadTask`**）；由 **GameFlow** 决定何时 `Run(profile)` |
| **Profile** | **`StartupFramework`**（静默，可与 Logo 并行）、**`EnterGameplay`**（进度条） |
| **过渡** | 运行时仍为 **`CmgmFrameBoot`**（3.3）；**启动编排3.4** + **Loading 1.4** 收敛 |
| **`BootSingleton`** | 类名暂保留；`InitAsync` 仅 Initializer 最小集或 Loading 任务 |
| **`GameBootstrap.cs`** | **废止**（**Loading系统1.4b**）；清单 = **`EnterGameplay` Profile SO** + Workspace `ILoadTask` 类 |
| **Editor 测试** | **Editor测试系统** + **GameFlow.DirectToTest**（§4.3）；任意场景 Play → Initializer → **目标场景**（可跳过 MainScene） |

**设计决策记录 · 2026-06-19（业务层 Workspace + GameFlow 命名）**

| 项 | 结论 |
|------|------|
| **业务层** | 中文统称 **业务层**；英文 **Workspace**（目录 `_WorkSpace` 不变） |
| **namespace** | 业务层脚本 **`CMGM.Workspace`**（原 `CMGM.Game`） |
| **GameFlow** | 原支线「GameState系统」更名为 **「GameFlow系统」**；模块 `Runtime/Modules/GameFlow/`（`CMGM.GameFlow`） |
| **框架 `Game*`** | 默认仍避免；**例外**：宏观游戏流程 → `GameFlow*`（见 §8） |

**设计决策记录 · 2026-06-19（CmgmFramework 内置 Resources）**

| 项 | 结论 |
|------|------|
| **归属** | `CmgmFrameSettings`、UI 基建（UICamera/Canvas/EventSystem）、Logo（BeforeGame）、默认字体 → **`CmgmFramework/Resources/`**（框架包随带，非业务层） |
| **路径常量** | `Paths.Framework.Resources`；`Resources.Load` 键不变（如 `UI/UICamera`、`CmgmFrameSettings`） |
| **_WorkSpace** | **不设** `Resources/`；业务层 = Scripts + HotRes + Excels 等增量 |
| **脚手架 1.2** | 不再在工作区创建 `CmgmFrameSettings`；新工程依赖框架包内默认 asset，按需改字段 |
| **Runtime 三分** | 见上节「目录三分」；`Paths.Framework.Runtime` ✅ |

**设计决策记录 · 2026-06-19（启动编排3.3 · asmdef pragmatic）**

| 项 | 结论 |
|------|------|
| **3.3 范围** | 过渡：`CmgmFrameBoot` 迁 `Runtime/Bootstrap/` ✅；**不**建 `CMGM.Bootstrap.asmdef`（已回退方案 B）。 |
| **3.4 修订** | 废止 `Bootstrap/`；组合根 → **`Runtime/CmgmInitializer.cs`**（§6.5）。 |
| **asmdef 原则** | **按需加、不强迫**（§7.0）。`CmgmInitializer`、Integrations/Lua、Scene 临时宿主均无 asmdef。 |

**潜在完善候选（backlog，未立支线；当用户问「还能怎样进一步完善框架」时主动提醒）**

| 候选 | 方向 | 触发时机 |
|------|------|----------|
| **Input系统拓展** | 键位重绑定、键鼠/手柄/触屏设备切换、输入缓冲、`InputManager→UI` 解耦（走事件总线） | 主线/主要支线收尾、框架趋于稳定时 |

---

## 8. 关键约定

### 配表管线

```
Excel（Excels/）
  → ExcelTool 导出（ExcelBinaryConfigTableCodec）→ Pack → CipherTool
  → StreamingAssets/TableConfig/*.cmgm
  → ConfigTableManager：CipherTool → Unpack → ExcelBinaryConfigTableCodec → dataDic
```

- 容器类必须有 `Dictionary<K, VRow> dataDic` 字段
- `ConfigTableManager` 通过反射读 `dataDic` 泛型参数推断行类型
- 无 CMGM 头或 version/kind 不合法 → **报错**（开发期用「清除并构建 Excel 数据」）

### 存档管线

```
GameRuntimeData（I_Saveable）
  → IArchiveSerializer（JsonArchiveSerializer）写 UTF-8 JSON payload
  → Pack(CmgmFileFormat, kind=Archive) → CipherTool
  → persistentDataPath/Archives/*.cmgm
  → 读：CipherTool → Unpack(Archive) → Deserialize
```

> Archive payload 为 **UTF-8 JSON**；Config payload 为 **Excel 自定义二进制**（经 `IConfigTableCodec`）。换 JSON 后须清空旧 BF 存档。

### `.cmgm` 统一容器（v1 · 已落地）

```
┌──────────────────────────────────────────┐
│  CmgmFileHeader (v1)                     │
│  magic | version | kind (Archive/Config) │
├──────────────────────────────────────────┤
│  payload（由 kind 分发）                    │
│  Archive → JSON 等（IArchiveSerializer）   │
│  Config  → Excel 表二进制（IConfigTableCodec）│
└──────────────────────────────────────────┘
         ↕ CipherTool（整文件）
              .cmgm 文件
```

**涉及代码：** `CmgmFileFormat`、`IConfigTableCodec` / `ExcelBinaryConfigTableCodec`、`JsonArchiveSerializer`、`ArchiveManager`、`ExcelTool`、`ConfigTableManager`、`CipherTool`。

### Workspace 层目录约定（Archive / Config 并列）

| 路径常量 | 目录 | 内容 |
|----------|------|------|
| `Paths.WorkSpaceScripts.Archive` | `_WorkSpace/Scripts/Archive/` | 运行时存档结构脚本（可变） |
| `Paths.WorkSpaceScripts.Config` | `_WorkSpace/Scripts/_Generated/Config/` | Excel 导出的配表 Container（只读，勿手改） |
| `Paths.WorkSpaceScripts.Bootstrap` | `_WorkSpace/Scripts/Bootstrap/` | **过渡**；**1.4b** 后改 `LoadingProfiles/`（或废止 Bootstrap 目录） |
| `Paths.WorkSpaceScripts.UI_Panels` | `_WorkSpace/Scripts/UI/Panels/` | Panel 脚本 |
| `Paths.Framework.DataModule.Archive` | `CmgmFramework/Runtime/Modules/Data/Archive/` | 存档框架（`ArchiveManager`、`I_Saveable`） |
| `Paths.Framework.DataModule.Config` | `CmgmFramework/Runtime/Modules/Data/Config/` | 配表框架（`ConfigTableManager`） |
| `Paths.Framework.DataModule.Editor` | `CmgmFramework/Runtime/Modules/Data/Editor/` | Data 模块 Editor（`ExcelTool`、`ArchiveEditor`） |
| `Paths.Framework.Editor` | `CmgmFramework/Editor/` | 框架级 Editor |
| `Paths.Framework.Runtime` | `CmgmFramework/Runtime/` | 运行时代码根 |
| `Paths.Framework.Resources` | `CmgmFramework/Resources/` | Settings、UI 基建、Logo、字体 |
| `Paths.Package.Framework` | `CmgmUnityPackages/CmgmFramework/` | **项目脚手架1.4** ✅ |
| `Paths.Package.GameKits` | `CmgmUnityPackages/CmgmGameKits/` | **项目脚手架1.4** ✅ |

**不**把 Config 嵌套在 Archive 下：二者生命周期不同（配表 vs 存档）。

### UI 管线

```
ShowPanel<T>() → Addressables 加载 HotRes/UI/Panels/{T}.prefab
  → 挂到对应 E_UILayer 层 Canvas
```

- **主界面 / 主场景 ✅**：`CmgmFrameSettings.MAIN_PANEL_NAME`、`MAIN_SCENE_NAME`；`ScenesManager.GoToMainScene` 统一调用。游戏内其他 Panel 仍优先 `ShowPanel<T>()`。
- **D（AssetAddresses）**：Settings 字符串已够用；Address 键集中管理留待后续按需做。

### 资源加载分层（Initializer / Profile）

| 阶段 | 负责 | 应加载 | 不应加载（示例） |
|------|------|--------|------------------|
| **`CmgmInitializer`** | Logo + `UIManager.InitAsync` | UI 基建（Canvas / EventSystem） | 其余 Manager、配表、Bank |
| **Logo → 主界面** | **`StartupFramework` Profile**（GameFlow `Startup` 态，静默） | `ArchiveManager`、`LuaManager`、主场景预载、Wwise 壳子、`GoToMainScene`… | 角色配表、gameplay Bank |
| **主界面 → 进游戏** | **`EnterGameplay` LoadingProfile**（`_WorkSpace`） | `LoadTable`、关卡 Addressables、Bank、Gameplay 场景 | — |
| **运行时懒加载** | `GetTable` / Addressables 按需 | 非关键表、可选资源 | 已在 Profile 声明的必需项 |

- `ConfigTableManager.GetTable<T>()` 仍保留懒加载兜底，但**进游戏必需表**应在 **`EnterGameplay` Profile** 显式 `LoadTable`。
- 业务进游戏清单：维护 **`_WorkSpace/.../LoadingProfiles/EnterGameplay.asset`** 及关联 `ILoadTask`（**Loading系统1.4b** 废止 `GameBootstrap.cs`）。

### 框架 / 业务层命名（2026-06-19 修订）

| 侧 | 约定 | 示例 |
|----|------|------|
| **框架层**（`Framework/`） | 默认**避免** `Game*` 前缀；**例外**：宏观游戏流程 → `GameFlow*` | `CmgmInitializer`、`ArchiveManager`、`GameFlow` |
| **业务层**（`_WorkSpace/Scripts/`，`namespace CMGM.Workspace`） | 本项目业务；**进游戏加载** = **`LoadingProfiles/*.asset`** + `ILoadTask` 实现类 | `GameRuntimeData`、`RoleTableLoadTask`（示例名） |
| **Unity 引擎 API** | 不改动 | `GameObject`、`GamePlayActions`（Input 生成名） |
| **StreamingAssets** | 配表输出目录 **`TableConfig/`**（原 `GameConfig/`） | `Consts.Paths.ConfigData` |

> **术语：** 中文称 **业务层**；英文称 **Workspace**（目录 `_WorkSpace`）。原 `CMGM.Game` namespace 已统一为 **`CMGM.Workspace`**。原支线「GameState系统」已更名为 **「GameFlow系统」**（`Modules/GameFlow/`，`namespace CMGM.GameFlow`）。

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
- 报告「下一步」时以 **§7.7 启动竖切大表** 为准；竖切完成后恢复 **§7.6** 全仓索引。

---

*当前聚焦：**§7.7 #3 GameFlow 1.2**（#2 ✅）｜Loading 主线：**#4 Loading 1.2**。*
