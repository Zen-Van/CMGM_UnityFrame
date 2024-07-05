using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorResLoader : Singleton<EditorResLoader>
{
    private EditorResLoader() { }

    /// <summary>
    /// �ڱ༭��ģʽ�¼���AbRes�ļ����µ���Դ
    /// </summary>
    /// <typeparam name="T">��Դ����</typeparam>
    /// <param name="uri">�����AbRes�ļ��е�uri</param>
    /// <returns>���غõ���Դ</returns>
    public T LoadResInEditor<T>(string uri) where T : Object
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<T>(uri);
#else
        return null;
#endif
    }

    //TODO:ͼ�����ء������༭����Դ����

}
