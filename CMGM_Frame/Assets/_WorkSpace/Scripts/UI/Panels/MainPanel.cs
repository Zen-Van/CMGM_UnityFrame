using System.Collections;
using System.Collections.Generic;
using CMGM.Core;
using CMGM.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CMGM.Workspace
{
public class MainPanel : BasePanel
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnButtonClick(string btnName)
    {
        switch(btnName)
        {
            case "btnStart":
                CmgmLog.fNormal("开始游戏");
                //进游戏加载（配表/资源/音频）；Loading 接入前直接 await（2.5c）
                EnterGameplayAsync().Forget();
                break;
            case "btnLoad":
                CmgmLog.fNormal("加载游戏");
                break;
            case "btnQuit":
                CmgmApplication.Quit();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 主界面 → 进游戏（将来由 Scene 模块 Loading 流程包一层进度条，见 2.5c）。
    /// </summary>
    private async UniTaskVoid EnterGameplayAsync()
    {
        await GameBootstrap.EnterGameplayAsync();
        // TODO 2.5c：Loading 完成后切 Gameplay 场景 / GameFlow
    }
}
}
