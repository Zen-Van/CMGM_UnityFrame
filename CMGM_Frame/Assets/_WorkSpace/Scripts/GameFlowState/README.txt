【GameFlowState 文件夹】

放这一作游戏自己的「宏观流程态」脚本，例如 GameplayState、WorldMapState。

和框架 `Modules/GameFlow/GameFlowStates/` 里的 CmgmInitState、MainMenuState 不一样：
那些是启动、进主菜单，几乎每个项目都差不多；
这里是你游戏的玩法阶段，换项目会整批换掉。

────────────────────────────────
两种用法（记一个就行）

1）换大阶段 → SwitchToAsync
   例：主菜单点「开始」→ new GameplayState()
   Loading 写在 State 的 EnterAsync + CreateTasks() 里。

2）同一大阶段里换区域 → 不要 SwitchTo，调 TravelTo
   例：探索态进传送门 → WorldMapState.TravelTo("World_B")
   栈顶还是 WorldMap，跑一遍 Loading 换场景（一次性读条，不是流式加载）。
   见 WorldMapState.cs 里的空壳。

Panel 只负责「我想去哪」：SwitchTo 或调当前态的 TravelTo。
不要在 Panel 里自己拼 LoadingManager.RunAsync。

────────────────────────────────
现有文件

GameplayState.cs   — 竖切接线（MainPanel → SwitchToAsync + Loading）；非完整「进游戏」
WorldMapState.cs     — 同态换区 TravelTo 样板（有大区域需求时再填 Task）

────────────────────────────────
进游戏 / 读档（业务层 · 暂不实现）

「开始游戏」可能是新建或读档，初始场景、Panel、Bank 因项目而异。
不在框架 CmgmFrameSettings 里配玩法场景名；定稿后在本文件夹 LoadTasks + 各 State 里按项目配置组装。
当前 EnterGameplayLoadTask 仅为占位，等业务需求明确再填。
