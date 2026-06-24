using CMGM.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CMGM.Loading
{
    /// <summary>
    /// 加载进度面板（框架 Resources/UI/Panels/LoadingPanel.prefab）。
    /// </summary>
    public class LoadingPanel : BasePanel
    {
        private Slider _progressSlider;
        private TMP_Text _statusText;

        protected override void Awake()
        {
            base.Awake();
            _progressSlider = GetControl<Slider>("sldProgress");
            _statusText = GetControl<TMP_Text>("txtStatus");
        }

        public void SetProgress(float overall01)
        {
            if (_progressSlider != null)
                _progressSlider.value = Mathf.Clamp01(overall01);
        }

        public void SetStatus(string text)
        {
            if (_statusText != null)
                _statusText.text = text;
        }
    }
}
