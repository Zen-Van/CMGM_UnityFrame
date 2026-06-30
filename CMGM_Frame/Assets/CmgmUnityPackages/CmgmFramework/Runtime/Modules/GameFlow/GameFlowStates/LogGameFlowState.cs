using CMGM.Core;
using Cysharp.Threading.Tasks;

namespace CMGM.GameFlow
{
    /// <summary>
    /// 1.1 验收 / 调试用态：EnterAsync / Exit 打日志。正式宏观态见 <see cref="CmgmInitState"/> / <see cref="MainMenuState"/>。
    /// </summary>
    public sealed class LogGameFlowState : GameFlowStateBase
    {
        public LogGameFlowState(string stateName) : base(stateName) { }

        public override UniTask EnterAsync()
        {
            CmgmLog.fPositive($"[GameFlow] Enter {StateName}");
            return UniTask.CompletedTask;
        }

        public override void Exit()
        {
            CmgmLog.fNormal($"[GameFlow] Exit {StateName}");
        }
    }
}
