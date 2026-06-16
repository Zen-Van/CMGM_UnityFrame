namespace CMGM.Core
{
    /// <summary>
    /// 跨模块可用的应用级操作（避免 Game / Level 程序集循环引用）。
    /// </summary>
    public static class CmgmApplication
    {
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
