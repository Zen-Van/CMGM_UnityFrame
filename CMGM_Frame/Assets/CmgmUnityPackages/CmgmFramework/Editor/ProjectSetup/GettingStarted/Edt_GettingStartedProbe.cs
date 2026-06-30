using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

/// <summary>
/// 入门引导：读取 framework_dependencies.manifest，检查/安装框架依赖（不引用 CMGM.Core）。
/// </summary>
public static class Edt_GettingStartedProbe
{
    public const string DependenciesManifestPath =
        "Assets/CmgmUnityPackages/CmgmFramework/Editor/ProjectSetup/Manifests/framework_dependencies.manifest";
    public const string ProjectLayerManifestPath =
        "Assets/CmgmUnityPackages/CmgmFramework/Editor/ProjectSetup/Manifests/project_layer.manifest";
    private const string FrameSettingsAssetPath =
        "Assets/CmgmUnityPackages/CmgmFramework/Resources/CmgmFrameSettings.asset";
    private const string DefaultWorkSpacePrefix = "Assets/_WorkSpace";
    private const string DefaultTestSpacePrefix = "Assets/_TestSpace";
    private const string DefaultWorkSpaceRoot = "Assets/_WorkSpace";
    private const string DefaultTestSpaceRoot = "Assets/_TestSpace";

    public sealed class DependencyItem
    {
        public string KindLabel;
        public string DisplayName;
        public bool Satisfied;
        public string Hint;
    }

    public sealed class Report
    {
        public readonly List<DependencyItem> Items = new();
        public readonly List<string> Satisfied = new();
        public readonly List<string> MissingRegistryPackages = new();
        public readonly List<string> MissingAssetPaths = new();
        public readonly List<string> InstalledRegistryPackages = new();
        public readonly List<string> InstallErrors = new();
        public bool AllSatisfied => MissingRegistryPackages.Count == 0 && MissingAssetPaths.Count == 0;

        public string BuildDialogSummary()
        {
            var sb = new StringBuilder();
            if (AllSatisfied && InstalledRegistryPackages.Count == 0)
            {
                sb.AppendLine("依赖包：已全部就绪。");
                return sb.ToString().TrimEnd();
            }
            sb.AppendLine("依赖包：");
            if (InstalledRegistryPackages.Count > 0)
            {
                sb.AppendLine($"  已安装 Registry 包 {InstalledRegistryPackages.Count} 个：");
                AppendItems(sb, InstalledRegistryPackages, 6);
            }
            if (MissingRegistryPackages.Count > 0)
            {
                sb.AppendLine($"  缺失 Registry 包 {MissingRegistryPackages.Count} 个：");
                AppendItems(sb, MissingRegistryPackages, 6);
            }
            if (MissingAssetPaths.Count > 0)
            {
                sb.AppendLine($"  缺失本地依赖 {MissingAssetPaths.Count} 项（需手动拷贝到工程）：");
                AppendItems(sb, MissingAssetPaths, 6);
            }
            if (InstallErrors.Count > 0)
            {
                sb.AppendLine($"  安装失败 {InstallErrors.Count} 项（详见控制台）：");
                AppendItems(sb, InstallErrors, 4);
            }
            return sb.ToString().TrimEnd();
        }

        private static void AppendItems(StringBuilder sb, List<string> items, int max)
        {
            for (int i = 0; i < items.Count && i < max; i++)
                sb.AppendLine($"    · {items[i]}");
            if (items.Count > max)
                sb.AppendLine($"    … 另有 {items.Count - max} 项");
        }
    }

    public sealed class ProjectInitReport
    {
        public readonly List<string> MissingPaths = new();
        /// <summary>project_layer.manifest 存在且至少有一条有效条目。</summary>
        public bool HasValidManifest;
        public bool IsInitialized => HasValidManifest && MissingPaths.Count == 0;
    }

    public static bool ShouldShowGettingStarted()
    {
        Report dependencies = Check();
        if (!dependencies.AllSatisfied)
            return true;
        return !CheckProjectInitialized().IsInitialized;
    }

    /// <summary>
    /// 调用 CMGM.Editor 中的项目初始化（无顶级菜单，仅供入门引导等入口）。
    /// </summary>
    public static bool TryRunProjectInitialization()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name != "CMGM.Editor")
                continue;
            Type type = assembly.GetType("Edt_ProjectLayerSetup");
            if (type == null)
                return false;
            MethodInfo method = type.GetMethod(
                "InitializeFrameworkDirectories",
                BindingFlags.Public | BindingFlags.Static);
            if (method == null)
                return false;
            method.Invoke(null, null);
            return true;
        }
        return false;
    }

    public static ProjectInitReport CheckProjectInitialized(ProjectInitReport reuse = null)
    {
        var report = reuse ?? new ProjectInitReport();
        report.MissingPaths.Clear();
        report.HasValidManifest = false;
        if (!File.Exists(ProjectLayerManifestPath))
            return report;
        int entryCount = 0;
        foreach (string entry in ReadManifestLines(ProjectLayerManifestPath))
        {
            entryCount++;
            string resolved = ResolveProjectLayerPath(entry);
            if (IsProjectLayerFileEntry(entry))
            {
                if (!File.Exists(resolved))
                    report.MissingPaths.Add(resolved);
            }
            else if (!Directory.Exists(resolved))
                report.MissingPaths.Add(resolved);
        }
        report.HasValidManifest = entryCount > 0;
        return report;
    }

    public static Report Check(Report reuse = null)
    {
        var report = reuse ?? new Report();
        report.Items.Clear();
        report.Satisfied.Clear();
        report.MissingRegistryPackages.Clear();
        report.MissingAssetPaths.Clear();
        report.InstallErrors.Clear();
        var installedPackages = GetInstalledPackageIds();
        foreach (DependencyRule rule in ReadRules())
            EvaluateRule(rule, installedPackages, report);
        return report;
    }

    public static Report EnsureDependencies(bool offerInstall)
    {
        var report = Check();
        if (!offerInstall || report.MissingRegistryPackages.Count == 0)
            return report;
        string missing = string.Join("\n", report.MissingRegistryPackages.Select(id => $"  · {id}"));
        if (!EditorUtility.DisplayDialog("项目初始化 · 依赖包",
                "检测到以下 Unity Registry 包尚未安装：\n\n" + missing +
                "\n\n是否通过 Package Manager 安装与当前 Unity 版本兼容的最新版？",
                "安装", "跳过"))
            return report;
        InstallRegistryPackages(report);
        return Check(report);
    }

    public static void InstallRegistryPackages(Report report)
    {
        var toInstall = report.MissingRegistryPackages.Distinct().ToList();
        foreach (string packageId in toInstall)
        {
            EditorUtility.DisplayProgressBar("入门引导", $"正在安装 {packageId}…", 0.5f);
            if (TryInstallRegistryPackage(packageId, out string error))
            {
                report.InstalledRegistryPackages.Add(packageId);
                UnityEngine.Debug.Log($"[入门引导] 已安装 Registry 包：{packageId}");
            }
            else
            {
                report.InstallErrors.Add($"{packageId}: {error}");
                UnityEngine.Debug.LogError($"[入门引导] 安装失败：{packageId}\n{error}");
            }
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    private static void EvaluateRule(DependencyRule rule, HashSet<string> installedPackages, Report report)
    {
        switch (rule.Kind)
        {
            case DependencyKind.Registry:
                bool registryOk = installedPackages.Contains(rule.RegistryPackageId);
                report.Items.Add(new DependencyItem
                {
                    KindLabel = "Registry",
                    DisplayName = rule.RegistryPackageId,
                    Satisfied = registryOk,
                    Hint = "可通过下方「安装 Registry 包」自动安装",
                });
                if (registryOk)
                    report.Satisfied.Add($"registry:{rule.RegistryPackageId}");
                else
                    report.MissingRegistryPackages.Add(rule.RegistryPackageId);
                break;
            case DependencyKind.Asset:
                bool assetOk = AssetPathExists(rule.AssetPath);
                report.Items.Add(new DependencyItem
                {
                    KindLabel = "本地",
                    DisplayName = rule.AssetPath,
                    Satisfied = assetOk,
                    Hint = "须从原工程或第三方包手动拷贝到 Assets 下对应路径",
                });
                if (assetOk)
                    report.Satisfied.Add($"asset:{rule.AssetPath}");
                else
                    report.MissingAssetPaths.Add(rule.AssetPath);
                break;
            case DependencyKind.Any:
                bool anyOk = IsAnySatisfied(rule.Alternatives, installedPackages, out string satisfiedLabel);
                string display = string.Join(" 或 ", rule.Alternatives);
                report.Items.Add(new DependencyItem
                {
                    KindLabel = "任选",
                    DisplayName = anyOk ? satisfiedLabel : display,
                    Satisfied = anyOk,
                    Hint = anyOk ? "" : "满足 UPM 包或 Assets 路径其一即可",
                });
                if (anyOk)
                {
                    report.Satisfied.Add($"any:{satisfiedLabel}");
                    return;
                }
                string registryAlt = rule.Alternatives.FirstOrDefault(IsRegistryId);
                if (registryAlt != null && !report.MissingRegistryPackages.Contains(registryAlt))
                    report.MissingRegistryPackages.Add(registryAlt);
                foreach (string alt in rule.Alternatives.Where(a => !IsRegistryId(a)))
                {
                    if (!report.MissingAssetPaths.Contains(alt))
                        report.MissingAssetPaths.Add(alt);
                }
                break;
        }
    }

    private static bool IsAnySatisfied(IReadOnlyList<string> alternatives, HashSet<string> installedPackages, out string label)
    {
        foreach (string alt in alternatives)
        {
            if (IsRegistryId(alt))
            {
                if (!installedPackages.Contains(alt))
                    continue;
                label = alt;
                return true;
            }
            if (AssetPathExists(alt))
            {
                label = alt;
                return true;
            }
        }
        label = null;
        return false;
    }

    private static bool TryInstallRegistryPackage(string packageId, out string error)
    {
        error = "";
        AddRequest request = Client.Add(packageId);
        while (!request.IsCompleted)
            Thread.Sleep(50);
        if (request.Status == StatusCode.Success)
            return true;
        error = request.Error?.message ?? "未知错误";
        return false;
    }

    private static HashSet<string> GetInstalledPackageIds()
    {
        ListRequest request = Client.List();
        while (!request.IsCompleted)
            Thread.Sleep(50);
        if (request.Status != StatusCode.Success)
        {
            UnityEngine.Debug.LogError("[入门引导] 无法读取已安装 Package 列表。");
            return new HashSet<string>();
        }
        return request.Result.Select(package => package.name).ToHashSet();
    }

    private static bool AssetPathExists(string assetPath)
    {
        string path = assetPath.Replace('\\', '/');
        return Directory.Exists(path) || File.Exists(path);
    }

    private static bool IsRegistryId(string value) => value.StartsWith("com.");

    private static string ResolveProjectLayerPath(string manifestEntry)
    {
        string workSpaceRoot = ResolveWorkSpaceRoot();
        string entry = NormalizeManifestEntry(manifestEntry);
        if (entry.StartsWith(DefaultWorkSpacePrefix))
            return workSpaceRoot + entry.Substring(DefaultWorkSpacePrefix.Length);
        if (entry == DefaultTestSpacePrefix)
            return DefaultTestSpaceRoot;
        if (entry.StartsWith(DefaultTestSpacePrefix + "/"))
            return DefaultTestSpaceRoot + entry.Substring(DefaultTestSpacePrefix.Length);
        return entry;
    }

    /// <summary>与 CmgmFrameSettings.WORK_SPACE_ROOT 对齐（无 CMGM.Core 引用时读 .asset YAML）。</summary>
    private static string ResolveWorkSpaceRoot()
    {
        if (!File.Exists(FrameSettingsAssetPath))
            return DefaultWorkSpaceRoot;
        foreach (string rawLine in File.ReadAllLines(FrameSettingsAssetPath))
        {
            string line = rawLine.Trim();
            if (!line.StartsWith("WORK_SPACE_ROOT:"))
                continue;
            string value = line.Substring("WORK_SPACE_ROOT:".Length).Trim();
            if (value.Length > 0)
                return value.Replace('\\', '/');
        }
        return DefaultWorkSpaceRoot;
    }

    private static bool IsProjectLayerFileEntry(string manifestEntry)
        => Path.HasExtension(NormalizeManifestEntry(manifestEntry));

    private static string NormalizeManifestEntry(string entry)
        => entry.Replace('\\', '/').Trim().TrimEnd('/');

    private static IEnumerable<string> ReadManifestLines(string manifestAssetPath)
    {
        if (!File.Exists(manifestAssetPath))
        {
            UnityEngine.Debug.LogError($"[入门引导] 找不到清单：{manifestAssetPath}");
            yield break;
        }
        foreach (string rawLine in File.ReadAllLines(manifestAssetPath))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#"))
                continue;
            yield return line;
        }
    }

    private static IEnumerable<DependencyRule> ReadRules()
    {
        foreach (string entry in ReadManifestLines(DependenciesManifestPath))
        {
            int space = entry.IndexOf('\t');
            if (space <= 0)
                continue;
            string kind = entry.Substring(0, space).Trim().ToLowerInvariant();
            string payload = entry.Substring(space + 1).Trim();
            if (payload.Length == 0)
                continue;
            switch (kind)
            {
                case "registry":
                    yield return DependencyRule.Registry(payload);
                    break;
                case "asset":
                    yield return DependencyRule.Asset(payload.Replace('\\', '/'));
                    break;
                case "any":
                    yield return DependencyRule.Any(
                        payload.Split('|').Select(part => part.Trim().Replace('\\', '/')).Where(part => part.Length > 0).ToArray());
                    break;
            }
        }
    }

    private enum DependencyKind
    {
        Registry,
        Asset,
        Any,
    }

    private sealed class DependencyRule
    {
        public DependencyKind Kind;
        public string RegistryPackageId;
        public string AssetPath;
        public string[] Alternatives;

        public static DependencyRule Registry(string packageId) =>
            new() { Kind = DependencyKind.Registry, RegistryPackageId = packageId };

        public static DependencyRule Asset(string assetPath) =>
            new() { Kind = DependencyKind.Asset, AssetPath = assetPath };

        public static DependencyRule Any(string[] alternatives) =>
            new() { Kind = DependencyKind.Any, Alternatives = alternatives };
    }
}
