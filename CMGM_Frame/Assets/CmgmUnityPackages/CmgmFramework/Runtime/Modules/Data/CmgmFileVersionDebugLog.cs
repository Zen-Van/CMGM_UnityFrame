using System.Collections;
using System.Reflection;
using System.Text;
using CMGM.Core;
using System;

namespace CMGM.Data
{
    /// <summary>
    /// 容器 version 与运行时不一致时，将解码结果打印到控制台供人工核对。
    /// </summary>
    internal static class CmgmFileVersionDebugLog
    {
        private const int MaxConfigPreviewRows = 5;

        public static void LogArchiveIfNeeded(string fileLabel, CmgmFileHeader header, object decoded)
        {
            if (header.Version == CmgmFileFormat.ContainerVersion)
                return;

            CmgmLog.fWarning(
                $"[CMGM version核对] {fileLabel} 解码预览（文件 v{header.Version} / 运行时 v{CmgmFileFormat.ContainerVersion}）：\n"
                + FormatObject(decoded));
        }

        public static void LogConfigTableIfNeeded(
            string fileLabel,
            CmgmFileHeader header,
            object tableContainer,
            Type rowType,
            string keyName)
        {
            if (header.Version == CmgmFileFormat.ContainerVersion)
                return;

            var sb = new StringBuilder();
            sb.AppendLine(
                $"[CMGM version核对] {fileLabel} 解码预览（文件 v{header.Version} / 运行时 v{CmgmFileFormat.ContainerVersion}）：");

            object dicObject = tableContainer.GetType().GetField("dataDic")?.GetValue(tableContainer);
            if (dicObject is not IDictionary dic)
            {
                sb.AppendLine("  (无法读取 dataDic)");
                CmgmLog.fWarning(sb.ToString());
                return;
            }

            sb.AppendLine($"  表容器: {tableContainer.GetType().Name}，行数: {dic.Count}");

            int preview = 0;
            FieldInfo[] fields = rowType.GetFields();
            foreach (DictionaryEntry entry in dic)
            {
                if (preview >= MaxConfigPreviewRows)
                {
                    sb.AppendLine($"  … 其余 {dic.Count - MaxConfigPreviewRows} 行省略");
                    break;
                }

                sb.AppendLine($"  [{keyName}={entry.Key}]");
                object row = entry.Value;
                foreach (FieldInfo field in fields)
                    sb.AppendLine($"    {field.Name} = {field.GetValue(row)}");
                preview++;
            }

            CmgmLog.fWarning(sb.ToString());
        }

        private static string FormatObject(object obj)
        {
            if (obj == null)
                return "  (null)";

            var sb = new StringBuilder();
            sb.AppendLine($"  类型: {obj.GetType().FullName}");

            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object value = field.GetValue(obj);
                if (value is IDictionary dic)
                    sb.AppendLine($"  {field.Name} = (Dictionary, Count={dic.Count})");
                else
                    sb.AppendLine($"  {field.Name} = {value}");
            }

            return sb.ToString();
        }
    }
}
