using CMGM.Core;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 项目初始化：按 manifest 创建目录与种子文件（含 vFolders 着色）。
/// </summary>
public static class Edt_ProjectLayerSetup
{
    public const string MenuPath = "草木句萌/▶ 项目初始化";
    private const int MenuPriority = -1000;

    [MenuItem(MenuPath, false, MenuPriority)]
    public static void InitializeFrameworkDirectories()
    {
        if (!EditorUtility.DisplayDialog("项目初始化",
                "将按 project_layer.manifest 创建 _WorkSpace、_TestSpace、_PublicRes 等目录与种子文件，\n" +
                "并设置项目初始化目录的 vFolders 文件夹颜色。\n\n" +
                "已存在的目录与文件不会覆盖。\n" +
                "框架内置资源见 CmgmFramework/Resources/。\n\n继续？",
                "初始化", "取消"))
            return;

        var report = Edt_ProjectLayerManifest.EnsureAll();
        if (report.AnyCreated)
            AssetDatabase.Refresh();

        int colored = Edt_VFolderColorSetup.ApplyFrameworkFolderColors();

        EditorUtility.DisplayDialog("项目初始化",
            report.BuildDialogSummary() + "\n\n" +
            (colored > 0 ? $"已设置 {colored} 个文件夹颜色。\n\n" : "") +
            "Settings / UI 基建见 CmgmFramework/Resources/。",
            "确定");
        Edt_QuickSearchMenus.PathSelect(Consts.Paths.WorkSpace);
    }
}
