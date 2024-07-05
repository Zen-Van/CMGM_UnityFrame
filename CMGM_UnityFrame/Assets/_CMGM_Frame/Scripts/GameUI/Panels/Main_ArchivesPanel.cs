using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main_ArchivesPanel : BasePanel
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
                UIManager.Instance.HidePanel<Main_ArchivesPanel>();
                break;
            default:
                break;
        }
    }
}