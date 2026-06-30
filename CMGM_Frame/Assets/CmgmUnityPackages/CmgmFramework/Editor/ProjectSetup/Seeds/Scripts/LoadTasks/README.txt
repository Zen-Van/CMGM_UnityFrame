【LoadTasks 文件夹】

放游戏专属的 Loading 步骤（一个个 ILoadTask 类）。

框架 Loading/Tasks 里已经有通用的，比如 ManagerInitLoadTask 用来 Init 各种 Manager。
业务这边也尽量用「一个通用类 + 泛型」，别每张表、每个场景都新建一个 cs。

────────────────────────────────
TableLoadTask<T>

预加载某张配表，和 ManagerInitLoadTask<T> 是一个思路：

  new TableLoadTask<RoleInfo>(),
  new TableLoadTask<别的Container>(),

表多了就在 GameplayState 的 CreateTasks() 里多写几行，不用新建 Task 文件。

────────────────────────────────
什么时候才新建一个 Task 类？

这一步和「Init 某个 Manager」「Load 某张表」都不一样时再写。
清单写在 GameFlowState 各态的 CreateTasks()（或 TravelTo 里的 CreateTravelTasks）里，
不要写在 Panel 里。

────────────────────────────────
区域切换 / TravelTo

SRPG 式「换地图、进传送门」一类：流程在 GameFlowState 的 WorldMapState.TravelTo，
栈顶不变，一次 RunAsync + 进度条。

本文件夹可加带 regionId / 场景名的 Task，在 CreateTravelTasks 里组装，例如：
  · 卸载上一块区域（可选）
  · LoadScene（单场景或 Additive）
  · 该区域的 TableLoadTask / Addressables 预载

表仍用 TableLoadTask<T>；整段换区可做成 LoadWorldRegionTask(regionId) 一类，不要每个区域一个 cs。

参考：../GameFlowState/WorldMapState.cs

────────────────────────────────
流式加载 / 区块卸载（远期拓展）

指开放世界里「走路时后台加载、离开视野卸载」——和上面 TravelTo 的一次性读条不是一回事。
TravelTo = 玩家触发的 blocking 事务；流式 = 常驻逻辑按距离/区块反复 Load / Unload。

本框架仍用同一套 ILoadTask + LoadingManager，以后好拓展：
  · 单次读条：State.TravelTo 或传送 → RunAsync(tasks, 显示进度条)
  · 流式：业务层常驻 Streaming 服务（名字未定），每帧/定时根据玩家位置
    → RunAsync(静默清单) 或单独调 StreamChunkLoadTask / StreamChunkUnloadTask

Task 仍建议放在本文件夹，例如：
  · StreamChunkLoadTask(chunkId)   — 加载一个区块的资源或 Additive 场景
  · StreamChunkUnloadTask(chunkId) — 卸载、释放 Addressables
  · 内存预算、邻块预测等放在 Streaming 服务里，不要塞进 Panel

现在不用实现；TableLoadTask、ManagerInitLoadTask 和 RunAsync 机制已经够用，后面只加 Task 类和服务即可。

────────────────────────────────
现有文件

TableLoadTask.cs          — 配表预加载样板
EnterGameplayLoadTask.cs  — 进游戏（场景 / Bank 等）
