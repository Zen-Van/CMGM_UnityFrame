using System;
using System.Reflection;
using CMGM.Core;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 项目初始化：按 manifest 创建目录与种子文件（含 vFolders 着色）。
/// 常规入口见「草木句萌/入门引导」窗口；不设顶级菜单项以防误触。
/// </summary>
public static class Edt_ProjectLayerSetup
{
    public static void InitializeFrameworkDirectories()
    {
        if (!EditorUtility.DisplayDialog("项目初始化",
                "将检查框架依赖包（Addressables、Input System、TextMesh Pro、XLua、UniTask、Wwise、Odin 等），\n" +
                "按 project_layer.manifest 创建目录与种子文件，\n" +
                "再从 WorkSpace/Excels 构建配表（Container + StreamingAssets/TableConfig），\n" +
                "并设置项目初始化目录的 vFolders 文件夹颜色。\n\n" +
                "已存在的目录与文件不会覆盖。\n" +
                "Registry 缺失包可自动安装；XLua / UniTask / Wwise / Odin 等本地目录需随工程一并拷贝。\n\n继续？",
                "初始化", "取消"))
            return;

        var dependencyReport = Edt_ProjectDependencyCheck.EnsureDependencies(offerInstall: true);

        var report = Edt_ProjectLayerManifest.EnsureAll();
        bool wwiseCleared = Edt_WwiseSettingsSetup.ClearWwiseProjectPath();
        if (report.AnyCreated || wwiseCleared)
            AssetDatabase.Refresh();

        bool excelBuilt = TryBuildExcelFromWorkSpace(out string excelBuildNote);
        if (excelBuilt)
            AssetDatabase.Refresh();

        int colored = Edt_VFolderColorSetup.ApplyFrameworkFolderColors();

        EditorUtility.DisplayDialog("项目初始化",
            dependencyReport.BuildDialogSummary() + "\n\n" +
            report.BuildDialogSummary() + "\n\n" +
            excelBuildNote + "\n\n" +
            (wwiseCleared ? "已清空 WwiseSettings.xml 中的 Wwise 工程路径。\n\n" : "") +
            (colored > 0 ? $"已设置 {colored} 个文件夹颜色。\n\n" : "") +
            "Settings / UI 基建见 CmgmFramework/Resources/。",
            "确定");
        Edt_QuickSearchMenus.PathSelect(Consts.Paths.WorkSpace);
    }

    /// <summary>
    /// 反射调用 CMGM.Data.Editor.ExcelTool.BuildFromWorkSpaceExcels（避免程序集环依赖）。
    /// </summary>
    private static bool TryBuildExcelFromWorkSpace(out string note)
    {
        note = "";
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name != "CMGM.Data.Editor")
                continue;

            Type type = assembly.GetType("CMGM.Data.Editor.ExcelTool");
            if (type == null)
            {
                note = "Excel 导表：未找到 ExcelTool（CMGM.Data.Editor 未就绪）。";
                CmgmLog.fWarning(note);
                return false;
            }

            MethodInfo method = type.GetMethod(
                "BuildFromWorkSpaceExcels",
                BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                note = "Excel 导表：ExcelTool.BuildFromWorkSpaceExcels 不存在。";
                CmgmLog.fError(note);
                return false;
            }

            try
            {
                method.Invoke(null, null);
                note = "Excel 导表：已扫描 WorkSpace/Excels 并生成 Container + TableConfig。";
                CmgmLog.fPositive(note);
                return true;
            }
            catch (TargetInvocationException ex)
            {
                note = "Excel 导表失败，详见 Console。";
                CmgmLog.fError($"[项目初始化] {note}\n{ex.InnerException ?? ex}");
                return false;
            }
        }

        note = "Excel 导表：CMGM.Data.Editor 未加载（Safe Mode 或尚未编译）。";
        CmgmLog.fWarning(note);
        return false;
    }
}
