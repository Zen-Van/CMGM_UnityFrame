using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class Edt_EventCenterWindow : EditorWindow
{
    [MenuItem("草木句萌/GameEvent管理器")]
    public static void ShowWindow()
    {
        Edt_EventCenterWindow window = GetWindow<Edt_EventCenterWindow>("GameEvent管理器");
        window.m_EventTreeView.Reload();
        window.Show();
    }

    [SerializeField] TreeViewState m_TreeViewState;
    TreeView m_EventTreeView;

    private void OnEnable()
    {
        if(m_TreeViewState ==null)
            m_TreeViewState = new TreeViewState();

        m_EventTreeView = EventCenter.Instance.GetEventTreeView(m_TreeViewState);
    }

    private void OnGUI()
    {
        m_EventTreeView.OnGUI(new Rect(0, 0, position.width, position.height));
    }
}
