namespace CMGM.GameFlow
{
    /// <summary>
    /// 游戏宏观流程态（<c>Startup</c> / <c>MainMenu</c> / <c>Gameplay</c> …）。
    /// </summary>
    public interface IGameFlowState
    {
        string StateName { get; }

        void Enter();

        void Exit();

        /// <summary>可选帧更新；默认空实现见 <see cref="GameFlowStateBase"/>。</summary>
        void Update(float deltaTime);
    }
}
