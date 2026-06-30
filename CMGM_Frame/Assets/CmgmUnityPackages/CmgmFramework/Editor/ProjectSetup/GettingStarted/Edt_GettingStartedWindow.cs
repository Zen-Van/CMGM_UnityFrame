using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 入门引导窗口：分步展示依赖检查、项目初始化与后续配置。
/// </summary>
public sealed class Edt_GettingStartedWindow : EditorWindow
{
    public const string MenuPath = "草木句萌/入门引导";
    private const int MenuPriority = -1100;
    private const int MissingPathPreviewMax = 6;
    private Edt_GettingStartedProbe.Report _report;
    private Edt_GettingStartedProbe.ProjectInitReport _projectInitReport;
    private bool _expandCompletedStep0;
    private bool _expandCompletedStep1;

    private enum Step
    {
        Dependencies = 0,
        ProjectInit = 1,
        PostInit = 2,
    }

    private Step CurrentStep
    {
        get
        {
            if (!_report.AllSatisfied)
                return Step.Dependencies;
            if (!_projectInitReport.IsInitialized)
                return Step.ProjectInit;
            return Step.PostInit;
        }
    }

    [MenuItem(MenuPath, false, MenuPriority)]
    public static void OpenFromMenu()
    {
        ShowWindow(Edt_GettingStartedProbe.Check());
    }

    public static void ShowWindow(Edt_GettingStartedProbe.Report report)
    {
        var window = GetWindow<Edt_GettingStartedWindow>(true, "入门引导", true);
        window.minSize = new Vector2(520f, 280f);
        window._report = report;
        window._projectInitReport = Edt_GettingStartedProbe.CheckProjectInitialized();
        window.Show();
        window.Focus();
    }

    private void OnGUI()
    {
        if (_report == null)
            RefreshAll();
        EditorGUILayout.Space(4f);
        if (CurrentStep == Step.Dependencies)
            DrawCurrentStepHeader("1. 框架依赖", DrawDependenciesBody);
        else
            DrawCompletedStepHeader("1. 框架依赖", ref _expandCompletedStep0, DrawDependenciesBody);
        if (CurrentStep == Step.ProjectInit)
            DrawCurrentStepHeader("2. 项目初始化", DrawProjectInitBody);
        else if ((int)CurrentStep > (int)Step.ProjectInit)
            DrawCompletedStepHeader("2. 项目初始化", ref _expandCompletedStep1, DrawProjectInitBody);
        if (CurrentStep == Step.PostInit)
            DrawPostInitStep();
        DrawFooter();
    }

    private static void DrawCurrentStepHeader(string title, System.Action drawBody)
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        drawBody();
    }

    private static void DrawCompletedStepHeader(string title, ref bool expanded, System.Action drawExpandedBody)
    {
        EditorGUILayout.Space(6f);
        var doneStyle = new GUIStyle(EditorStyles.foldout);
        doneStyle.fontStyle = FontStyle.Bold;
        doneStyle.normal.textColor = new Color(0.2f, 0.65f, 0.3f);
        expanded = EditorGUILayout.Foldout(expanded, $"✓ {title}", true, doneStyle);
        if (expanded)
        {
            EditorGUILayout.Space(2f);
            drawExpandedBody();
        }
    }

    private void DrawDependenciesBody()
    {
        EditorGUILayout.HelpBox(
            "请补齐 Registry 包或拷贝本地 Assets。每次编译结束后本窗口会自动弹出，直至三步均完成。",
            MessageType.Info);
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

    private void DrawProjectInitBody()
    {
        EditorGUILayout.HelpBox(
            "按 project_layer.manifest 创建 _WorkSpace 目录与种子文件；已存在路径不会覆盖。",
            MessageType.Info);
        if (!_projectInitReport.HasValidManifest)
        {
            EditorGUILayout.HelpBox(
                $"无法读取 project_layer.manifest（{Edt_GettingStartedProbe.ProjectLayerManifestPath}）。",
                MessageType.Error);
            return;
        }
        if (_projectInitReport.IsInitialized)
        {
            EditorGUILayout.HelpBox("project_layer.manifest 清单路径均已存在。", MessageType.None);
            return;
        }
        EditorGUILayout.HelpBox(
            $"尚有 {_projectInitReport.MissingPaths.Count} 项目录或种子文件缺失。",
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

    private void DrawPostInitStep()
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("3. 后续配置", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "依赖与项目初始化已完成。下列为 Play 前建议项（项目脚手架1.8 计划自动化 Addressables 模板）。",
            MessageType.Info);
        EditorGUILayout.LabelField("Addressables · Default Local Group（或你的默认组）", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Window → Asset Management → Addressables → Groups，确认含下列文件夹条目（Address = 文件夹路径）：",
            MessageType.None);
        EditorGUILayout.LabelField("· HotRes/UI  →  Label: UI", EditorStyles.miniLabel);
        EditorGUILayout.LabelField("    ShowPanel → UI/Panels/{Panel}.prefab", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.LabelField("· HotRes/Lua  →  Label: Lua", EditorStyles.miniLabel);
        EditorGUILayout.LabelField("    正式包 Lua 预载", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.LabelField("· HotRes/Scenes  →  无 Label", EditorStyles.miniLabel);
        EditorGUILayout.LabelField("    SceneLoadTask · InitScene / MainScene", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.LabelField("· 可选：LevelPrefabs、BuildSource、_TestSpace/Scenes、CmgmFrameSettings", EditorStyles.miniLabel);
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Play 与构建", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Play Mode Start Scene → InitScene（_WorkSpace/HotRes/Scenes/InitScene）。\n" +
            "首次 Play 前可 Addressables → Build → Default Build Script。\n" +
            "若修改 WORK_SPACE_ROOT，Address 前缀须与 WorkSpace 根一致。",
            MessageType.None);
    }

    private void DrawFooter()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.BeginHorizontal();
        if (CurrentStep == Step.Dependencies)
        {
            GUI.enabled = _report.MissingRegistryPackages.Count > 0;
            if (GUILayout.Button("安装 Registry 包", GUILayout.Height(28f)))
            {
                Edt_GettingStartedProbe.InstallRegistryPackages(_report);
                RefreshAll();
            }
            GUI.enabled = true;
        }
        if (GUILayout.Button("重新检查", GUILayout.Height(28f)))
            RefreshAll();
        EditorGUILayout.EndHorizontal();
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
