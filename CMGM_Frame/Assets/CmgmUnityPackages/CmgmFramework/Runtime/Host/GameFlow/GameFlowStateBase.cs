using Cysharp.Threading.Tasks;

namespace CMGM.GameFlow
{
    /// <summary>
    /// <see cref="IGameFlowState"/> 默认实现：空 <see cref="EnterAsync"/> / <see cref="Exit"/> / <see cref="Update"/>。
    /// </summary>
    public abstract class GameFlowStateBase : IGameFlowState
    {
        protected GameFlowStateBase(string stateName)
        {
            StateName = stateName;
        }

        public string StateName { get; }

        public virtual UniTask EnterAsync() => UniTask.CompletedTask;

        public virtual void Exit() { }

        public virtual void Update(float deltaTime) { }
    }
}
