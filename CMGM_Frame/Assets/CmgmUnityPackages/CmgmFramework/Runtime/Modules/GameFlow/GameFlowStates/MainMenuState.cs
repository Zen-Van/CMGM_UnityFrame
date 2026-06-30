using CMGM.Core;
using CMGM.Data;
using CMGM.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CMGM.GameFlow
{
    /// <summary>
    /// 宏观态 <c>MainMenu</c>（#7）：进主界面场景 + 主 Panel。
    /// </summary>
    public sealed class MainMenuState : GameFlowStateBase
    {
        public MainMenuState() : base("MainMenu") { }

        public override async UniTask EnterAsync()
        {
            var settings = CmgmFrameSettings.Instance;

            ArchiveManager.Instance.ClearRuntimeData();
            UIManager.Instance.ClearPanel();

            await UIManager.Instance.ShowPanel(settings.MAIN_PANEL_NAME);
            await AddressablesResMgr.Instance.LoadSceneAsync(settings.MAIN_SCENE_NAME);
            UIManager.Instance.SetUICameraOverlap(Camera.main);
        }
    }
}
