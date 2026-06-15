# CMGM 框架架构说明

> 本文档是框架化改造的长期参考（「北极星」）。  
> 目标：将当前工程从「带 Demo 的原型项目」逐步改造为「可跨项目迁移的框架」。  
> 最后更新：阶段 1.2b（Consts 合并 + 可配置根路径）

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
├── Editor/                  工作区级 Editor 工具（非 Scripts 内）
└── Scripts/                 全部 C# 代码（⚠ 框架与游戏尚未分离）
    ├── GameCore/            核心基础设施
    ├── GameUI/              UI 框架 + Panel（⚠ 含游戏 Panel）
    ├── GameData/            配表 + 存档（⚠ 含游戏数据）
    ├── LuaCore/             XLua 宿主
    ├── GameLevel/           场景流程 + 初始化（⚠ 含游戏逻辑）
    ├── AudioSystem/         Wwise + 节拍同步
    ├── GameInput/           输入封装
    ├── OptionalSystem/      占位（Event / Command 未实现）
    └── Utils/               工具类
```

### 路径常量入口

所有路径由 **`Consts.Paths`**（`GameCore/Consts.Paths.cs`）统一定义：

- **`WorkSpace`** — 从 `CmgmFrameSettings.WORK_SPACE_ROOT` 读取（默认 `Assets/_WorkSpace`），其余路径由此派生
- `HotRes` / `HotScene` / `Lua_Path` / `RhythmMap_Path` / UI、Scripts 等
- `ARCHIVE_PATH`、`ConfigData` — 运行时路径（persistentDataPath / StreamingAssets）

换项目时：改 Settings 里的根路径即可，不必改代码里的字符串。

---

## 2b. 路径与资源引用优化线（并入总路线图）

除主阶段外，路径/地址相关改进按下列步骤穿插推进：

| 代号 | 内容 | 计划阶段 | 状态 |
|------|------|----------|------|
| **A** | 合并 `Consts` 为单文件，去掉 partial | 1.2b | ✅ |
| **C** | `WorkSpace` 根路径迁入 `CmgmFrameSettings` | 1.2b | ✅ |
| **B** | 拆 `FrameworkPaths`（Core）与 `GamePaths`（Game 层） | 2.2~2.3 | 待做 |
| **D** | Addressables 加载键独立为 `AssetAddresses` 或 Label 分组 | 2.4（ScenesManager 配置化同期） | 待做 |
| **F** | 关键 Prefab/SO 改用 `AssetReference`，减少字符串路径 | 2.1 迁 Panel 后 / 7.x | 按需 |
| **E** | 扫描 `HotRes/` 自动生成路径常量（代码生成） | 8.4 前后（Lua 懒加载、内容量上来后） | 远期 |

---

## 3. 模块清单

### 3.1 核心模块（GameCore）

| 组件 | 文件 | 职责 | 成熟度 |
|------|------|------|--------|
| 单例基类 | `Singleton/` | 懒汉 / Mono / AutoMono 三种单例 | ★★★ |
| 资源管理 | `ResourceManagement/AddressablesResMgr` | AB 加载、引用计数、防重复、预加载 | ★★★★ |
| 对象池 | `ResourceManagement/ObjectPool/` | 基础对象池 | ★★ |
| 帧设置 | `CmgmFrameSettings` | LOG、Lua 热重载、**工作区根路径**、根脚本路径 | ★★★ |
| 路径常量 | `Consts.Paths` | 由 Settings 派生的目录与管线路径 | ★★★ |
| 日志 | `CoreUtils/CmgmLog` | 分级日志；Error 不受 LOG 开关影响 | ★★★ |
| Mono 调度 | `MonoManager/` | Update 委托挂载 | ★★ |

### 3.2 UI（GameUI）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `UIManager` | 分层 Canvas、异步加载 Panel、Hide 中途取消容错 | ★★★★ |
| `BasePanel` | Panel 基类 | ★★★ |
| `Panels/MainPanel` 等 | ⚠ **游戏层 UI**，应在框架化后迁出 | — |
| Editor 工具 | Panel 模板创建、快速搜索 | ★★★ |

### 3.3 数据（GameData）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `GameConfigManager` | 读取 `.cmgm` 二进制配表（反射 + 解密） | ★★★★ |
| `ExcelTool`（Editor） | Excel → Container.cs + 二进制 | ★★★★ |
| `RoleInfoContainer` 等 | ⚠ **游戏配表**，应迁到 Game 层 | — |
| `GameArchiveManager` | 存档元数据 + 运行时数据读写 | ★★★ |
| `GameRuntimeData` | ⚠ **游戏存档结构**（博物、任务、背包…） | — |
| `CipherTool` | 配表 / 存档加解密 | ★★★ |

### 3.4 Lua（LuaCore）

| 组件 | 职责 | 成熟度 |
|------|------|--------|
| `LuaManager` | LuaEnv 生命周期、Loader 链、脚本执行 | ★★★ |
| `LuaBridge` | C# ↔ Lua 桥接（Talk / Wait / DebugLog） | ★★ |

**Lua 加载策略（阶段 0.3 整理后）：**

| 模式 | 预加载 | require / ExecuteLua |
|------|--------|----------------------|
| 热重载（Editor） | 跳过 `LoadLuaMapper` | Loader 1 / `GetLuaContent` 直读磁盘 |
| 正式包体 | `LoadLuaMapper` → `luaMapper` | Loader 3 查内存字典 |

### 3.5 关卡 / 流程（GameLevel）

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
              ├─ ShowPanel<MainPanel>()      ← ⚠ 硬编码
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
| 主界面 Panel | `ScenesManager.GoToMainScene()` → `MainPanel` | 硬编码游戏 UI |
| 主场景名 | `GoToMainScene()` → `"MainScene"` | 硬编码场景 |
| 存档数据结构 | `GameRuntimeData`（博物、任务、背包…） | 游戏专属字段 |
| 配表容器 | `RoleInfoContainer` | 某个 JRPG 的角色表 |
| Lua 桥接 | `LuaBridge.Talk` | 空实现，且写死在框架里 |
| 第三方绑定 | Wwise / Odin / URP 直接引用 | 换音频 / RP 需改 Core |

### 其他技术债

| 问题 | 位置 | 计划阶段 |
|------|------|----------|
| 无 namespace / asmdef | 全局命名空间；GameCore 已用 `CMGM.Core` | 阶段 1 进行中 |
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

### 目标目录（远期）

```
Packages/
  com.cmgm.core/              UPM 包：Core + 可选模块

Assets/_WorkSpace/              仅放本项目内容
  HotRes/  Excels/
  Scripts/
    Game/                     游戏专属代码
    Bootstrap/                模块注册、GameInitializer 子类
  Resources/CmgmFrameSettings.asset
```

---

## 7. 改造路线图

每步改动量可控，按顺序推进。**✅ = 已完成；待做 = 未开始。**  
已完成步骤**保留原文不删减**，只改状态列，便于回顾曾做过什么。

| 阶段 | 内容 | 状态 |
|------|------|------|
| **0** 清理 | 0.1 配表 bug（`RoleInfoContainer` 字典类型、`ExcelTool` 生成器、`CmgmLog.fError` 不受 LOG 开关影响） | ✅ |
| **0** 清理 | 0.2 移除 `Singleton.cs` 对 NUnit 的错误引用 | ✅ |
| **0** 清理 | 0.3 `LuaManager`：去掉启动打印全部 Lua 源码；Addressables Loader 去同步阻塞；热重载跳过 `LoadLuaMapper` | ✅ |
| **0** 清理 | 0.4 编写本文档 `ARCHITECTURE.md` | ✅ |
| **1** 编译边界 | 1.1 给 `GameCore` 下所有类加 `namespace CMGM.Core` | ✅ |
| **1** 编译边界 | 1.2 创建 `CMGM.Core.asmdef`；`Consts` partial 收拢至 GameCore（后为 1.2b 合并单文件取代） | ✅ |
| **1** 编译边界 | 1.2b 合并 `Consts` 为 `Consts.Paths.cs`；`WorkSpace` 迁入 `CmgmFrameSettings`（§2b A/C） | ✅ |
| **1** 编译边界 | 1.3 给 `GameUI`、`GameData`、`LuaCore` 加 namespace 并建 asmdef | 待做 |
| **1** 编译边界 | 1.4 给 `AudioSystem`、`GameInput`、`GameLevel` 加 namespace + asmdef | 待做 |
| **1** 编译边界 | 1.5 Editor 脚本单独 `CMGM.Editor.asmdef` | 待做 |
| **2** 框架/游戏分层 | 2.1 新建 `Scripts/Game/`，迁移 `MainPanel`、`SamplePanel` 等游戏 Panel | 待做 |
| **2** 框架/游戏分层 | 2.2 迁移 `GameRuntimeData` 及游戏专属存档字段至 `Scripts/Game/Data/` | 待做 |
| **2** 框架/游戏分层 | 2.3 迁移游戏配表（如 `RoleInfoContainer`）至 `Scripts/Game/Config/`；**B** 路径分层（§2b） | 待做 |
| **2** 框架/游戏分层 | 2.4 `ScenesManager` 去硬编码，改从 Settings / 接口读取主场景与主 Panel；**D** `AssetAddresses`（§2b） | 待做 |
| **2** 框架/游戏分层 | 2.5 新建 `Scripts/Game/Bootstrap/GameBootstrap.cs`，游戏专属初始化从 `GameInitializer` 拆出 | 待做 |
| **3** Bootstrap | 3.1 定义 `IGameModule` + `CmgmInitContext` | 待做 |
| **3** Bootstrap | 3.2~3.4 将各 Manager 改为 Module，构造函数不再做重活 | 待做 |
| **3** Bootstrap | 3.5 `GameInitializer` 改为按 Order 依次 await 注册模块 | 待做 |
| **3** Bootstrap | 3.6 游戏项目在 `GameBootstrap` 注册自己的 Module | 待做 |
| **4** 存档升级 | 4.1~4.6 分块存档、`ISaveChunk`、版本头、替换 `BinaryFormatter`、迁移示例 | 待做 |
| **5** GameState | 5.1~5.5 状态机基础态 + Pause/Cutscene/Battle 预留 | 待做 |
| **6** 事件总线 | 6.1~6.4 `IEventBus` 落地 OptionalSystem，替代一处直接调用 | 待做 |
| **7** 依赖抽象 | 7.1~7.4 `IAudioService`、`ILuaBridgeRegistry`、Odin 降级、URP 文档或抽象 | 待做 |
| **8** 内容扩展 | 8.1 对话；8.2 场景持久化；8.3 配表类型扩展；8.4 Lua 懒加载 + **E** 路径代码生成（§2b）；8.5 MUG；8.6 SRPG 接口 | 按需 |
| **9** 网游预埋 | 9.1~9.4 LocalSave vs ServerSync、网络层、战斗重放、Cloud save | 远期 |

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
GameRuntimeData（I_Saveable）
  → GameArchiveManager 序列化（当前 BinaryFormatter，待阶段 4 升级）
  → persistentDataPath/Archives/
```

### UI 管线

```
ShowPanel<T>() → Addressables 加载 HotRes/UI/Panels/{T}.prefab
  → 挂到对应 E_UILayer 层 Canvas
```

### Lua 管线

```
CmgmFrameSettings.ROOT_LUA_URI（如 main.lua.txt）
  → ExecuteLua → GetLuaContent / Loader 链
  → LuaBridge 暴露 C# API 给 Lua
  → util.async_to_sync 包装异步回调
```

---

## 9. 文档维护

- 每完成一步，更新 **§7 路线图** 对应行的**状态列**（改为 ✅），**不要合并或删减已完成步骤的描述**
- 更新 **§3 模块清单**、**§5 耦合点**（如有变化）
- 新增模块时在 §3 登记；发现新耦合点在 §5 补充
- 重大架构决策记录在对应阶段的小节或 PR 描述中

---

*下一步：阶段 1.3 — `GameUI` / `GameData` / `LuaCore` 加 namespace 与 asmdef*
