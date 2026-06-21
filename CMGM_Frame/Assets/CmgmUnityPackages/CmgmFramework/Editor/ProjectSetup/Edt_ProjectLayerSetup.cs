using CMGM.Core;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 项目脚手架：按 manifest 初始化框架目录（含 vFolders 着色）。
/// </summary>
public static class Edt_ProjectLayerSetup
{
    private const string MenuPath = "草木句萌/脚手架/框架目录初始化";

    [MenuItem(MenuPath, false, 0)]
    public static void InitializeFrameworkDirectories()
    {
        if (!EditorUtility.DisplayDialog("框架目录初始化",
                "将按 project_layer.manifest 创建 _WorkSpace、_TestSpace、_PublicRes 等目录与种子文件，\n" +
                "并为框架关键目录设置 vFolders 文件夹颜色。\n\n" +
                "已存在的目录与文件不会覆盖。\n" +
                "框架内置资源见 CmgmFramework/Resources/。\n\n继续？",
                "初始化", "取消"))
            return;

        var report = Edt_ProjectLayerManifest.EnsureAll();
        int colored = Edt_VFolderColorSetup.ApplyFrameworkFolderColors();
        if (report.AnyCreated)
            AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("脚手架",
            report.BuildDialogSummary() + "\n\n" +
            (colored > 0 ? $"已设置 {colored} 个文件夹颜色。\n\n" : "") +
            "Settings / UI 基建见 CmgmFramework/Resources/。",
            "确定");
        Edt_QuickSearchMenus.PathSelect(Consts.Paths.WorkSpace);
    }
}
