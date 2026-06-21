using CMGM.Core;
using UnityEditor;
using UnityEngine;
using VFolderData = VFolders.VFoldersData;
using VFoldersLib = VFolders.VFolders;

/// <summary>
/// 脚手架：为框架关键目录设置 vFolders 文件夹颜色。
/// </summary>
public static class Edt_VFolderColorSetup
{
    private const string PublicResRoot = "Assets/_PublicRes";

    // vFolders 调色板索引，与当前工程四个顶层目录一致
    private static readonly (string path, int colorIndex)[] FrameworkFolderColors =
    {
        (Consts.Paths.WorkSpace, 1),
        (Consts.Paths.TestSpace, 4),
        (Consts.Paths.PackageRoot, 6),
        (PublicResRoot, 9),
    };

    public static int ApplyFrameworkFolderColors()
    {
        if (VFoldersLib.data == null)
        {
            CmgmLog.fNormal("[脚手架] 未找到 vFolders Data，跳过文件夹着色。");
            return 0;
        }

        int applied = 0;
        foreach (var (path, colorIndex) in FrameworkFolderColors)
        {
            if (!AssetDatabase.IsValidFolder(path))
                continue;

            VFoldersLib.SetColor(path, colorIndex);
            applied++;
        }

        if (applied > 0)
            SaveFolderColorData();

        return applied;
    }

    private static void SaveFolderColorData()
    {
        if (VFolderData.storeDataInMetaFiles)
        {
            foreach (var (path, _) in FrameworkFolderColors)
            {
                if (!AssetDatabase.IsValidFolder(path))
                    continue;

                string guid = AssetDatabase.AssetPathToGUID(path);
                var folderData = VFoldersLib.GetFolderData(guid, createDataIfDoesntExist: false);
                if (folderData == null)
                    continue;

                var importer = AssetImporter.GetAtPath(path);
                importer.userData = folderData.iconNameOrGuid == "" && folderData.colorIndex == 0
                    ? ""
                    : JsonUtility.ToJson(folderData);
                importer.SaveAndReimport();
            }

            VFoldersLib.folderDatasFromMetaFiles_byGuid.Clear();
        }
        else
        {
            EditorUtility.SetDirty(VFoldersLib.data);
            AssetDatabase.SaveAssetIfDirty(VFoldersLib.data);
        }

        EditorApplication.RepaintProjectWindow();
    }
}
