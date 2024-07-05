using System;
using System.Collections;
using System.Collections.Generic;
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
        var s = LuaManager.Instance.CurrentEventSourceStack.Pop();
        s.TrySetResult(ret);//将弹出的标记设置为已完成，并把返回值返回到C#一侧
        CmgmLog.FrameLogPositive("结束了该lua脚本的调用");
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
        UniTask.Void(async () =>
        {
            //显示UI面板
            var panel = await UIManager.Instance.ShowPanel<DialogPanel>();
            //触发UI面板逐字打印，并在逐字打印结束后调用结束回调
            await panel.PrintContent(roleId, content, imgId);
            //隐藏面板
            UIManager.Instance.HidePanel<DialogPanel>(false);
            //通知lua，命令完成
            callback?.Invoke();
        });
    }
    /// <summary>
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
        CmgmLog.FrameLogPositive(content);
    }
    public static void Wait(float sec, Action callback)
    {
        UniTask.WaitForSeconds(sec).ContinueWith(callback).Forget();
    }


    #endregion
}
