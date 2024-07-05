using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level_TeamPanel : BasePanel
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnButtonClick(string btnName)
    {
        switch(btnName)
        {
            case "btnBack":
                UIManager.Instance.HidePanel<Level_TeamPanel>();
                break;
            default:
                break;
        }
    }
}