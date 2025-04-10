using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// 需要指定字体的路径 名字   路径放到Resources文件夹中
/// </summary>
public class Edt_FontsTool : EditorWindow
{
    //替换场景内的所有字体
    [MenuItem("草木句萌/TMPTools/替换场景中所有tmp字体为默认字体")]
    public static void ChangeFont_Scene()
    {
        //加载目标字体  "目标字体的名字"      
        TMP_FontAsset targetFont = TMP_Settings.defaultFontAsset;
        //获取场景所有激活物体
        //GameObject[] objs = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        //获取场景所有物体
        GameObject[] allObj = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
        TMP_Text tmpText;
        int textCount = 0;
        for (int i = 0; i < allObj.Length; i++)
        {
            //带有Text组件的GameObject，替换字体
            tmpText = allObj[i].GetComponent<TMP_Text>();
            if (tmpText != null)
            {
                textCount++;
                tmpText.font = targetFont;
                //在此扩展，可以给添加外边框，也可以根据需求进行其他操作
                //allObj[i].AddComponent<Outline>();
            }
        }
        Debug.Log("<color=yellow> 当前场景共有：物体 </color>" + allObj.Length + "<color=yellow> 个，TMP_Text组件 </color>" + textCount + "<color=green> 个 </color>");
    }
    //替换资源文件夹中全部Prefab的字体
    [MenuItem("草木句萌/TMPTools/替换预制体中所有tmp字体为默认字体")]
    public static void ChangeFont_Prefab()
    {
        TMP_FontAsset targetFont = TMP_Settings.defaultFontAsset;
        List<TMP_Text[]> textList = new List<TMP_Text[]>();
        //获取Asset文件夹下所有Prefab的GUID
        string[] ids = AssetDatabase.FindAssets("t:Prefab");
        string tmpPath;
        GameObject tmpObj;
        TMP_Text[] tmpArr;
        for (int i = 0; i < ids.Length; i++)
        {
            tmpObj = null;
            tmpArr = null;
            //根据GUID获取路径
            tmpPath = AssetDatabase.GUIDToAssetPath(ids[i]);
            if (!string.IsNullOrEmpty(tmpPath))
            {
                //根据路径获取Prefab(GameObject)
                tmpObj = AssetDatabase.LoadAssetAtPath(tmpPath, typeof(GameObject)) as GameObject;
                if (tmpObj != null)
                {
                    //获取Prefab及其子物体孙物体.......的所有Text组件
                    tmpArr = tmpObj.GetComponentsInChildren<TMP_Text>();
                    if (tmpArr != null && tmpArr.Length > 0)
                        textList.Add(tmpArr);
                }
            }
        }
        //替换所有Text组件的字体
        int textCount = 0;
        for (int i = 0; i < textList.Count; i++)
        {
            for (int j = 0; j < textList[i].Length; j++)
            {
                textCount++;
                textList[i][j].font = targetFont;
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"<color=yellow> 当前Project共有：Prefab </color>{ids.Length}<color=yellow> 个，带有TMP_Text组件Prefab</color> {textList.Count} <color=yellow>个</color>，<color=green>TMP_Text组件 </color>{textCount}<color=green> 个 </color>");
    }
}