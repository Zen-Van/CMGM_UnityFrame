using CMGM.Core;
using CMGM.GameFlow;
using CMGM.UI;
using CMGM.Workspace.GameFlowState;
using Cysharp.Threading.Tasks;

namespace CMGM.Workspace
{
    public class MainPanel : BasePanel
    {
        protected override void OnButtonClick(string btnName)
        {
            switch (btnName)
            {
                case "btnStart":
                    GameFlowMachine.Instance.SwitchToAsync(new GameplayState()).Forget();
                    break;
                case "btnLoad":
                    // 竖切占位：演示 Lua 剧情（story/demo_intro.lua.txt）
                    LuaManager.Instance.ExecuteLua("story/demo_intro.lua.txt").Forget();
                    break;
                case "btnQuit":
                    CmgmApplication.Quit();
                    break;
            }
        }
    }
}
