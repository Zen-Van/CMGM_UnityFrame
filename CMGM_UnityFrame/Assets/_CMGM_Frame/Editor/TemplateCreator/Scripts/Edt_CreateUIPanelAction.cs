using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

public class Edt_CreateUIPanelAction : EndNameEditAction
{
    #region 各模板路径
    private const string TemplatesPath = Consts.Paths.RootPath + "/Editor/TemplateCreator/Templates";

    private const string TemplatePath_PanelScript = TemplatesPath + "/NewPanel.cs.txt";
    private const string TemplatePath_PanelPrefab = TemplatesPath + "/NewPanel.prefab.txt";
    #endregion

    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        string fileName = Path.GetFileNameWithoutExtension(pathName);
        //创建资源
        GameObject prefab = CreateAssetFormTemplate<GameObject>(pathName, resourceFile,false);
        MonoBehaviour script = CreateAssetFormTemplate<MonoBehaviour>
            (Consts.Paths.Script_UI_Panel_Path + "/" + fileName + ".cs", TemplatePath_PanelScript, true);

        //高亮显示该资源
        ProjectWindowUtil.ShowCreatedAsset(prefab);
        ProjectWindowUtil.ShowCreatedAsset(script);
    }

    internal static T CreateAssetFormTemplate<T>(string pathName, string resourceFile,bool isScript) where T : UnityEngine.Object
    {
        //获取要创建资源的绝对路径
        string fullName = Path.GetFullPath(pathName);
        //读取本地模版文件
        StreamReader reader = new StreamReader(resourceFile);
        string content = reader.ReadToEnd();
        reader.Close();

        if(isScript)
        {
            //获取资源的文件名称
            string fileName = Path.GetFileNameWithoutExtension(pathName);
            //替换默认的文件名称
            content = content.Replace("#SCRIPTNAME#", fileName);
        }

        //写入新文件
        StreamWriter writer = new StreamWriter(fullName, false, System.Text.Encoding.UTF8);
        writer.Write(content);
        writer.Close();

        //刷新本地资源
        AssetDatabase.ImportAsset(pathName);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<T>(pathName);
    }

    [MenuItem("Assets/CMGM Create/UI_NewPanel", false, -100)]
    [MenuItem("草木句萌/Create/UI_NewPanel", false, -100)]
    static void CreateNewPanelScripts()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            CreateInstance<Edt_CreateUIPanelAction>(),
            Consts.Paths.Ab_UI_Panel_Path + "/" + "NewPanel.prefab", null,
            TemplatePath_PanelPrefab);
    }
}
