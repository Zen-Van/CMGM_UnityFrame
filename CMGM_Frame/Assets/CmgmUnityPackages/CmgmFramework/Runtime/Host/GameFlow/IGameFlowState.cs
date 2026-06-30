using Cysharp.Threading.Tasks;

namespace CMGM.GameFlow
{
    /// <summary>
    /// 游戏宏观流程态（<c>CmgmInit</c> / <c>MainMenu</c> / <c>Gameplay</c> …）。
    /// </summary>
    public interface IGameFlowState
    {
        string StateName { get; }

        /// <summary>进入态；<see cref="GameFlowMachine.SwitchToAsync"/> 会 await 此方法（#7 起支持异步 Loading / 切场景）。</summary>
        UniTask EnterAsync();

        void Exit();

        /// <summary>可选帧更新；默认空实现见 <see cref="GameFlowStateBase"/>。</summary>
        void Update(float deltaTime);
    }
}
