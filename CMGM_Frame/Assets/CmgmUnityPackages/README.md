# CmgmUnityPackages

可跨项目拷贝的 CMGM 框架包根目录（**项目脚手架1.1** ✅ 占位）。

- **当前**：代码仍在 `Assets/_WorkSpace/Scripts/`；本目录仅记录目标位置。
- **目标（项目脚手架1.4）**：将 `Framework` 迁入 `CmgmFramework/`；可选将 `Scripts/CmgmGameKits` 迁入 `CmgmGameKits/`；游戏层留在 `_WorkSpace`。

| 子目录 | 将来迁入内容 |
|--------|----------------|
| `CmgmFramework/` | 原 `_WorkSpace/Scripts/Framework`（Core、Modules、Integrations、Editor、Bootstrap） |
| `CmgmGameKits/` | 原 `_WorkSpace/Scripts/CmgmGameKits`（非框架 WIP 目录，可选） |

详见 `Assets/_WorkSpace/ARCHITECTURE.md` §2.2、§7.4「项目脚手架与包体迁移」。
