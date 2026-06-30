【GameFlowState 文件夹】

放这一作游戏自己的「宏观流程态」脚本，例如 GameplayState、WorldMapState。

和框架 Bootstrap/GameFlow 里的 CmgmInitState、MainMenuState 不一样：
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

GameplayState.cs   — 进游戏（#8 会接到 MainPanel）
WorldMapState.cs     — 同态换区 TravelTo 样板（#8 之后、有大区域需求时再填 Task）

配表、Loading 步骤 → LoadTasks 文件夹（流式/区块加载说明也在那边）。
