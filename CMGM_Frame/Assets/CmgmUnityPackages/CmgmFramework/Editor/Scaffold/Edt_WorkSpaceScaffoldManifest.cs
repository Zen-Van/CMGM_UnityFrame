using System.Collections.Generic;
using System.IO;
using System.Text;
using CMGM.Core;
using UnityEditor;

/// <summary>
/// 读取 work_space_scaffold.manifest，供脚手架创建与工程路径检查共用。
/// </summary>
public static class Edt_WorkSpaceScaffoldManifest
{
    private const string DefaultWorkSpacePrefix = "Assets/_WorkSpace";
    private const string DefaultTestSpacePrefix = "Assets/_TestSpace";

    public static string ManifestPath =>
        Edt_BaseUtils.EditorRoot + "/Scaffold/work_space_scaffold.manifest";

    private static string TemplateRoot =>
        Edt_BaseUtils.EditorRoot + "/TemplateCreator/Templates/WorkSpaceSeed";

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
            var sb = new StringBuilder();
            sb.AppendLine("[脚手架] 补全结果：");

            AppendSection(sb, "新建目录", CreatedDirectories);
            AppendSection(sb, "已存在（跳过目录）", SkippedDirectories);
            AppendSection(sb, "新建文件", CreatedFiles);
            AppendSection(sb, "已存在（跳过文件）", SkippedFiles);
            AppendSection(sb, "错误", Errors);

            if (AnyCreated)
                CmgmLog.fPositive(sb.ToString().TrimEnd());
            else if (Errors.Count > 0)
                CmgmLog.fError(sb.ToString().TrimEnd());
            else
                CmgmLog.fNormal(sb.ToString().TrimEnd());
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
    {
        var entries = new List<string>();
        if (!File.Exists(ManifestPath))
        {
            CmgmLog.fError($"[脚手架] 找不到清单 {ManifestPath}");
            return entries;
        }

        foreach (string rawLine in File.ReadAllLines(ManifestPath))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#"))
                continue;

            entries.Add(NormalizeEntry(line));
        }

        return entries;
    }

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
            report.LogSummary();

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
        => entry.Replace('\\', '/').Trim().TrimEnd('/');

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
        // .cs 部署后由 Unity 生成 MonoScript meta；不可复制 TextScriptImporter 的 .cs.txt.meta
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
