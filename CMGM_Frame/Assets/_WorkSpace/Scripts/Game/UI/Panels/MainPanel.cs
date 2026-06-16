using System.Collections;
using System.Collections.Generic;
using CMGM.Core;
using CMGM.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CMGM.Game
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
}
}
