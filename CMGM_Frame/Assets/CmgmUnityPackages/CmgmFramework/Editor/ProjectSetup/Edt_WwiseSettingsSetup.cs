using System.IO;
using System.Xml;
using CMGM.Core;
using UnityEditor;

/// <summary>
/// 项目初始化：重置 Wwise 集成中与工程绑定的路径项。
/// </summary>
public static class Edt_WwiseSettingsSetup
{
    private const string WwiseSettingsPath = "Assets/WwiseSettings.xml";

    public static bool ClearWwiseProjectPath()
    {
        if (!File.Exists(WwiseSettingsPath))
        {
            CmgmLog.fNormal("[项目初始化] 未找到 WwiseSettings.xml，跳过 Wwise 工程路径重置。");
            return false;
        }

        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.Load(WwiseSettingsPath);

        XmlNodeList nodes = doc.GetElementsByTagName("WwiseProjectPath");
        if (nodes.Count == 0 || nodes[0] is not XmlElement node)
        {
            CmgmLog.fNormal("[项目初始化] WwiseSettings.xml 中未找到 WwiseProjectPath 节点。");
            return false;
        }

        if (string.IsNullOrEmpty(node.InnerText))
            return false;

        node.InnerText = "";
        doc.Save(WwiseSettingsPath);
        CmgmLog.fPositive("[项目初始化] 已清空 WwiseSettings.xml / WwiseProjectPath。");
        return true;
    }
}
