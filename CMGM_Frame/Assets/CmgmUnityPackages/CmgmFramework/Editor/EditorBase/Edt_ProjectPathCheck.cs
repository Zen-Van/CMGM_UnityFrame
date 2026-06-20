using CMGM.Core;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 加载时检查框架路径的正确性
/// </summary>
[InitializeOnLoad]
public class Edt_ProjectPathCheck
{
    private static string errorPaths = "";

    static Edt_ProjectPathCheck()
    {
        //当前检查的路径：
        bool checkResult =
            CheckDirectory(Consts.Paths.WorkSpace) &&
            CheckDirectory(Consts.Paths.ScriptsPath) &&
            CheckDirectory(Consts.Paths.Package.Framework) &&
            CheckDirectory(Consts.Paths.Package.GameKits) &&
            CheckDirectory(Consts.Paths.Framework.Core) &&
            CheckDirectory(Consts.Paths.Framework.Modules) &&
            CheckDirectory(Consts.Paths.Framework.DataModule.Archive) &&
            CheckDirectory(Consts.Paths.Framework.DataModule.Config) &&
            CheckDirectory(Consts.Paths.Framework.DataModule.Editor) &&
            CheckDirectory(Consts.Paths.WorkSpaceScripts.Bootstrap) &&
            CheckDirectory(Consts.Paths.WorkSpaceScripts.UI_Panels) &&
            CheckDirectory(Consts.Paths.WorkSpaceScripts.Archive) &&
            CheckDirectory(Consts.Paths.WorkSpaceScripts.Config) &&
            CheckDirectory(Edt_BaseUtils.EditorRoot);

        //CmgmLog.fPositive("泥嚎，我是你的效能助手小蝉，ฅ ˘ฅ让我开看看工程基础结构是否正确喔～");
        if (checkResult)
        {
            CmgmLog.fPositive("检查完毕，工程路径无误(ノ・ω・)ノ");
        }
        else
        {
            CmgmLog.fError("检查完毕，下列路径没有找到o(╥﹏╥):\n" + errorPaths);
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
