using System.Collections.Generic;
using System.IO;
using System.Text;
using CMGM.Core;

/// <summary>
/// 读取 work_space_scaffold.manifest，供脚手架创建与工程路径检查共用。
/// </summary>
public static class Edt_ProjectLayerManifest
{
    private const string DefaultWorkSpacePrefix = "Assets/_WorkSpace";
    private const string DefaultTestSpacePrefix = "Assets/_TestSpace";

    public static string ManifestPath => Edt_CmgmEditorPaths.ProjectLayerManifest;

    private static string TemplateRoot => Edt_CmgmEditorPaths.ProjectLayerSeeds;

    public sealed class EnsureReport
    {
        public readonly List<string> CreatedDirectories = new();
        public readonly List<string> SkippedDirectories = new();
        public readonly List<string> CreatedFiles = new();
        public readonly List<string> SkippedFiles = new();
        public readonly List<string> Errors = new();

        public bool AnyCreated => CreatedDirectories.Count > 0 || CreatedFiles.Count > 0;

        public void LogSummary()
        {
            CmgmLog.fNormal(BuildFullLog());
        }

        public string BuildDialogSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"新建：目录 {CreatedDirectories.Count}，文件 {CreatedFiles.Count}");
            sb.AppendLine($"跳过：目录 {SkippedDirectories.Count}，文件 {SkippedFiles.Count}");

            if (Errors.Count > 0)
                sb.AppendLine($"错误：{Errors.Count} 项（详见控制台）");

            AppendDialogSection(sb, "新建目录", CreatedDirectories);
            AppendDialogSection(sb, "新建文件", CreatedFiles);
            AppendDialogSection(sb, "跳过", SkippedDirectories, SkippedFiles);

            return sb.ToString().TrimEnd();
        }

        public string BuildFullLog()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[脚手架] 补全结果：");
            AppendSection(sb, "新建目录", CreatedDirectories);
            AppendSection(sb, "已存在（跳过目录）", SkippedDirectories);
            AppendSection(sb, "新建文件", CreatedFiles);
            AppendSection(sb, "已存在（跳过文件）", SkippedFiles);
            AppendSection(sb, "错误", Errors);
            return sb.ToString().TrimEnd();
        }

        private static void AppendDialogSection(
            StringBuilder sb, string title, List<string> primary, List<string> secondary = null)
        {
            var items = new List<string>();
            items.AddRange(primary);
            if (secondary != null)
                items.AddRange(secondary);

            if (items.Count == 0)
                return;

            sb.AppendLine();
            sb.AppendLine($"{title}：");
            const int max = 8;
            for (int i = 0; i < items.Count && i < max; i++)
                sb.AppendLine($"  · {items[i]}");

            if (items.Count > max)
                sb.AppendLine($"  … 另有 {items.Count - max} 项（见控制台）");
        }

        private static void AppendSection(StringBuilder sb, string title, List<string> items)
        {
            sb.AppendLine($"  {title} ({items.Count})");
            if (items.Count == 0)
            {
                sb.AppendLine("    （无）");
                return;
            }

            foreach (string item in items)
                sb.AppendLine($"    - {item}");
        }
    }

    public static IReadOnlyList<string> ReadEntries()
        => Edt_ManifestPathUtil.ReadEntries(ManifestPath);

    public static string ResolvePath(string manifestEntry)
    {
        string entry = NormalizeEntry(manifestEntry);
        if (entry.StartsWith(DefaultWorkSpacePrefix))
            return Consts.Paths.WorkSpace + entry.Substring(DefaultWorkSpacePrefix.Length);

        if (entry == DefaultTestSpacePrefix)
            return Consts.Paths.TestSpace;

        if (entry.StartsWith(DefaultTestSpacePrefix + "/"))
            return Consts.Paths.TestSpace + entry.Substring(DefaultTestSpacePrefix.Length);

        return entry;
    }

    public static bool IsFileEntry(string manifestEntry)
        => Path.HasExtension(NormalizeEntry(manifestEntry).TrimEnd('/'));

    public static EnsureReport EnsureAll(bool logSummary = true)
    {
        var report = new EnsureReport();
        foreach (string entry in ReadEntries())
        {
            string resolved = ResolvePath(entry);
            if (IsFileEntry(entry))
                EnsureFile(resolved, report);
            else
                EnsureDirectory(resolved, report);
        }

        if (logSummary || report.AnyCreated || report.Errors.Count > 0)
        {
            string log = report.BuildFullLog();
            if (report.AnyCreated)
                CmgmLog.fPositive(log);
            else if (report.Errors.Count > 0)
                CmgmLog.fError(log);
            else
                CmgmLog.fNormal(log);
        }

        return report;
    }

    public static bool ValidateAll(out string missingPaths)
    {
        missingPaths = "";
        bool ok = true;

        foreach (string entry in ReadEntries())
        {
            string resolved = ResolvePath(entry);
            if (IsFileEntry(entry))
            {
                if (File.Exists(resolved))
                    continue;

                missingPaths += resolved + "\n";
                ok = false;
            }
            else if (!Directory.Exists(resolved))
            {
                missingPaths += resolved + "\n";
                ok = false;
            }
        }

        return ok;
    }

    private static string NormalizeEntry(string entry)
        => Edt_ManifestPathUtil.NormalizeEntry(entry);

    private static void EnsureDirectory(string assetPath, EnsureReport report)
    {
        if (Directory.Exists(assetPath))
        {
            report.SkippedDirectories.Add(assetPath);
            return;
        }

        Directory.CreateDirectory(assetPath);
        report.CreatedDirectories.Add(assetPath);
    }

    private static void EnsureFile(string targetAssetPath, EnsureReport report)
    {
        if (File.Exists(targetAssetPath))
        {
            report.SkippedFiles.Add(targetAssetPath);
            return;
        }

        string relative = GetWorkSpaceRelativePath(targetAssetPath);
        if (relative == null)
        {
            report.Errors.Add($"清单文件不在工作区内，无法映射模板：{targetAssetPath}");
            return;
        }

        if (!TryResolveTemplatePath(relative, out string templateAssetPath))
        {
            report.Errors.Add($"找不到模板：{TemplateRoot}/{relative}（.cs 模板为 *.cs.txt）");
            return;
        }

        EnsureDirectory(Path.GetDirectoryName(targetAssetPath), report);
        File.Copy(templateAssetPath, targetAssetPath);
        CopyMetaIfMissing(targetAssetPath, templateAssetPath);
        report.CreatedFiles.Add(targetAssetPath);
    }

    private static bool TryResolveTemplatePath(string workSpaceRelative, out string templateAssetPath)
    {
        templateAssetPath = TemplateRoot + "/" + workSpaceRelative;
        if (File.Exists(templateAssetPath))
            return true;

        if (workSpaceRelative.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase))
        {
            templateAssetPath = TemplateRoot + "/" + workSpaceRelative + ".txt";
            if (File.Exists(templateAssetPath))
                return true;
        }

        templateAssetPath = null;
        return false;
    }

    private static string GetWorkSpaceRelativePath(string resolvedAssetPath)
    {
        string workSpace = Consts.Paths.WorkSpace.Replace('\\', '/');
        string path = resolvedAssetPath.Replace('\\', '/');
        if (!path.StartsWith(workSpace))
            return null;

        string relative = path.Substring(workSpace.Length).TrimStart('/');
        return relative.Length == 0 ? null : relative;
    }

    private static void CopyMetaIfMissing(string targetAssetPath, string templateAssetPath)
    {
        if (targetAssetPath.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase))
            return;

        string targetMeta = targetAssetPath + ".meta";
        if (File.Exists(targetMeta))
            return;

        string sourceMeta = templateAssetPath + ".meta";
        if (!File.Exists(sourceMeta))
            return;

        File.Copy(sourceMeta, targetMeta);
    }
}
