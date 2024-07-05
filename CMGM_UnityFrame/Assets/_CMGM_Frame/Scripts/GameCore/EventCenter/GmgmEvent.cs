using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

/*
关于GmgmEvent的说明： 
是较低层的委托封装成的事件系统，与UnityEvent对应，应与业务层的GameEvent相区分。（GameEvent指具体的游戏中可能发生的任何情况）
GmgmEvent作为多UnityAction的管理，并统一触发。
在项目中，UnityAcion仅储存单函数信息，方便获取TargetType与MethodInfo。每条Action即为GmgmEvent中的一条监听。
 */

#region 通过里氏替换原则兼容有无泛型的两种对象，泛型事件只提供单泛型，如果需要传入多个变量请使用元表
public abstract class GmgmEventBase
{
    //用来编辑器打印事件相关的信息，不需要打包进游戏

    //事件名字和类型在事件枚举tostring后用“_”分割 Type_Name 初始化子类时赋值
    public string eventName { get; protected set; }
    public string eventType { get; protected set; }

#if UNITY_EDITOR
    /// <summary>
    /// 所有监听路径记录
    /// <para>item1：监听函数所在的类，item2：监听的函数</para>
    /// </summary>
    List<(Type, MethodInfo)> listenersInfo = new List<(Type, MethodInfo)>();
    /// <summary>
    /// 监听注册的总数目
    /// </summary>
    public int ListenerCount => listenersInfo.Count;

    //【TODO】补充其他Event相关的信息方便在控制台查询如：

    //事件注册记录（该Event首次被初始化的时间）

    //事件调用记录（该Event每次触发）



    /// <summary>
    /// 为事件新增一条监听信息
    /// </summary>
    /// <param name="listener">监听器</param>
    /// <param name="method">监听函数</param>
    public void AddListenerInfo(Type listener, MethodInfo method)
    {
        listenersInfo.Add((listener, method));
    }
    /// <summary>
    /// 移除一条已有的监听信息
    /// </summary>
    /// <param name="listener">监听器</param>
    /// <param name="method">监听函数</param>
    public void RemoveListenerInfo(Type listener, MethodInfo method)
    {
        if (listenersInfo.Contains((listener, method)))
            listenersInfo.Remove((listener, method));
        else
            CmgmLog.FrameLogWarning($"事件{this}中不存在监听({listener.Name},{method.Name})");
    }

    /// <summary>
    /// 获得该事件的所有监听信息列表
    /// </summary>
    public List<(Type, MethodInfo)> GetEventInfoList()
    {
        return listenersInfo;
    }
#endif
}

public class GmgmEvent : GmgmEventBase
{
    public UnityAction actions;
    public GmgmEvent(E_EventDef def, UnityAction action)
    {
        actions += action;

        string[] parts = def.ToString().Split('_');
        eventType = parts[0];
        eventName = parts[1];
    }

    public void Invoke()
    {
        actions?.Invoke();
    }

}

public class GmgmEvent<T> : GmgmEventBase
{
    public UnityAction<T> actions;
    public GmgmEvent(E_EventDef def, UnityAction<T> action)
    {
        actions += action;

        string[] parts = def.ToString().Split('_');
        eventType = parts[0];
        eventName = parts[1];
    }

    public void Invoke(T arg)
    {
        actions?.Invoke(arg);
    }
}
#endregion