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
    #region ��ģ��·��
    private const string TemplatesPath = Consts.Paths.RootPath + "/Editor/TemplateCreator/Templates";

    private const string TemplatePath_PanelScript = TemplatesPath + "/NewPanel.cs.txt";
    private const string TemplatePath_PanelPrefab = TemplatesPath + "/NewPanel.prefab.txt";
    #endregion

    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        string fileName = Path.GetFileNameWithoutExtension(pathName);
        //������Դ
        GameObject prefab = CreateAssetFormTemplate<GameObject>(pathName, resourceFile,false);
        MonoBehaviour script = CreateAssetFormTemplate<MonoBehaviour>
            (Consts.Paths.Script_UI_Panel_Path + "/" + fileName + ".cs", TemplatePath_PanelScript, true);

        //������ʾ����Դ
        ProjectWindowUtil.ShowCreatedAsset(prefab);
        ProjectWindowUtil.ShowCreatedAsset(script);
    }

    internal static T CreateAssetFormTemplate<T>(string pathName, string resourceFile,bool isScript) where T : UnityEngine.Object
    {
        //��ȡҪ������Դ�ľ���·��
        string fullName = Path.GetFullPath(pathName);
        //��ȡ����ģ���ļ�
        StreamReader reader = new StreamReader(resourceFile);
        string content = reader.ReadToEnd();
        reader.Close();

        if(isScript)
        {
            //��ȡ��Դ���ļ�����
            string fileName = Path.GetFileNameWithoutExtension(pathName);
            //�滻Ĭ�ϵ��ļ�����
            content = content.Replace("#SCRIPTNAME#", fileName);
        }

        //д�����ļ�
        StreamWriter writer = new StreamWriter(fullName, false, System.Text.Encoding.UTF8);
        writer.Write(content);
        writer.Close();

        //ˢ�±�����Դ
        AssetDatabase.ImportAsset(pathName);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<T>(pathName);
    }

    [MenuItem("Assets/CMGM Create/UI_NewPanel", false, -100)]
    [MenuItem("��ľ����/Create/UI_NewPanel", false, -100)]
    static void CreateNewPanelScripts()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            CreateInstance<Edt_CreateUIPanelAction>(),
            Consts.Paths.Ab_UI_Panel_Path + "/" + "NewPanel.prefab", null,
            TemplatePath_PanelPrefab);
    }
}
