# CMGM 框架架构说明

> 本文档是框架化改造的长期参考（「北极星」）。  
> 目标：将当前工程从「带 Demo 的原型项目」逐步改造为「可跨项目迁移的框架」。  
> 最后更新：阶段 2.3b（M2 Data asmdef）；下一步 **2.4** ScenesManager 配置化

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
    │       │   ├── Archive/ GameArchiveManager、I_Saveable
    │       │   ├── Config/  GameConfigManager
    │       │   └── Editor/  ExcelTool、ArchiveEditor（CMGM.Data.Editor）
    │       ├── Level/       原 GameLevel
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
| `GameConfigManager` | 读取 `.cmgm` 二进制配表（反射 + 解密） | ★★★★ |
| `ExcelTool`（Editor） | Excel → Container.cs + 二进制 | ★★★★ |
| `RoleInfoContainer` 等 | 游戏配表（`Scripts/Game/Config/`，2.3 ✅） | — |
| `GameArchiveManager` | 存档元数据 + 运行时数据读写 | ★★★ |
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

### 3.5 关卡 / 流程（现 `GameLevel/` → 2.1a 后 `Framework/Modules/Level/`）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `GameInitializer` | 启动 Logo + 各系统 Init + 进主界面 | ★★ |
| `ScenesManager` | 场景切换、回主界面 | ★★ |

### 3.6 可选模块

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
InitScene（GameInitializer.Awake）
    │
    ├─ 显示 Logo（Video / Texture）
    │
    └─ InitGame()  ── UniTask 并行 ──┐
                                      │
        ┌─────────────────────────────┘
        │
        ├─ AddressablesResMgr.PreloadAssetsAsync("MainScene")
        ├─ UIManager.Init()
        ├─ GameArchiveManager.Init()        ← 构造函数已读存档元数据
        ├─ LuaManager.Init()
        │     ├─ 注册 Loader 链
        │     ├─ [非热重载] LoadLuaMapper()
        │     └─ ExecuteLua(ROOT_LUA_URI)
        └─ WwiseAudioManager.Init()
        │
        _gameInitFinished = true
        │
        └─ ScenesManager.GoToMainScene()
              ├─ ClearRuntimeData + ClearPanel
              ├─ ShowPanel<MainPanel>()      ← ⚠ 硬编码（暂保留泛型；2.1b 字符串写法仅为过渡，见 2.4/2.5）
              └─ LoadSceneAsync("MainScene") ← ⚠ 硬编码
```

### 已知生命周期问题（待阶段 3 解决）

- 部分 Manager 在**构造函数**里做重活（`UIManager`、`GameArchiveManager`），`Init()` 反而是空的
- `LuaManager.Init()` 内部 fire-and-forget，`GameInitializer` 不 await Lua 真正就绪
- 无统一模块注册 / 依赖顺序 / 失败回滚

---

## 5. 耦合点（框架化的主要障碍）

以下代码属于**游戏层**，但目前放在框架 Scripts 中，迁移新项目时必须改框架源码：

| 耦合点 | 位置 | 问题 |
|--------|------|------|
| 主界面 Panel | `ScenesManager.GoToMainScene()` → `MainPanel` | 硬编码游戏 UI；**现阶段保留** `ShowPanel<MainPanel>()`（泛型优先）。2.1b 曾用 `ShowPanel("MainPanel")` **仅为程序集解耦过渡**，正式方案见 **2.4 / 2.5** |
| 主场景名 | `GoToMainScene()` → `"MainScene"` | 硬编码场景 |
| 存档数据结构 | `GameRuntimeData`（博物、任务、背包…） | 游戏专属字段 |
| 配表容器 | `RoleInfo` 等（`Scripts/Game/Config/`，2.3 ✅） | 游戏专属表结构 |
| Lua 桥接 | `LuaBridge.Talk` | 空实现，且写死在框架里 |
| 第三方绑定 | Wwise / Odin / URP 直接引用 | 换音频 / RP 需改 Core |

### 其他技术债

| 问题 | 位置 | 计划阶段 |
|------|------|----------|
| namespace / asmdef | `GameCore` 已有 `CMGM.Core` + asmdef（1.1~1.3 ✅）；其余模块待阶段 2 模块闭环（2.1b~2.8） | 见 §7.1 |
| `BinaryFormatter` 序列化 | `GameArchiveManager` | 阶段 4 |
| 无 GameState 状态机 | — | 阶段 5 |
| 无事件总线 | `OptionalSystem/` | 阶段 6 |

---

## 6. 目标架构（三层）

```
┌─────────────────────────────────────────────────┐
│  YourGame.Runtime（每个项目独有）                  │
│  GameRuntimeData / 配表 Container / Panel /       │
│  ScenesManager 配置 / GameBootstrap 模块注册       │
└───────────────────────┬─────────────────────────┘
                        │ 依赖
┌───────────────────────▼─────────────────────────┐
│  CMGM.Extensions（可选模块）                       │
│  Wwise 音频 / Input / BeatSync / 未来战斗接口      │
└───────────────────────┬─────────────────────────┘
                        │ 依赖
┌───────────────────────▼─────────────────────────┐
│  CMGM.Core（框架核心，跨项目复用）                  │
│  单例 / 资源 / UI 基类 / 存档接口 / Lua 宿主 /      │
│  IGameModule Bootstrap / 事件总线接口              │
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
      Bootstrap/                   2.5 GameBootstrap
    Framework/
      Core/                        必选（原 GameCore 内容直接在此，无 GameCore 子目录）
      Modules/                     可选，按项目勾选（§6.1、§6.3）
        UI/                        原 GameUI
        Data/                      原 GameData（CMGM.Data）
          Archive/                 GameArchiveManager、I_Saveable
          Config/                  GameConfigManager
          Editor/                  ExcelTool、ArchiveEditor
        Level/                     原 GameLevel
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
| **Modules** | `…/Level/` | `Level` | 推荐 | 原 `GameLevel/`；`CMGM.Level` |
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

最小 JRPG 示例：`Core` + `UI` + `Data` + `Level` + `Lua`  
最小 MUG 示例：`Core` + `UI` + `Audio` + `Input`（+ `Level` 若走统一 Init）

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
| `GameLevel/` | `Framework/Modules/Level/` | `CMGM.Level` |
| `LuaCore/` | `Framework/Modules/Lua/` | `CMGM.Lua` |
| `AudioSystem/` | `Framework/Modules/Audio/` | `CMGM.Audio` |
| `GameInput/` | `Framework/Modules/Input/` | `CMGM.Input` |
| `OptionalSystem/` | `Framework/Modules/Optional/` | `CMGM.Optional` |
| `Utils/` | `Framework/Modules/Utils/` | 随模块闭环 |

> **Core 无 `GameCore` 子文件夹**：原 `GameCore/` 内文件直接进入 `Framework/Core/`，不再嵌套一层 `GameCore/`。

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

**建议模块闭环顺序：** UI → Data → Level → Lua → Audio / Input → Editor 工具

### 7.0b 子编号说明（2.x / 2.xa / 2.xb）

主表里的 **2.1、2.2…** 是阶段内主步骤；**字母后缀不是按 a→z 的执行顺序**，而是按**类型**区分。看 **「← 下一步」** 列和本表为准。

| 编号形式 | 含义 | 示例 |
|----------|------|------|
| **2.x** | 主步骤：迁移、配置化、Bootstrap 等 | 2.1 迁 Panel ✅；2.2 迁存档 |
| **2.xa** | **目录 / 布局**（仅 2.1a）：`Framework/` 收拢 + §6.3 去 `Game*` 前缀 | 在 **2.1b** 之前做 |
| **2.xb** | **M 模块闭环**：namespace + asmdef | 2.1b=M1 UI；2.3b=M2 Data；2.5b=M3 Level… |
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
| **2** 框架/游戏分层 | 2.4 `ScenesManager` 去硬编码；`IGameFlowConfig` / `GameBootstrap` 回调；**D** `AssetAddresses`（§2b） | **← 下一步** |
| **2** 框架/游戏分层 | 2.5 新建 `Scripts/Game/Bootstrap/GameBootstrap.cs`，游戏专属初始化从 `GameInitializer` 拆出 | 待做 |
| **2** 框架/游戏分层 | 2.5b **M3 闭环**：`Level` 模块 `CMGM.Level` asmdef | 待做 |
| **2** 框架/游戏分层 | 2.6 **M4 闭环**：`Lua` 模块 `CMGM.Lua` asmdef（与 XLua Generate Code 同单） | 待做 |
| **2** 框架/游戏分层 | 2.7 **M5 闭环**：`Audio` + `Input` 模块 asmdef | 待做 |
| **2** 框架/游戏分层 | 2.8 **M6 闭环**：Editor namespace + asmdef | 待做 |
| **3** Bootstrap | 3.1 定义 `IGameModule` + `CmgmInitContext` | 待做 |
| **3** Bootstrap | 3.2~3.4 将各 Manager 改为 Module，构造函数不再做重活 | 待做 |
| **3** Bootstrap | 3.5 `GameInitializer` 改为按 Order 依次 await 注册模块 | 待做 |
| **3** Bootstrap | 3.6 游戏项目在 `GameBootstrap` 注册自己的 Module | 待做 |
| **4** 存档升级 | 4.1~4.6 分块存档、`ISaveChunk`、版本头、替换 `BinaryFormatter`、迁移示例 | 待做 |
| **5** GameState | 5.1~5.5 状态机基础态 + Pause/Cutscene/Battle 预留 | 待做 |
| **6** 事件总线 | 6.1~6.4 `IEventBus` 落地 OptionalSystem，替代一处直接调用 | 待做 |
| **7** 依赖抽象 | 7.1~7.4 `IAudioService`、`ILuaBridgeRegistry`、Odin 降级、URP 文档或抽象 | 待做 |
| **7** 依赖抽象 | 7.5 `CmgmModuleManifest`：模块 id、Core/Modules 分级、依赖链（§6.1） | 待做 |
| **8** 内容扩展 | 8.1 对话；8.2 场景持久化；8.3 配表类型扩展；8.4 Lua 懒加载 + **E** 路径代码生成（§2b）；8.5 MUG；8.6 SRPG 接口 | 按需 |
| **8** 内容扩展 | 8.7 Editor 模块导入向导：勾选 `Framework/Modules` 子目录，校验 asmdef / 场景引用 | 待做 |
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
| **2.2** ✅ | 迁出游戏存档结构 | `GameRuntimeData` → `Scripts/Game/Archive/`；`I_Saveable` → `Framework/Modules/Data/Archive/`；`GameArchiveManager` 通用读写 | 读档 / 存档流程不变 |
| **2.3** ✅ | 迁出游戏配表 | `RoleInfoContainer` → `Scripts/Game/Config/`；`ExcelTool` 输出至 `Paths.Game.Config`；生成类带 `namespace CMGM.Game` | Editor 导表 + `LoadTable<RoleInfo>()` 正常 |
| **2.3b M2** ✅ | Data 模块闭环 | `CMGM.Data` + `CMGM.Data.Editor` asmdef；Editor 收拢至 `Data/Editor/` | 编译 + 导表 + 存档 Init |
| **2.4** | ScenesManager 配置化 | 主场景名、主 Panel 不再硬编码；`IGameFlowConfig` / `GameBootstrap` 注册回调（**Game 层仍用 `ShowPanel<T>()`**，Level 不引用 Game）；**D** `AssetAddresses` 或 Label | 换主场景 / 主 UI 不改框架源码 |
| **2.5** | GameBootstrap | 新建 `Scripts/Game/Bootstrap/GameBootstrap.cs`；游戏专属 Init（注册主界面 Panel 展示、配表预载等）从 `GameInitializer` 拆出 | Init 流程清晰：框架 Init vs 游戏 Init |
| **2.5b M3** | Level 模块闭环 | `CMGM.Level` asmdef | Logo → 各系统 Init → 主场景 全流程 |
| **2.6 M4** | Lua 模块闭环 | `CMGM.Lua` + XLua 同单 | Lua 启动、`require`、C# 桥接无类型分裂错误 |
| **2.7 M5** | Audio + Input 闭环 | `CMGM.Audio`、`CMGM.Input` asmdef | 音频事件、输入 map 正常 |
| **2.8 M6** | Editor 模块闭环 | `_WorkSpace/Editor/` → `Framework/Editor/`；各 `Modules/*/Editor/` + `CMGM.Editor` asmdef（§6.2） | 路径检查、模板、导入向导可用 |

#### 阶段 3 · Bootstrap 模块化

| 编号 | 任务 | 详细内容 | 验收 |
|------|------|----------|------|
| 3.1 | 模块接口 | 定义 `IGameModule`（`Order`、`InitAsync(CmgmInitContext)`、`Shutdown` 等）与 `CmgmInitContext`（共享服务访问） | 接口文档 + 空实现可编译 |
| 3.2 | UI / 资源 Module | `UIManager`、`AddressablesResMgr` 改为 Module；构造函数不做重活 | Init 只在 `InitAsync` |
| 3.3 | 存档 / Lua Module | `GameArchiveManager`、`LuaManager` 同上 | Lua Init 可被 await |
| 3.4 | 音频 Module | `WwiseAudioManager`（及可选 Input）注册为 Module | 启动顺序可配置 |
| 3.5 | 统一调度 | `GameInitializer` 收集 Module 列表，按 `Order` 依次 `await InitAsync`；失败可日志 / 中断策略 | 无 fire-and-forget 的 Init |
| 3.6 | 游戏注册 | `GameBootstrap` 向框架注册游戏专属 Module（如配表预载、GameState 入口） | 新项目只改 Game 层注册 |

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
| 5.3 | 基础态 | `Boot`、`MainMenu`、`Gameplay`、`Loading` | 与 ScenesManager 协作 |
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
| **M3** | 2.5b | Level（`CMGM.Level`） |
| **M4** | 2.6 | Lua（`CMGM.Lua`） |
| **M5** | 2.7 | Audio + Input |
| **M6** | 2.8 | Editor asmdef（最后） |

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
| `I_Saveable` 归属 Archive 模块 | **已迁** `Archive/I_Saveable.cs`；**已建** `CMGM.Data` asmdef | — | 2.3b ✅ |
| **`CMGM.Game` asmdef** | **框架不创建**；`Scripts/Game/` 示例代码进默认 `Assembly-CSharp`，保留 `namespace CMGM.Game` | **各游戏项目自定** | 框架主迭代 `Framework/*` 程序集；JRPG / SRPG 等可自建 Game asmdef |

**记录时间：** 2026-06-15（`I_Saveable` 提前迁移尝试后回退；`CMGM.Game` asmdef 移除）

---

## 8. 关键约定

### 配表管线

```
Excel（Excels/）
  → ExcelTool 导出
  → *Container.cs（行类 *Row + 容器类 *）
  → StreamingAssets/GameConfig/*.cmgm
  → GameConfigManager.LoadTable<T>() / GetTable<T>()
```

- 容器类必须有 `Dictionary<K, VRow> dataDic` 字段
- `GameConfigManager` 通过反射读 `dataDic` 泛型参数推断行类型

### 存档管线

```
GameRuntimeData（I_Saveable，Scripts/Game/Archive/）
  → GameArchiveManager 序列化（Framework/Modules/Data/Archive/）
  → persistentDataPath/Archives/
```

### Game 层目录约定（Archive / Config 并列）

| 路径常量 | 目录 | 内容 |
|----------|------|------|
| `Paths.Game.Archive` | `Scripts/Game/Archive/` | 运行时存档结构（可变） |
| `Paths.Game.Config` | `Scripts/Game/Config/` | Excel 导出的配表 Container（只读） |
| `Paths.Framework.DataModule.Archive` | `Framework/Modules/Data/Archive/` | 存档框架（`GameArchiveManager`、`I_Saveable`） |
| `Paths.Framework.DataModule.Config` | `Framework/Modules/Data/Config/` | 配表框架（`GameConfigManager`） |
| `Paths.Framework.DataModule.Editor` | `Framework/Modules/Data/Editor/` | Data 模块 Editor（`ExcelTool`、`ArchiveEditor`） |

**不**把 Config 嵌套在 Archive 下：二者生命周期不同（配表 vs 存档）。

### UI 管线

```
ShowPanel<T>() → Addressables 加载 HotRes/UI/Panels/{T}.prefab
  → 挂到对应 E_UILayer 层 Canvas
```

- **编码偏好**：能写泛型时优先 `ShowPanel<T>()`，避免框架层散落 Panel 名字符串。
- **Level ↔ Game 解耦**：Level 未建 asmdef 前，`ScenesManager` 可继续 `ShowPanel<MainPanel>()`；**2.1b 字符串写法仅为过渡**，**2.5b** 建 `CMGM.Level` 前须在 **2.4 / 2.5** 用接口或 `GameBootstrap` 回调解耦（Game 层内部仍用泛型）。

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

*下一步：**2.4** — `ScenesManager` 配置化 + `GameBootstrap` 回调解耦（见 §8 UI 管线）。*
