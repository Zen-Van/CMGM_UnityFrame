using CMGM.UI;
using Cysharp.Threading.Tasks;
using TMPro;

namespace CMGM.Story
{
    /// <summary>
    /// 剧情对话面板（<c>Modules/Story/Panels/DialogPanel.prefab</c>，随 CmgmUnityPackages 迁移）。
    /// <para>控件约定：<c>txtContent</c>、<c>btnNext</c>；配表 / 立绘后续在 <see cref="PrintContent"/> 内扩展。</para>
    /// </summary>
    public class DialogPanel : BasePanel
    {
        /// <summary>Addressables 加载键；须 Mark <c>Runtime/Modules/Story/Panels</c>，Address = <c>Story/Panels</c>，Label <c>UI</c>。</summary>
        public const string AddressKey = "Story/Panels/DialogPanel.prefab";

        private UniTaskCompletionSource _advanceSource;

        /// <summary>
        /// 显示对话的逻辑
        /// </summary>
        public async UniTask PrintContent(int roleId, int imgId, string content)
        {
            var label = GetControl<TMP_Text>("txtContent");
            if (label != null)
                label.text = content ?? string.Empty;
            _advanceSource = new UniTaskCompletionSource();
            await _advanceSource.Task;
            _advanceSource = null;
        }

        /// <summary>
        /// 完成对话的逻辑
        /// </summary>
        protected override void OnButtonClick(string btnName)
        {
            if (btnName == "btnNext")
                _advanceSource?.TrySetResult();
        }
    }
}
