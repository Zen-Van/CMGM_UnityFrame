using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelPanel : BasePanel
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnButtonClick(string btnName)
    {
        switch (btnName)
        {
            case "btnTeam":
                UIManager.Instance.ShowPanel<Level_TeamPanel>().Forget();
                break;
            case "btnBag":
                UIManager.Instance.ShowPanel<Level_BagPanel>().Forget();
                break;
            case "btnSystem":
                UIManager.Instance.ShowPanel<Level_SystemPanel>().Forget();
                break;
            default:
                break;
        }
    }
}