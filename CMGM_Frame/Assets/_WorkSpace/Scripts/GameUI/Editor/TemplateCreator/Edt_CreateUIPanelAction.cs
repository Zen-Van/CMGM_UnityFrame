using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

public class Edt_CreateUIPanelAction : EndNameEditAction
{
    #region 各模板路径
    private const string TemplatesPath = Edt_BaseUtils.EditorRoot + "/TemplateCreator/Templates";
    private const string TemplatePath_PanelScript = TemplatesPath + "/NewPanel.cs.txt";
    private const string TemplatePath_PanelPrefab = TemplatesPath + "/NewPanel.prefab.txt";
    #endregion

    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        string fileName = Path.GetFileNameWithoutExtension(pathName);
        string scriptPath = Consts.Paths.Script_UI_Panel_Path + "/" + fileName + ".cs";
        //创建资源        
        MonoScript script = CreateAssetFormTemplate<MonoScript>
            (scriptPath, TemplatePath_PanelScript, true);

        GameObject prefab = CreateAssetFormTemplate<GameObject>(pathName, resourceFile, false);

        EditorPrefs.SetString("RawUiPrefabPath", pathName);
        EditorPrefs.SetString("RawUiScriptPath", scriptPath);

        //高亮显示资源
        ProjectWindowUtil.ShowCreatedAsset(prefab);
        ProjectWindowUtil.ShowCreatedAsset(script);
    }

    internal static T CreateAssetFormTemplate<T>(string pathName, string resourceFile, bool isScript) where T : UnityEngine.Object
    {
        //获取要创建的资源的绝对路径
        string fullName = Path.GetFullPath(pathName);
        //读取本地模板文件
        StreamReader reader = new StreamReader(resourceFile);
        string content = reader.ReadToEnd();
        reader.Close();

        if (isScript)
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
            Consts.Paths.HotRes_UIPanel + "/" + "NewPanel.prefab", null,
            TemplatePath_PanelPrefab);
    }

    /// <summary>
    /// 在编译完成后，自动添加脚本到预制体
    /// </summary>
    [InitializeOnLoadMethod]
    static void OnCompiled()
    {
        string prefabPath = EditorPrefs.GetString("RawUiPrefabPath", null);
        string scriptPath = EditorPrefs.GetString("RawUiScriptPath", null);

        //如果没有需要处理的UI绑定，直接返回即可
        if (string.IsNullOrEmpty(prefabPath) || string.IsNullOrEmpty(scriptPath))
            return;

        // 加载预制体
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);

        // 资源校验
        if (prefab == null)
        {
            CmgmLog.fError($"预制体加载失败：{prefabPath}");
            return;
        }

        if (script == null)
        {
            CmgmLog.fError($"脚本加载失败：{scriptPath}");
            return;
        }

        // 获取脚本类型
        Type componentType = script.GetClass();
        if (componentType == null || !typeof(MonoBehaviour).IsAssignableFrom(componentType))
        {
            CmgmLog.fError("脚本类型无效或不是MonoBehaviour派生类");
            return;
        }

        // 编辑预制体
        GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);

        try
        {
            // 检查是否已存在该组件
            if (prefabInstance.GetComponent(componentType) == null)
            {
                // 添加组件
                prefabInstance.AddComponent(componentType);

                // 保存修改
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                CmgmLog.fPositive($"成功添加 {componentType.Name} 组件到预制体");
            }
            else
            {
                CmgmLog.fNormal($"预制体已包含 {componentType.Name} 组件，无需重复添加");
            }
        }
        finally
        {
            // 卸载预制体内容
            PrefabUtility.UnloadPrefabContents(prefabInstance);
        }

        //删除临时数据
        EditorPrefs.DeleteKey("RawUiPrefabPath");
        EditorPrefs.DeleteKey("RawUiScriptPath");
    } 
}
