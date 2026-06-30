using UnityEditor;

/// <summary>
/// 入门引导：每次 Domain Reload 后，若依赖未齐或项目未初始化则自动弹出窗口。
/// </summary>
[InitializeOnLoad]
public static class Edt_GettingStartedGate
{
    static Edt_GettingStartedGate()
    {
        EditorApplication.delayCall += OnEditorReady;
    }

    private static void OnEditorReady()
    {
        if (!Edt_GettingStartedProbe.ShouldShowGettingStarted())
            return;
        Edt_GettingStartedWindow.ShowWindow(Edt_GettingStartedProbe.Check());
    }
}
