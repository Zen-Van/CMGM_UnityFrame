using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using System.IO;

public class Edt_CreateScriptAssetAction : EndNameEditAction
{
    #region 各模板路径
    private const string TemplatesPath = Consts.Paths.RootPath + "/Editor/TemplateCreator/Templates";

    private const string Path_Empty = TemplatesPath + "/Empty.txt";
    private const string Path_MonoBehaviour = TemplatesPath + "/NewMonoBehaviour.cs.txt";
    private const string Path_CSharp = TemplatesPath + "/NewCSharp.cs.txt";
    #endregion


    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        //创建资源
        Object obj = CreateAssetFormTemplate(pathName, resourceFile);
        //高亮显示该资源
        ProjectWindowUtil.ShowCreatedAsset(obj);
    }


    internal static Object CreateAssetFormTemplate(string pathName, string resourceFile)
    {
        //获取要创建资源的绝对路径
        string fullName = Path.GetFullPath(pathName);
        //读取本地模版文件
        StreamReader reader = new StreamReader(resourceFile);
        string content = reader.ReadToEnd();
        reader.Close();

        //获取资源的文件名称
        string fileName = Path.GetFileNameWithoutExtension(pathName);
        //替换默认的文件名称
        content = content.Replace("#SCRIPTNAME#", fileName);

        //写入新文件
        StreamWriter writer = new StreamWriter(fullName, false, System.Text.Encoding.UTF8);
        writer.Write(content);
        writer.Close();

        //刷新本地资源
        AssetDatabase.ImportAsset(pathName);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
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

    #region 创建不同模板的按钮接口
    //------------------------------------------------------------
    [MenuItem("Assets/CMGM Create/Script_MonoBehaviour", false, -13)]
    [MenuItem("草木句萌/Create/Script_MonoBehaviour", false, -13)]
    static void CreateNewMonoScripts()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            CreateInstance<Edt_CreateScriptAssetAction>(),
            GetSelectedPath() + "/NewMonoBehaviour.cs", null,
            Path_MonoBehaviour);
    }
    [MenuItem("Assets/CMGM Create/Script_C#", false, -13)]
    [MenuItem("草木句萌/Create/Script_C#", false, -13)]
    static void CreateNewCSharpScripts()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            CreateInstance<Edt_CreateScriptAssetAction>(),
            GetSelectedPath() + "/NewCSharp.cs", null,
            Path_CSharp);
    }
    [MenuItem("Assets/CMGM Create/Script_Lua", false, -11)]
    [MenuItem("草木句萌/Create/Script_Lua", false, -11)]
    static void CreateNewLuaScripts()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            CreateInstance<Edt_CreateScriptAssetAction>(),
            GetSelectedPath() + "/NewLua.lua.txt", null,
            Path_Empty);
    }

    //------------------------------------------------------------

    [MenuItem("Assets/CMGM Create/文本文档", false, 0)]
    [MenuItem("草木句萌/Create/文本文档", false, 0)]
    static void CreateNewTxtScripts()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            CreateInstance<Edt_CreateScriptAssetAction>(),
            GetSelectedPath() + "/NewTxt.txt", null,
            Path_Empty);
    }
    #endregion
}
