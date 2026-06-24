namespace CMGM.GameFlow
{
    /// <summary>
    /// 单槽流程上下文：Replace 触发 Exit → Enter。1.2 由 <see cref="GameFlowMachine"/> 扩展为栈式状态机。
    /// </summary>
    public sealed class GameFlowContext
    {
        public IGameFlowState Current { get; private set; }

        public void SwitchTo(IGameFlowState next)
        {
            if (ReferenceEquals(Current, next))
                return;

            Current?.Exit();
            Current = next;
            Current?.Enter();
        }

        public void Tick(float deltaTime)
        {
            Current?.Update(deltaTime);
        }
    }
}
