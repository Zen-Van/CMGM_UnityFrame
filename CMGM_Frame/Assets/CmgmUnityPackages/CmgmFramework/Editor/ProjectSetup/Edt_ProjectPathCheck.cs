using UnityEditor;
using CMGM.Core;

/// <summary>
/// 启动时校验工程路径（业务层 manifest + 框架 manifest；不自动创建）。
/// </summary>
[InitializeOnLoad]
public class Edt_ProjectPathCheck
{
    private static string errorPaths = "";

    static Edt_ProjectPathCheck()
    {
        string scaffoldMissing = "";
        string frameworkMissing = "";
        bool scaffoldOk = Edt_ProjectLayerManifest.ValidateAll(out scaffoldMissing);
        bool frameworkOk = ValidateFrameworkPaths(out frameworkMissing);
        bool checkResult = scaffoldOk && frameworkOk;

        if (!string.IsNullOrEmpty(scaffoldMissing))
            errorPaths += scaffoldMissing;

        if (!string.IsNullOrEmpty(frameworkMissing))
            errorPaths += frameworkMissing;

        if (checkResult)
        {
            CmgmLog.fPositive("检查完毕，工程路径无误(ノ・ω・)ノ");
        }
        else
        {
            CmgmLog.fError("检查完毕，下列路径没有找到o(╥﹏╥):\n" + errorPaths +
                           $"\n可通过菜单「{Edt_ProjectLayerSetup.MenuPath}」补全。");
        }
    }

    private static bool ValidateFrameworkPaths(out string missingPaths)
    {
        return Edt_ManifestPathUtil.ValidateDirectories(
            Edt_ManifestPathUtil.ReadEntries(Edt_CmgmEditorPaths.FrameworkLayoutManifest),
            entry => entry,
            out missingPaths);
    }
}
