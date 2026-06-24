# 业务层工作区说明（Workspace · 本项目）



> **本文档随游戏项目**，描述 `_WorkSpace` 里「这一作」的约定与目录。  

> **框架**架构、迭代计划见 `Assets/CmgmUnityPackages/ARCHITECTURE.md`。



---



## 1. 与本工程其它根目录的关系



| 目录 | 角色 |

|------|------|

| **`Assets/CmgmUnityPackages/CmgmFramework/`** | **Resources** + **Editor** + **Runtime**（Settings、UI、代码） |

| **`Assets/_WorkSpace/`** | **业务层（Workspace）**（本文档）：脚本、HotRes、Excels |

| **`Assets/_TestSpace/`** | **测试层** |



换项目时：复制 `CmgmUnityPackages` + 通过「▶ 项目初始化」生成 `_WorkSpace`；框架 Resources 随包，无需在工作区重复一份 Settings。



---



## 2. `_WorkSpace` 目录（当前）



```

_WorkSpace/

├── GAME_WORKSPACE.md

├── Excels/

├── HotRes/

│   ├── Lua/

│   ├── Scenes/

│   ├── UI/Panels/

│   └── RhythmMap/              Demo 专有

└── Scripts/

    ├── Bootstrap/              GameBootstrap

    ├── UI/Panels/

    ├── Archive/

    └── _Generated/

        └── Config/               Excel 导表 Container（勿手改）

```



**框架配置与内置资源：** `CmgmUnityPackages/CmgmFramework/Resources/`（含 `CmgmFrameSettings.asset`）。



路径常量：`Consts.Paths.WorkSpaceScripts.*`、`Consts.Paths.Framework.Resources`。



---



## 3. 命名与程序集



| 项 | 约定 |

|----|------|

| **namespace** | 业务层脚本默认 `CMGM.Workspace` |

| **asmdef** | 本 Demo 业务脚本在默认 `Assembly-CSharp` |



---



## 4. 启动与加载分工



| 阶段 | 负责 | 入口 |

|------|------|------|

| Logo → 主界面 | 框架 Boot | `CmgmFrameBoot` |

| 主界面 → 进游戏 | 业务 Boot | `GameBootstrap.EnterGameplayAsync()` |



Boot 参数（主场景名、主 Panel 名、工作区根路径等）在 **`CmgmFramework/Resources/CmgmFrameSettings.asset`** 编辑。



---



## 5. 配表与存档



| 类型 | 目录 |

|------|------|

| Excel 源 | `Excels/` |

| Container 脚本 | `Scripts/_Generated/Config/`（勿手改） |

| 存档结构 | `Scripts/Archive/` |



---



## 6. UI



- Panel 脚本：`Scripts/UI/Panels/`

- Panel Prefab：`HotRes/UI/Panels/`

- 框架 UI 摄像机/Canvas：`CmgmFramework/Resources/UI/`



---



## 7. 新建游戏项目时建议修改



1. `CmgmFramework/Resources/CmgmFrameSettings.asset` — 工作区根、主场景/主 Panel 名等

2. 扩展 `Scripts/`、`Excels/`、`HotRes/`

3. 更新本 `GAME_WORKSPACE.md` 的 Demo 专有说明



---



## 8. 文档索引



| 文档 | 位置 |

|------|------|

| 框架架构 | `CmgmUnityPackages/ARCHITECTURE.md` |

| 业务层（Workspace） | `_WorkSpace/GAME_WORKSPACE.md` |

