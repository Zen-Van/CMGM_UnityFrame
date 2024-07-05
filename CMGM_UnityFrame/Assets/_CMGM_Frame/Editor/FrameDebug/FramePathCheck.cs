using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 加载时检查框架路径的正确性
/// </summary>
[InitializeOnLoad]
public class FramePathCheck
{
    private static string errorPaths = "";

    #region 要检查的Editor路径
    const string Creator_Script_Path = Consts.Paths.RootPath + "/Editor/TemplateCreator/Scripts";
    const string Creator_Template_Path = Consts.Paths.RootPath + "/Editor/TemplateCreator/Templates";
    #endregion



    static FramePathCheck()
    {
        //当前检查的路径：根路径、脚本路径、设置路径、AB包路径、UI面板路径、UI面板脚本路径
        //当前检查的编辑器路径：Creator脚本、Creator模板
        if (CheckDirectory(Consts.Paths.RootPath) && CheckDirectory(Consts.Paths.ScriptsPath) &&
             CheckDirectory(Consts.Paths.AbResPath) &&
           CheckDirectory(Consts.Paths.Script_UI_Panel_Path) && CheckDirectory(Consts.Paths.Ab_UI_Panel_Path) &&
           CheckDirectory(Creator_Template_Path) && CheckDirectory(Creator_Script_Path) && CheckDirectory(Consts.Paths.Lua_Path)
           )
        {
            CmgmLog.FrameLogPositive("框架路径检查完毕，框架路径正确");
        }
        else
        {
            CmgmLog.FrameLogError("<color=#FF7F00>[蝉的框架]</color>：框架路径检查完毕，下列路径没有找到:\n" + errorPaths);
        }
    }

    private static bool CheckDirectory(string path)
    {
        if (Directory.Exists(path)) { return true; }
        else
        {
            errorPaths += path + "\n";
            return false;
        }
    }
}
