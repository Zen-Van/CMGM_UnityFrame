# CmgmUnityPackages

可跨项目拷贝的 CMGM 框架包根目录（**项目脚手架1.1 占位**）。

- **当前**：代码仍在 `Assets/_WorkSpace/Scripts/`；本目录仅记录目标位置。
- **目标（项目脚手架1.4，须在启动组合根3.2 后）**：将 Framework / GameKits 迁入子文件夹，游戏层留在 `_WorkSpace`。

| 子目录 | 将来迁入内容 |
|--------|----------------|
| `CmgmFramework/` | 原 `_WorkSpace/Scripts/Framework`（Core、Modules、Integrations、Editor、Bootstrap） |
| `CmgmGameKits/` | 原 `_WorkSpace/Scripts/CmgmGameKits`（可选工具包） |

详见 `Assets/_WorkSpace/ARCHITECTURE.md` §2.2、§7.4「项目脚手架与包体迁移」。
