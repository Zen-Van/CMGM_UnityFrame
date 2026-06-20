using System.IO;
using CMGM.Core;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 项目脚手架1.2：一键创建 _WorkSpace / _TestSpace 目录骨架（空工程 + 已有 CmgmUnityPackages 时使用）。
/// </summary>
public static class Edt_WorkSpaceScaffold
{
    private const string MenuWorkSpace = "草木句萌/脚手架/创建 _WorkSpace 游戏层骨架";
    private const string MenuTestSpace = "草木句萌/脚手架/创建 _TestSpace 测试层骨架";

    [MenuItem(MenuWorkSpace, false, 0)]
    public static void CreateWorkSpaceSkeleton()
    {
        if (!Confirm("创建 _WorkSpace 游戏层骨架",
                "将创建 Excels、HotRes、Scripts 等目录；已存在的目录与文件不会覆盖。\n\n" +
                "框架内置资源（Settings、UI、Logo、字体）在 CmgmFramework/Resources/，无需在工作区重复创建。\n\n继续？"))
            return;

        string workSpace = Consts.Paths.WorkSpace;
        EnsureDirectory(workSpace);
        EnsureDirectory(workSpace + "/Excels");
        EnsureDirectory(Consts.Paths.HotRes);
        EnsureDirectory(Consts.Paths.Lua_Path);
        EnsureDirectory(Consts.Paths.HotScene);
        EnsureDirectory(Consts.Paths.HotRes_UIPanel);
        EnsureDirectory(Consts.Paths.WorkSpaceScripts.Bootstrap);
        EnsureDirectory(Consts.Paths.WorkSpaceScripts.UI_Panels);
        EnsureDirectory(Consts.Paths.WorkSpaceScripts.Archive);
        EnsureDirectory(Consts.Paths.WorkSpaceScripts.Config);

        CopyTemplateIfMissing(
            workSpace + "/GAME_WORKSPACE.md",
            Edt_BaseUtils.EditorRoot + "/TemplateCreator/Templates/GAME_WORKSPACE.md.txt");

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("脚手架",
            "_WorkSpace 骨架已创建或补全。\n" +
            "CmgmFrameSettings 等见 CmgmFramework/Resources/。\n请查看 GAME_WORKSPACE.md 并按项目填写 §5。",
            "确定");
        Edt_BaseUtils.PathSelect(workSpace);
    }

    [MenuItem(MenuTestSpace, false, 1)]
    public static void CreateTestSpaceSkeleton()
    {
        if (!Confirm("创建 _TestSpace 测试层骨架",
                "将创建 Scripts、Scenes 目录；已存在的目录不会覆盖。\n\n继续？"))
            return;

        EnsureDirectory(Consts.Paths.TestSpace);
        EnsureDirectory(Consts.Paths.TestScriptsPath);
        EnsureDirectory(Consts.Paths.TestSpace + "/Scenes");

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("脚手架", "_TestSpace 骨架已创建或补全。", "确定");
        Edt_BaseUtils.PathSelect(Consts.Paths.TestSpace);
    }

    private static bool Confirm(string title, string message)
        => EditorUtility.DisplayDialog(title, message, "创建", "取消");

    private static void EnsureDirectory(string assetPath)
    {
        if (Directory.Exists(assetPath))
            return;

        Directory.CreateDirectory(assetPath);
        CmgmLog.fPositive($"[脚手架] 创建目录 {assetPath}");
    }

    private static void CopyTemplateIfMissing(string targetAssetPath, string templateAssetPath)
    {
        if (File.Exists(targetAssetPath))
            return;

        if (!File.Exists(templateAssetPath))
        {
            CmgmLog.fError($"[脚手架] 找不到模板 {templateAssetPath}");
            return;
        }

        File.Copy(templateAssetPath, targetAssetPath);
        CmgmLog.fPositive($"[脚手架] 写入 {targetAssetPath}");
    }
}
