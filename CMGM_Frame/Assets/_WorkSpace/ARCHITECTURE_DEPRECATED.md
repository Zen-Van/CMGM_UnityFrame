# CMGM 框架架构 · 过时归档

> **用途：** 存放从 `ARCHITECTURE.md` 迁出的**已废弃 / 已过时**内容，仅供历史回顾，**不代表当前有效计划**。
> 当前有效计划请看 `ARCHITECTURE.md`。
> **规则：** 后续凡有内容废弃，迁入本文档并标注**废弃日期 + 时分（UTC+8）**。

---

## 归档索引

| 归档块 | 原位置 | 废弃原因 | 废弃时间（UTC+8） |
|--------|--------|----------|-------------------|
| A. 旧线性路线图（0→9） | 原 `ARCHITECTURE.md` §7.0b / §7.1 / §7.2 | 改为「主线任务 + 支线任务」并行模型 | 2026-06-19 04:28 |
| B. 废止计划附录（`*原1.x` 等） | 原 `ARCHITECTURE.md` §7.4 | 随旧路线图一并归档（本就是废弃记录） | 2026-06-19 04:28 |
| C. Lua 独立 asmdef 方案（`CMGM.Lua` + XLua asmdef 版） | 原 `ARCHITECTURE.md` §3.4 / §7.0 / §7.2「编译边界2.6」 | XLua 官方 `feature/asmdef` 已被 Revert；改方案 C：Lua 不建 asmdef、契约入 Core、实现入 `Integrations`、XLua 退官方 master | 2026-06-19 06:09 |

> **说明：** 原 §7.5（2026-06-19 Loading/Scene 决策）**未废弃**，其结论已并入新 `ARCHITECTURE.md` 的主线/支线计划，故不归档于此。

---

# 归档块 A · 旧线性路线图（0→9）

> **废弃时间：2026-06-19 04:28（UTC+8）**
> 原为单一线性路线图（阶段 0→9 连续编号）。已被「主线任务（线性地基）+ 支线任务（解锁后并行）」模型取代。
> 已完成步骤（0、1、2.1~2.5b）的事实记录在新文档「已完成基线」中以浓缩形式保留。

### 旧 §7.0b 子编号说明（2.x / 2.xa / 2.xb）

主表里的 **2.1、2.2…** 是阶段内主步骤；**字母后缀不是按 a→z 的执行顺序**，而是按**类型**区分。

| 编号形式 | 含义 | 示例 |
|----------|------|------|
| **2.x** | 主步骤：迁移、配置化、Bootstrap 等 | 2.1 迁 Panel ✅；2.2 迁存档 |
| **2.xa** | **目录 / 布局**（仅 2.1a）：`Framework/` 收拢 + 去 `Game*` 前缀 | 在 **2.1b** 之前做 |
| **2.xb** | **M 模块闭环**：namespace + asmdef | 2.1b=M1 UI；2.3b=M2 Data；2.5b=M3 Scene… |
| **Mx** | 与 **2.xb** 同义，闭环索引用 | M1↔2.1b |

> 曾用编号 **2.1c** 指目录步骤，已改为 **2.1a**。

### 旧 §7.1 路线图明细（含状态）

| 阶段 | 内容 | 状态 |
|------|------|------|
| **0** 清理 | 0.1 配表 bug（`RoleInfoContainer` 字典类型、`ExcelTool` 生成器、`CmgmLog.fError` 不受 LOG 开关影响） | ✅ |
| **0** 清理 | 0.2 移除 `Singleton.cs` 对 NUnit 的错误引用 | ✅ |
| **0** 清理 | 0.3 `LuaManager`：去掉启动打印全部 Lua 源码；Addressables Loader 去同步阻塞；热重载跳过 `LoadLuaMapper` | ✅ |
| **0** 清理 | 0.4 编写本文档 `ARCHITECTURE.md` | ✅ |
| **1** 编译边界 | 1.1 给 `GameCore` 下所有类加 `namespace CMGM.Core` | ✅ |
| **1** 编译边界 | 1.2 创建 `CMGM.Core.asmdef`；`Consts` partial 收拢至 GameCore（后为 1.2b 合并单文件取代） | ✅ |
| **1** 编译边界 | 1.2b 合并 `Consts` 为 `Consts.Paths.cs`；`WorkSpace` 迁入 `CmgmFrameSettings`（§2b A/C） | ✅ |
| **1** 编译边界 | 1.3 路线图修订：采用「按模块闭环」；非 Core 的 asmdef 并入阶段 2 闭环步骤 | ✅ |
| **2** 框架/游戏分层 | **2.1** 新建 `Scripts/Game/`，迁移 `MainPanel`、`SamplePanel`、`zzzTextPanel` 等游戏 Panel | ✅ |
| **2** 框架/游戏分层 | 2.1a 新建 `Framework/Core/`、`Framework/Modules/`；Runtime 迁入 + 去 `Game*` 前缀 | ✅ |
| **2** 框架/游戏分层 | 2.1b **M1 闭环**：`CMGM.UI` + `.Editor` asmdef；`namespace CMGM.UI`；Game 层仅目录 + `CMGM.Game` namespace（无框架级 Game asmdef）；`CmgmApplication.Quit` | ✅ |
| **2** 框架/游戏分层 | 2.2 迁移 `GameRuntimeData` 等至 `Scripts/Game/Archive/` | ✅ |
| **2** 框架/游戏分层 | 2.3 迁移游戏配表（如 `RoleInfoContainer`）至 `Scripts/Game/Config/`；**B** `Paths.Framework` / `Paths.Game` | ✅ |
| **2** 框架/游戏分层 | 2.3b **M2 闭环**：`CMGM.Data` + `CMGM.Data.Editor` asmdef | ✅ |
| **2** 框架/游戏分层 | 2.4 `ScenesManager` 配置化；`MAIN_SCENE_NAME` + `MAIN_PANEL_NAME` | ✅ |
| **2** 框架/游戏分层 | 2.5 `GameBootstrap`：游戏**进游戏**加载入口（`EnterGameplayAsync`）；Logo 链不载大表 | ✅ |
| **2** 框架/游戏分层 | 2.5b **M3 闭环**：`Scene` 模块 `CMGM.Scene` asmdef（`ScenesManager`；Boot 待 3.5 → `CMGM.Bootstrap`） | ✅（asmdef 部分已于 2026-06-19 撤销，见新文档） |
| **2** 框架/游戏分层 | **2.5c** Scene · 进游戏 Loading（`ScenesManager` + LoadingPanel） | 重排为支线「Loading系统」 |
| **2** 框架/游戏分层 | 2.6 **M4 闭环**：`Lua` 模块 `CMGM.Lua` asmdef（与 XLua Generate Code 同单） | 重排为主线「编译边界2.6」 |
| **2** 框架/游戏分层 | 2.7 **M5 闭环**：`Audio` + `Input` 模块 asmdef | 重排为主线「编译边界2.7」 |
| **2** 框架/游戏分层 | 2.8 **M6 闭环**：Editor namespace + asmdef | 重排为主线「编译边界2.8」 |
| **3** Bootstrap | 3.1 定义 `IGameModule` + `CmgmInitContext`（放 **Core**，无 Module 引用） | 重排为主线「启动组合根3.1」 |
| **3** Bootstrap | 3.2~3.4 将各 Manager 改为 Module，构造函数不再做重活 | 并入主线「启动组合根3.1」 |
| **3** Bootstrap | 3.5 `CmgmFrameBoot` 迁至 `Framework/Bootstrap/`（**`CMGM.Bootstrap`** 组合根）；按 Order await 注册模块 | 重排为主线「启动组合根3.2」一部分 |
| **3** Bootstrap | 3.6 游戏项目在 `GameBootstrap` 注册自己的 Module | 同上 |
| **4** 存档升级 | 4.1~4.6 分块存档、`ISaveChunk`、版本头、替换 `BinaryFormatter`、迁移示例 | 重排为支线「存档升级系统」 |
| **5** GameState | 5.1~5.5 状态机基础态 + Pause/Cutscene/Battle 预留 | 重排为支线「GameState系统」 |
| **6** 事件总线 | 6.1~6.4 `IEventBus` 落地 OptionalSystem，替代一处直接调用 | 重排为支线「事件总线系统」 |
| **7** 依赖抽象 | 7.1 `IAudioService` → **Audio系统1.1**；7.2 `ILuaBridgeRegistry` → **Lua系统1.1**；7.3 Odin 降级 → **依赖抽象系统1.1**；7.4 URP/RP 抽象 → **依赖抽象系统1.2** | 重排为支线（拆入相关系统 + 依赖抽象系统） |
| **7** 依赖抽象 | 7.5 `CmgmModuleManifest`：模块 id、Core/Modules 分级、依赖链 | 重排为主线「启动组合根3.2」 |
| **8** 内容扩展 | 8.1 对话；8.2 场景持久化；8.3 配表类型扩展；8.4 Lua 懒加载 + **E** 路径代码生成；8.5 MUG；8.6 SRPG 接口 | 重排为支线「内容扩展」 |
| **8** 内容扩展 | 8.7 Editor 模块导入向导 | 重排为主线/Editor 相关 |
| **8.K** **GameKits** | `CmgmGameKits`：`CMGM.GameKits` asmdef、RoleControl / MapTriggers / Camera 等模板 | 重排为支线「GameKits」 |
| **9** 网游预埋 | 9.1~9.4 LocalSave vs ServerSync、网络层、战斗重放、Cloud save | 重排为支线「网游预埋」 |

### 旧 §7.2 九阶段详细任务表

下表是旧 §7.1 的展开版，便于排期与验收。**仅历史参考。**

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
| 1.3 | 路线图修订：「按模块闭环」；非 Core 的 asmdef 并入阶段 2 之 2.1b~2.8 | 本文档 §7 | 主表连续 0→9 |

#### 阶段 2 · 框架 / 游戏分层 + 模块 asmdef

| 编号 | 任务 | 详细内容 | 验收 |
|------|------|----------|------|
| **2.1** ✅ | 迁出游戏 Panel | `Scripts/Game/UI/Panels/` | 主界面 Play 正常 |
| **2.1a** ✅ | Framework 目录 + 重命名 | §6.3 映射；`Consts.Paths.Framework.*` | 编译通过 |
| **2.1b M1** ✅ | UI 模块闭环 | `CMGM.UI`、`CMGM.UI.Editor` asmdef；`CmgmApplication.Quit`；Game 层 namespace 保留，**不建** `CMGM.Game` asmdef | ShowPanel / HidePanel 正常 |
| **2.2** ✅ | 迁出游戏存档结构 | `GameRuntimeData` → `Scripts/Game/Archive/`；`I_Saveable` → `Framework/Modules/Data/Archive/`；`ArchiveManager` 通用读写 | 读档 / 存档流程不变 |
| **2.3** ✅ | 迁出游戏配表 | `RoleInfoContainer` → `Scripts/Game/Config/`；`ExcelTool` 输出至 `Paths.Game.Config`；生成类带 `namespace CMGM.Game` | Editor 导表 + `LoadTable<RoleInfo>()` 正常 |
| **2.3b M2** ✅ | Data 模块闭环 | `CMGM.Data` + `CMGM.Data.Editor` asmdef；`namespace CMGM.Data` / `CMGM.Data.Editor`；Editor 收拢至 `Data/Editor/` | 编译 + 导表 + 存档 Init |
| **2.4** ✅ | ScenesManager 配置化 | `CmgmFrameSettings` 配置主场景 + 主 Panel；`GoToMainScene` 用字符串 `ShowPanel` / `LoadSceneAsync` | 换主 UI/主场景只改 Settings |
| **2.5** ✅ | GameBootstrap | `EnterGameplayAsync`：主界面→进游戏时加载配表/资源/音频；**不在** Logo→主界面链 | MainPanel「开始」可触发 |
| **2.5b M3** ✅ | Scene 模块闭环 | `CMGM.Scene` asmdef + `namespace CMGM.Scene`（目录 `Modules/Scene/`；曾用 Level）；Boot 暂留默认程序集 | Logo → 框架 Init → 主场景 |
| **2.5c** | Scene · 进游戏 Loading | `ScenesManager` 扩展 + LoadingPanel；编排 `EnterGameplayAsync`、Addressables/切场景进度 | 主界面→进游戏有进度条 |
| **2.6 M4** | Lua 模块闭环 | `CMGM.Lua` + XLua 同单 | Lua 启动、`require`、C# 桥接无类型分裂错误 |
| **2.7 M5** | Audio + Input 闭环 | `CMGM.Audio`、`CMGM.Input` asmdef | 音频事件、输入 map 正常 |
| **2.8 M6** | Editor 模块闭环 | `_WorkSpace/Editor/` → `Framework/Editor/`；各 `Modules/*/Editor/` + `CMGM.Editor` asmdef | 路径检查、模板、导入向导可用 |

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
| 5.3 | 基础态 | `Boot`、`MainMenu`、`Gameplay`、**`Loading`**（态内调用进游戏 Loading + `GameBootstrap.EnterGameplayAsync`） | 与 ScenesManager 协作 |
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
| **M3** | 2.5b | Scene（`CMGM.Scene`，asmdef 部分已撤销） |
| **M4** | 2.6 | Lua（`CMGM.Lua`） |
| **M5** | 2.7 | Audio + Input |
| **M6** | 2.8 | Editor asmdef（最后） |
| **K** | 8.K | CmgmGameKits（低优先级，框架完成后独立补充） |

#### 阶段 8.K · CmgmGameKits

| 编号 | 任务 | 详细内容 | 验收 |
|------|------|----------|------|
| **8.K1** | 目录与 asmdef | `Scripts/CmgmGameKits/`；`CMGM.GameKits` asmdef；`namespace CMGM.GameKits` | 引用 Framework Modules 编译通过 |
| **8.K2** | RoleControl | 2D / 3D 通用角色控制器模板 | 示例场景可跑 |
| **8.K3** | MapTriggers | 可继承的场景触发器基类 + 常用变体 | 与 Scene `ScenesManager` 切场景无耦合 |
| **8.K4** | Camera | 跟随 / 边界等相机控制（按需） | 可选 |
| **8.K5** | 发布 | 与框架同仓库或同 UPM 包组；新项目可整包删除 | 文档 §6.4 |

---

# 归档块 B · 废止计划附录（`*原1.x` 等）

> **废弃时间：2026-06-19 04:28（UTC+8）**
> 原 `ARCHITECTURE.md` §7.4。记录已放弃的旧编号与决策背景。
> **标记：** `*原1.x` = 已废止的原阶段 1 编号（`*` 前缀 + `原`）。

**背景：** 原策略为「阶段 1 先给全部模块加 asmdef → 阶段 2 再分层」。该策略在 `*原1.3` 实践与回退后废止；`*原1.4`、`*原1.5` 未单独执行。有效 **1.3** 改为「按模块闭环」，asmdef 工作并入旧 §7.1 之 **2.1b~2.8**。

**记录时间：** 2026-06-16 08:27（UTC+8）

| 废止编号 | 原内容 | 结果 | 工作并入（旧 §7.1 有效编号） |
|----------|--------|------|------------------------------|
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
| ~~独立 `Modules/Loading` + 2.9 M7~~（曾延后，2026-06-19 复活为支线「Loading系统」） | — | — | — |
| 框架层 `Game*` 类名 | **2.5 起废止**新命名 | — | `CmgmFrameBoot`、`ArchiveManager`、`ConfigTableManager`；游戏层保留 `GameBootstrap` 等 |

**记录时间：** 2026-06-15（`I_Saveable` 提前迁移；`CMGM.Game` asmdef 移除）；2026-06-16（2.4 废止 GameFlow；Loading 不单独建 Modules；框架层去 Game 命名）

---

# 归档块 C · Lua 独立 asmdef 方案（`CMGM.Lua` + XLua asmdef 版）

> **废弃时间：2026-06-19 06:09（UTC+8）**
> 被新 `ARCHITECTURE.md` §7.5「XLua / Lua 模块定位」决策（方案 C）取代。

**曾经的做法（已废弃）：**

| 项 | 旧方案 |
|------|--------|
| XLua 版本 | 官方 `feature/asmdef` 分支（含 `Xlua.Core.asmdef` / `Xlua.Core.Editor.asmdef`） |
| Lua 模块程序集 | 新建 `CMGM.Lua.asmdef`（references `CMGM.Core`、`Xlua.Core`）+ `CMGM.Lua.Editor.asmdef` |
| 命名空间 | `LuaManager` → `namespace CMGM.Lua`；`Edt_LuaSuffConverter` → `CMGM.Lua.Editor`；`LuaBridge` 保留全局 |
| 旧「编译边界2.6」定义 | 「`CMGM.Lua` asmdef + XLua Generate Code 同单」 |
| 旧 §7.0 约定 | 「XLua 与 asmdef 同单：若给 XLua 建 asmdef，须把 `Gen/` 纳入同一程序集或重配 Generate Code」 |

**废弃原因：** XLua 官方 `feature/asmdef` 提交已被 Revert（PR#1067 加入、PR#1068/commit d919198 撤销），master 不再含这些 asmdef。继续用等于绑死「官方已回滚版本」，且后续 hotfix / wrap 生成受 asmdef 约束。改为**方案 C**：Lua 不建 asmdef，契约 `ILuaService` 入 `CMGM.Core`，实现入 `Framework/Integrations/Lua/`（随 XLua master 落 `Assembly-CSharp`）。详见 `ARCHITECTURE.md` §3.4 / §7.5。

> 待执行的代码迁移：删除 `CMGM.Lua` / `CMGM.Lua.Editor` 两个 asmdef；`LuaManager`/`LuaBridge` 迁出 `Modules/Lua` 至 `Integrations/Lua`；Core 加 `ILuaService`；Boot 注册。
