using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 入门引导窗口：展示 framework_dependencies.manifest 逐项检查结果。
/// </summary>
public sealed class Edt_GettingStartedWindow : EditorWindow
{
    public const string MenuPath = "草木句萌/入门引导";
    private const int MenuPriority = -1100;
    private const int MissingPathPreviewMax = 6;
    private Vector2 _scroll;
    private Edt_GettingStartedProbe.Report _report;
    private Edt_GettingStartedProbe.ProjectInitReport _projectInitReport;

    [MenuItem(MenuPath, false, MenuPriority)]
    public static void OpenFromMenu()
    {
        ShowWindow(Edt_GettingStartedProbe.Check());
    }

    public static void ShowWindow(Edt_GettingStartedProbe.Report report)
    {
        var window = GetWindow<Edt_GettingStartedWindow>(true, "入门引导", true);
        window.minSize = new Vector2(520f, 400f);
        window._report = report;
        window._projectInitReport = Edt_GettingStartedProbe.CheckProjectInitialized();
        window.Show();
        window.Focus();
    }

    private void OnGUI()
    {
        if (_report == null)
            RefreshAll();
        DrawHeader();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        DrawDependencyItems();
        if (_report.AllSatisfied)
            DrawProjectInitSection();
        EditorGUILayout.EndScrollView();
        DrawFooter();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(4f);
        if (!_report.AllSatisfied)
        {
            EditorGUILayout.HelpBox(
                "以下依赖尚未满足。请按提示补齐 Registry 包或拷贝本地 Assets；每次编译结束后本窗口会自动弹出，直至依赖与项目初始化均完成。",
                MessageType.Warning);
        }
        else if (!_projectInitReport.IsInitialized)
        {
            EditorGUILayout.HelpBox(
                "框架依赖已全部就绪。下一步请运行「项目初始化」，按 project_layer.manifest 创建 _WorkSpace 目录与种子文件。",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "框架依赖与项目初始化均已完成。若 Console 仍有编译错误请等待重编；可关闭本窗口并开始配置 Addressables、Play 入口场景等。",
                MessageType.Info);
        }
        EditorGUILayout.Space(4f);
    }

    private void DrawDependencyItems()
    {
        EditorGUILayout.LabelField("框架依赖", EditorStyles.boldLabel);
        foreach (Edt_GettingStartedProbe.DependencyItem item in _report.Items)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            string mark = item.Satisfied ? "✓" : "✗";
            var style = new GUIStyle(EditorStyles.boldLabel);
            if (!item.Satisfied)
                style.normal.textColor = new Color(0.85f, 0.25f, 0.2f);
            else
                style.normal.textColor = new Color(0.2f, 0.65f, 0.3f);
            EditorGUILayout.LabelField($"[{item.KindLabel}] {mark}  {item.DisplayName}", style);
            if (!string.IsNullOrEmpty(item.Hint))
                EditorGUILayout.LabelField(item.Hint, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2f);
        }
    }

    private void DrawProjectInitSection()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("项目初始化", EditorStyles.boldLabel);
        if (!_projectInitReport.HasValidManifest)
        {
            EditorGUILayout.HelpBox(
                $"无法读取 project_layer.manifest（{Edt_GettingStartedProbe.ProjectLayerManifestPath}）。",
                MessageType.Error);
            return;
        }
        if (_projectInitReport.IsInitialized)
        {
            EditorGUILayout.HelpBox("project_layer.manifest 清单路径均已存在，项目已初始化。", MessageType.None);
            return;
        }
        EditorGUILayout.HelpBox(
            $"尚有 {_projectInitReport.MissingPaths.Count} 项目录或种子文件缺失（清单见 project_layer.manifest）。",
            MessageType.Warning);
        foreach (string path in _projectInitReport.MissingPaths.Take(MissingPathPreviewMax))
            EditorGUILayout.LabelField("✗  " + path, EditorStyles.miniLabel);
        if (_projectInitReport.MissingPaths.Count > MissingPathPreviewMax)
        {
            EditorGUILayout.LabelField(
                $"… 另有 {_projectInitReport.MissingPaths.Count - MissingPathPreviewMax} 项",
                EditorStyles.centeredGreyMiniLabel);
        }
        EditorGUILayout.Space(4f);
        if (GUILayout.Button("▶ 项目初始化", GUILayout.Height(32f)))
            TryRunProjectInitialization();
    }

    private void DrawFooter()
    {
        EditorGUILayout.Space(6f);
        if (!_report.AllSatisfied)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = _report.MissingRegistryPackages.Count > 0;
            if (GUILayout.Button("安装 Registry 包", GUILayout.Height(28f)))
            {
                Edt_GettingStartedProbe.InstallRegistryPackages(_report);
                RefreshAll();
            }
            GUI.enabled = true;
            if (GUILayout.Button("重新检查", GUILayout.Height(28f)))
                RefreshAll();
            EditorGUILayout.EndHorizontal();
        }
        else if (!_projectInitReport.IsInitialized)
        {
            if (GUILayout.Button("重新检查", GUILayout.Height(28f)))
                RefreshAll();
        }
        else if (GUILayout.Button("重新检查", GUILayout.Height(28f)))
            RefreshAll();
        if (_report.InstallErrors.Count > 0)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.HelpBox(
                "部分 Registry 包安装失败，详见 Console。\n" + string.Join("\n", _report.InstallErrors),
                MessageType.Error);
        }
    }

    private void TryRunProjectInitialization()
    {
        if (!Edt_GettingStartedProbe.TryRunProjectInitialization())
        {
            EditorUtility.DisplayDialog("入门引导",
                "无法执行「项目初始化」：CMGM.Editor 可能尚未编译通过。\n\n" +
                "请等待 Console 编译错误清零后，再点本窗口「▶ 项目初始化」按钮。",
                "确定");
            return;
        }
        RefreshAll();
    }

    private void RefreshAll()
    {
        _report = Edt_GettingStartedProbe.Check();
        _projectInitReport = Edt_GettingStartedProbe.CheckProjectInitialized();
        Repaint();
    }
}
