using System.IO;
using CMGM.Core;
using UnityEditor;
using UnityEngine;
using VFolderData = VFolders.VFoldersData;
using VFoldersLib = VFolders.VFolders;

/// <summary>
/// 项目初始化：为关键目录设置 vFolders 文件夹颜色。
/// </summary>
public static class Edt_VFolderColorSetup
{
    // vFolders 调色板索引，与当前工程四个顶层目录一致
    private static readonly (string path, int colorIndex)[] FrameworkFolderColors =
    {
        (Consts.Paths.WorkSpace, 1),
        (Consts.Paths.TestSpace, 4),
        (Consts.Paths.PackageRoot, 6),
        (Consts.Paths.PublicRes, 9),
    };

    public static int ApplyFrameworkFolderColors()
    {
        if (VFoldersLib.data == null)
        {
            CmgmLog.fNormal("[项目初始化] 未找到 vFolders Data，跳过文件夹着色。");
            return 0;
        }

        EnsureFoldersImported();

        int applied = 0;
        foreach (var (path, colorIndex) in FrameworkFolderColors)
        {
            if (!TryGetImportedFolderGuid(path, out _))
                continue;

            VFoldersLib.SetColor(path, colorIndex);
            applied++;
        }

        if (applied > 0)
            SaveFolderColorData();

        return applied;
    }

    private static void EnsureFoldersImported()
    {
        bool needsRefresh = false;
        foreach (var (path, _) in FrameworkFolderColors)
        {
            if (Directory.Exists(path) && string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)))
                needsRefresh = true;
        }

        if (needsRefresh)
            AssetDatabase.Refresh();
    }

    private static bool TryGetImportedFolderGuid(string path, out string guid)
    {
        guid = AssetDatabase.AssetPathToGUID(path);
        if (!string.IsNullOrEmpty(guid))
            return true;

        if (!Directory.Exists(path))
            return false;

        CmgmLog.fNormal($"[项目初始化] 文件夹未导入 AssetDatabase，跳过着色：{path}");
        return false;
    }

    private static void SaveFolderColorData()
    {
        if (VFolderData.storeDataInMetaFiles)
        {
            foreach (var (path, _) in FrameworkFolderColors)
            {
                if (!TryGetImportedFolderGuid(path, out string guid))
                    continue;

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
