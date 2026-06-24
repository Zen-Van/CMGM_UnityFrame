using System.Collections;
using System.Collections.Generic;
using CMGM.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CMGM.Workspace
{
public class TestPanel : BasePanel
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
                UIManager.Instance.HidePanel<TestPanel>();
                break;
            default:
                break;
        }
    }
}
}
