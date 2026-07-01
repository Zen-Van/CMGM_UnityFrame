using System;
using CMGM.Core;
using CMGM.Story;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public class LuaBridge
{
    /// <summary>
    /// 一段Lua结束时自动调用
    /// </summary>
    public static void LuaExecuteFinished(string ret)
    {
        LuaManager.Instance.CompleteCurrentExecution(ret);
    }

    #region 交互&UI
    /// <summary>
    /// 对话命令
    /// </summary>
    /// <param name="roleId">说话角色ID</param>
    /// <param name="imgId">立绘差分编号</param>
    /// <param name="content">文本内容</param>
    public static void Talk(int roleId, int imgId, string content, Action callback)
    {
        // Lua → Host/LuaBridge → CMGM.Story.StoryDialogueManager（Story 不反向依赖 Lua）
        StoryDialogueManager.Talk(roleId, imgId, content, callback);
    }    /// <summary>
    /// 对话命令（默认立绘）
    /// </summary>
    /// <param name="roleId">说话角色ID</param>
    /// <param name="content">文本内容</param>
    public static void Talk(int roleId, string content, Action callback)
    {
        Talk(roleId, 0, content, callback);
    }

    #endregion

    #region 工具&测试
    public static void DebugLog(string content)
    {
        CmgmLog.fNormal(content);
    }
    public static void Wait(float sec, Action callback)
    {
        UniTask.WaitForSeconds(sec).ContinueWith(callback).Forget();
    }

    #endregion
}
