using CMGM.Core;

namespace CMGM.GameFlow
{
    /// <summary>
    /// 1.1 验收 / 调试用态：Enter / Exit 打日志。正式宏观态在 1.3 起替换为具名 State 类。
    /// </summary>
    public sealed class LogGameFlowState : GameFlowStateBase
    {
        public LogGameFlowState(string stateName) : base(stateName) { }

        public override void Enter()
        {
            CmgmLog.fPositive($"[GameFlow] Enter {StateName}");
        }

        public override void Exit()
        {
            CmgmLog.fNormal($"[GameFlow] Exit {StateName}");
        }
    }
}
