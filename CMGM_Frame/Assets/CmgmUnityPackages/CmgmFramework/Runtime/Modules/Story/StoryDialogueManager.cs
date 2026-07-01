using System;
using CMGM.Core;
using CMGM.UI;
using Cysharp.Threading.Tasks;

namespace CMGM.Story
{
    /// <summary>
    /// Lua <c>Talk</c> 编排：<see cref="UIManager.ShowPanelAtAddress{T}"/> → <see cref="DialogPanel.PrintContent"/> → Hide → callback。
    /// </summary>
    public static class StoryDialogueManager
    {
        public static void Talk(int roleId, int imgId, string content, Action callback)
        {
            UniTask.Void(async () =>
            {
                try
                {
                    if (!UIManager.IsReady)
                    {
                        CmgmLog.fError("[StoryDialogueManager] UIManager 尚未 InitAsync，无法显示对话。");
                        return;
                    }
                    var panel = await UIManager.Instance.ShowPanelAtAddress<DialogPanel>(DialogPanel.AddressKey, E_UILayer.Top);
                    if (panel == null)
                    {
                        CmgmLog.fError("[StoryDialogueManager] DialogPanel 加载失败（Addressables：" + DialogPanel.AddressKey + "）。");
                        return;
                    }
                    await panel.PrintContent(roleId, imgId, content);
                }
                finally
                {
                    if (UIManager.IsReady)
                        UIManager.Instance.HidePanel(nameof(DialogPanel), isDestroy: false);
                    callback?.Invoke();
                }
            });
        }
    }
}
