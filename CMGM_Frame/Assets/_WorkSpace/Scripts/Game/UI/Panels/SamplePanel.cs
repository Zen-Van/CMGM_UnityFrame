using System.Collections;
using System.Collections.Generic;
using CMGM.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CMGM.Game
{
public class SamplePanel : BasePanel
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
                UIManager.Instance.HidePanel<SamplePanel>();
                break;
            default:
                break;
        }
    }
}
}
