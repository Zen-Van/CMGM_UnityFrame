using System.IO;
using UnityEditor;
using UnityEngine;

namespace CMGM.Data.Editor
{
public class ArchiveEditor
{
    [MenuItem("草木句萌/清空所有本地存档数据", false, 113)]
    private static void ClearArchiveData()
    {
        if (Directory.Exists(Application.persistentDataPath + "/Archives/"))
        {
            var files = Directory.GetFiles(Application.persistentDataPath + "/Archives/");
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        AssetDatabase.Refresh();
    }
}
}
