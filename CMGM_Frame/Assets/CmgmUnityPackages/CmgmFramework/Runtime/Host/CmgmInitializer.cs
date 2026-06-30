using System.Collections.Generic;
using CMGM.Core;
using CMGM.GameFlow;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace CMGM
{
    /// <summary>
    /// InitScene 薄组合根（#9）：Logo 展示 + <see cref="GameFlowMachine"/>，不直调 Loading / Scene。
    /// </summary>
    public class CmgmInitializer : MonoBehaviour
    {
        public bool SHOW_LOGO = true;

        [Header("Logo 展示")]
        [Tooltip("InitScene 上用于 Logo 视频/图片的全屏 Canvas（sortingOrder 较高，Loading 前须隐藏）")]
        public Canvas logoPresentationCanvas;
        public VideoPlayer videoPlayer;
        public List<Object> Logos;

        private void Awake()
        {
            if (!SHOW_LOGO) Logos.Clear();
            InitGame().Forget();
        }

        /// <summary>Logo 与 CmgmInitState 并行，完成后淡出并进主界面。</summary>
        public async UniTask InitGame()
        {
            await UniTask.WhenAll(
                GameFlowMachine.Instance.SwitchToAsync(new CmgmInitState()),
                ShowLogosAsync());

            await HideLogoPresentationAsync();

            await GameFlowMachine.Instance.SwitchToAsync(new MainMenuState());
        }

        private async UniTask ShowLogosAsync()
        {
            //TODO: 加载logo并逐个显示
            for (int i = 0; i < Logos.Count; i++)
            {
                Object logo = Logos[i];
                if (logo is VideoClip)
                {
                    videoPlayer.clip = logo as VideoClip;
                    videoPlayer.time = 0f;

                    videoPlayer.Prepare();
                    videoPlayer.Play();

                    bool videoFinished = false;
                    videoPlayer.loopPointReached += (video) => { videoFinished = true; };
                    await UniTask.WaitUntil(() => videoFinished);
                }
                else if (logo is Texture2D)
                {
                    //如果是图片的话

                }
                else
                {
                    CmgmLog.fNegative($"不支持的logo文件类型：{logo.GetType()}");
                }
            }
        }

        private async UniTask HideLogoPresentationAsync()
        {
            videoPlayer?.Stop();

            var logoRoot = logoPresentationCanvas != null
                ? logoPresentationCanvas.gameObject
                : GameObject.Find("VedioImage")?.GetComponentInParent<Canvas>()?.gameObject;

            if (logoRoot == null)
                return;

            if (!logoRoot.TryGetComponent(out CanvasGroup cg))
                cg = logoRoot.AddComponent<CanvasGroup>();

            for (float t = 0.5f; t > 0f; t -= Time.deltaTime, cg.alpha = t / 0.5f)
                await UniTask.Yield();

            logoRoot.SetActive(false);
        }
    }
}
