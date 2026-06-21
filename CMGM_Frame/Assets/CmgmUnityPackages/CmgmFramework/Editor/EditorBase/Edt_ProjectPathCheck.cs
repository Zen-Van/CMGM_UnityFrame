using CMGM.Core;
using UnityEditor;
using System.IO;

/// <summary>
/// 加载时检查工程路径（不自动创建脚手架内容，仅校验）。
/// </summary>
[InitializeOnLoad]
public class Edt_ProjectPathCheck
{
    private static string errorPaths = "";

    static Edt_ProjectPathCheck()
    {
        bool checkResult =
            Edt_WorkSpaceScaffoldManifest.ValidateAll(out string scaffoldMissing) &&
            CheckFrameworkPaths();

        if (!string.IsNullOrEmpty(scaffoldMissing))
            errorPaths += scaffoldMissing;

        if (checkResult)
        {
            CmgmLog.fPositive("检查完毕，工程路径无误(ノ・ω・)ノ");
        }
        else
        {
            CmgmLog.fError("检查完毕，下列路径没有找到o(╥﹏╥):\n" + errorPaths +
                           "\n可通过菜单「草木句萌/脚手架/创建游戏层与测试层」创建项目基础目录喔~");
        }
    }

    private static bool CheckFrameworkPaths()
    {
        return CheckDirectory(Consts.Paths.Package.Framework) &&
               CheckDirectory(Consts.Paths.Package.GameKits) &&
               CheckDirectory(Consts.Paths.Framework.Runtime) &&
               CheckDirectory(Consts.Paths.Framework.Core) &&
               CheckDirectory(Consts.Paths.Framework.Modules) &&
               CheckDirectory(Consts.Paths.Framework.DataModule.Archive) &&
               CheckDirectory(Consts.Paths.Framework.DataModule.Config) &&
               CheckDirectory(Consts.Paths.Framework.DataModule.Editor) &&
               CheckDirectory(Consts.Paths.Framework.Resources) &&
               CheckDirectory(Consts.Paths.Framework.Resources + "/UI") &&
               CheckDirectory(Edt_BaseUtils.EditorRoot);
    }

    private static bool CheckDirectory(string path)
    {
        if (Directory.Exists(path))
            return true;

        errorPaths += path + "\n";
        return false;
    }
}
