using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

public class Edt_CreateEmptyFolderAction : EndNameEditAction
{
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        Directory.CreateDirectory(pathName);

        //刷新本地资源
        AssetDatabase.ImportAsset(pathName);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 返回在Project窗口下选中的路径
    /// </summary>
    /// <returns></returns>
    private static string GetSelectedPath()
    {
        //默认路径为Assets
        string selectedPath = "Assets";

        //获取选中的资源
        Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

        //遍历选中的资源以返回路径
        foreach (Object obj in selection)
        {
            selectedPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(selectedPath) && File.Exists(selectedPath))
            {
                selectedPath = Path.GetDirectoryName(selectedPath);
                break;
            }
        }

        return selectedPath;
    }


    //--------------------------------------------------------------
    [MenuItem("Assets/CMGM Create/Folder", false, -200)]
    static void CreateEmptyFolder()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            CreateInstance<Edt_CreateEmptyFolderAction>(),
            GetSelectedPath() + "/NewFolder", null,
            null);
    }
}
