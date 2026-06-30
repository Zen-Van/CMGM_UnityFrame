using CMGM.GameFlow;
using CMGM.Scene;
using Cysharp.Threading.Tasks;

namespace CMGM.Bootstrap.GameFlow
{
    /// <summary>
    /// 宏观态 <c>MainMenu</c>（#7）：进主界面场景 + 主 Panel。
    /// </summary>
    public sealed class MainMenuState : GameFlowStateBase
    {
        public MainMenuState() : base("MainMenu") { }

        public override UniTask EnterAsync()
        {
            return ScenesManager.Instance.GoToMainScene();
        }
    }
}
