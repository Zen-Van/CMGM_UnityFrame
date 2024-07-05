using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ArchiveEditor
{
    [MenuItem("草木句萌/清空所有存档数据", false, 112)]
    private static void ClearArchiveData()
    {
        if(Directory.Exists(Application.persistentDataPath + "/Archives/"))
        {
            var files = Directory.GetFiles(Application.persistentDataPath + "/Archives/");
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        //刷新Project窗口
        AssetDatabase.Refresh();
    }

}
