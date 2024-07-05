using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level_SystemPanel : BasePanel
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override async void OnButtonClick(string btnName)
    {
        switch(btnName)
        {
            case "btnBack":
                UIManager.Instance.HidePanel<Level_SystemPanel>();
                break;
            case "btnSave":
                GameArchiveManager.Instance.SaveRuntimeData(1);
                break;
            case "btnLoad":
                GameArchiveManager.Instance.LoadRuntimeData(1);
                break;
            case "btnSetting":
                break;
            case "btnQuit":
                await GameSystem.GoToMainUI();
                break;
            default:
                break;
        }
    }
}