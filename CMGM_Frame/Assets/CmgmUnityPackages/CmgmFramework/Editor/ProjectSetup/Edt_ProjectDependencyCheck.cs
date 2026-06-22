using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CMGM.Core;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

/// <summary>
/// 项目初始化：检查并安装 framework_dependencies.manifest 中的框架依赖。
/// </summary>
public static class Edt_ProjectDependencyCheck
{
    public sealed class Report
    {
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

    private static Report Check(Report reuse = null)
    {
        var report = reuse ?? new Report();
        report.Satisfied.Clear();
        report.MissingRegistryPackages.Clear();
        report.MissingAssetPaths.Clear();
        report.InstallErrors.Clear();

        var installedPackages = GetInstalledPackageIds();
        foreach (DependencyRule rule in ReadRules())
            EvaluateRule(rule, installedPackages, report);

        LogCheckSummary(report);
        return report;
    }

    private static void EvaluateRule(DependencyRule rule, HashSet<string> installedPackages, Report report)
    {
        switch (rule.Kind)
        {
            case DependencyKind.Registry:
                if (installedPackages.Contains(rule.RegistryPackageId))
                    report.Satisfied.Add($"registry:{rule.RegistryPackageId}");
                else
                    report.MissingRegistryPackages.Add(rule.RegistryPackageId);
                break;

            case DependencyKind.Asset:
                if (AssetPathExists(rule.AssetPath))
                    report.Satisfied.Add($"asset:{rule.AssetPath}");
                else
                    report.MissingAssetPaths.Add(rule.AssetPath);
                break;

            case DependencyKind.Any:
                if (IsAnySatisfied(rule.Alternatives, installedPackages, out string satisfiedLabel))
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

    private static void InstallRegistryPackages(Report report)
    {
        var toInstall = report.MissingRegistryPackages.Distinct().ToList();
        foreach (string packageId in toInstall)
        {
            EditorUtility.DisplayProgressBar("项目初始化", $"正在安装 {packageId}…", 0.5f);
            if (TryInstallRegistryPackage(packageId, out string error))
            {
                report.InstalledRegistryPackages.Add(packageId);
                CmgmLog.fPositive($"[项目初始化] 已安装 Registry 包：{packageId}");
            }
            else
            {
                report.InstallErrors.Add($"{packageId}: {error}");
                CmgmLog.fError($"[项目初始化] 安装失败：{packageId}\n{error}");
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
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
        ListRequest request = Client.List(offline: true);
        while (!request.IsCompleted)
            Thread.Sleep(50);

        if (request.Status != StatusCode.Success)
        {
            CmgmLog.fError("[项目初始化] 无法读取已安装 Package 列表。");
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

    private static void LogCheckSummary(Report report)
    {
        if (report.AllSatisfied)
            CmgmLog.fPositive("[项目初始化] 框架依赖已全部就绪。");
        else
            CmgmLog.fNormal("[项目初始化] 框架依赖检查完成，存在缺失项。");
    }

    private static IEnumerable<DependencyRule> ReadRules()
    {
        foreach (string entry in Edt_ManifestPathUtil.ReadEntries(Edt_CmgmEditorPaths.FrameworkDependenciesManifest))
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
