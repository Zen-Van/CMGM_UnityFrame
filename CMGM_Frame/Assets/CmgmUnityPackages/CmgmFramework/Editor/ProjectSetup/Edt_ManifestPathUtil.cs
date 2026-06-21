using System;
using System.Collections.Generic;
using System.IO;
using CMGM.Core;

/// <summary>
/// 读取 *.manifest 路径清单（# 注释、空行跳过）。
/// </summary>
public static class Edt_ManifestPathUtil
{
    public static IReadOnlyList<string> ReadEntries(string manifestAssetPath)
    {
        var entries = new List<string>();
        if (!File.Exists(manifestAssetPath))
        {
            CmgmLog.fError($"[路径清单] 找不到 {manifestAssetPath}");
            return entries;
        }

        foreach (string rawLine in File.ReadAllLines(manifestAssetPath))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#"))
                continue;

            entries.Add(NormalizeEntry(line));
        }

        return entries;
    }

    public static bool ValidateDirectories(
        IReadOnlyList<string> entries,
        Func<string, string> resolvePath,
        out string missingPaths)
    {
        missingPaths = "";
        bool ok = true;

        foreach (string entry in entries)
        {
            string resolved = resolvePath(entry);
            if (Directory.Exists(resolved))
                continue;

            missingPaths += resolved + "\n";
            ok = false;
        }

        return ok;
    }

    public static string NormalizeEntry(string entry)
        => entry.Replace('\\', '/').Trim().TrimEnd('/');
}
