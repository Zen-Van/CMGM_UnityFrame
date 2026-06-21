using CMGM.Core;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 项目脚手架：按 work_space_scaffold.manifest 手动创建 _WorkSpace 与 _TestSpace。
/// </summary>
public static class Edt_WorkSpaceScaffold
{
    private const string MenuPath = "草木句萌/脚手架/创建游戏层与测试层";

    [MenuItem(MenuPath, false, 0)]
    public static void CreateProjectLayers()
    {
        if (!EditorUtility.DisplayDialog("创建游戏层与测试层",
                "将按 work_space_scaffold.manifest 创建 _WorkSpace、_TestSpace 目录与种子文件。\n\n" +
                "已存在的目录与文件不会覆盖。\n" +
                "框架内置资源见 CmgmFramework/Resources/。\n\n继续？",
                "创建", "取消"))
            return;

        var report = Edt_WorkSpaceScaffoldManifest.EnsureAll();
        if (report.AnyCreated)
            AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("脚手架",
            "_WorkSpace / _TestSpace 已按清单创建或补全。\n" +
            "详情见控制台。\n" +
            "Settings / UI 基建见 CmgmFramework/Resources/。",
            "确定");
        Edt_BaseUtils.PathSelect(Consts.Paths.WorkSpace);
    }
}
