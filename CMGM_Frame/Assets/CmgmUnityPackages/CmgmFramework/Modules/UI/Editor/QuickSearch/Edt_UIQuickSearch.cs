using CMGM.Core;
using UnityEditor;
using UnityEngine;

namespace CMGM.UI.Editor
{
public class Edt_UIQuickSearch
{
    //------------------------------------------------------
    [MenuItem("Assets/CMGM Search/UI面板_脚本", false, -189)]
    static void SelectPanelScriptPath()//s = s.Substring(0,s.Length - 1)
    {
        string path = Consts.Paths.WorkSpaceScripts.UI_Panels;
        Edt_BaseUtils.PathSelect(path);
    }
    
    [MenuItem("Assets/CMGM Search/UI面板_资源", false, -189)]
    static void SelectPanelAssetPath()
    {
        string path = Consts.Paths.HotRes_UIPanel;
        Edt_BaseUtils.PathSelect(path);
    }
    
}
}
