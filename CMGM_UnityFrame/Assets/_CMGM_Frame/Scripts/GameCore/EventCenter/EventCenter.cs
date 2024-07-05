using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

/*
关于GmgmEvent的说明： 
是较低层的委托封装成的事件系统，与UnityEvent对应，应与业务层的GameEvent相区分。（GameEvent指具体的游戏中可能发生的任何情况）
它作为多UnityAction的管理，并统一触发。
在项目中，UnityAcion仅储存单函数信息，方便获取TargetType与MethodInfo。每条Action即为GmgmEvent中的一条监听。
 */

public class EventCenter : Singleton<EventCenter>
{
    private EventCenter() { }
    private Dictionary<E_EventDef, GmgmEventBase> eventDic = new Dictionary<E_EventDef, GmgmEventBase>();

    /// <summary>
    /// 为某事件添加监听
    /// </summary>
    /// <param name="evtDef">事件名</param>
    /// <param name="func">监听函数</param>
    public void AddEventListener<T>(E_EventDef evtDef, UnityAction<T> func)
    {
        if (eventDic.ContainsKey(evtDef))
        {
            (eventDic[evtDef] as GmgmEvent<T>).actions += func;
        }
        else
        {
            eventDic.Add(evtDef, new GmgmEvent<T>(evtDef, func));
        }
#if UNITY_EDITOR
        eventDic[evtDef].AddListenerInfo(func.Target.GetType(), func.Method); //添加编辑器需要的监听信息
        //刷新编辑器UI
        evtTreeView?.Reload();
#endif
    }
    /// <summary>
    /// 为某事件添加监听
    /// </summary>
    /// <param name="evtDef">事件名</param>
    /// <param name="func">监听函数</param>
    public void AddEventListener(E_EventDef evtDef, UnityAction func)
    {
        if (eventDic.ContainsKey(evtDef))
        {
            (eventDic[evtDef] as GmgmEvent).actions += func;
        }
        else
        {
            eventDic.Add(evtDef, new GmgmEvent(evtDef, func));
        }
#if UNITY_EDITOR
        eventDic[evtDef].AddListenerInfo(func.Target.GetType(), func.Method); //添加编辑器需要的监听信息
        //刷新编辑器UI
        evtTreeView?.Reload();
#endif
    }

    /// <summary>
    /// 移除某事件的某一监听
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="func">监听函数</param>
    public void RemoveEventListener(E_EventDef eventName, UnityAction func)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as GmgmEvent).actions -= func;
#if UNITY_EDITOR
            (eventDic[eventName] as GmgmEvent).RemoveListenerInfo(func.Target.GetType(), func.Method);
            //刷新编辑器UI
            evtTreeView?.Reload();
#endif
        }
        else
            CmgmLog.FrameLogWarning($"试图从不存在的event：{eventName}中移除监听函数： {func.Target.GetType()} -> {func.Method.Name}");

    }
    /// <summary>
    /// 移除某事件的某一监听
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="func">监听函数</param>
    public void RemoveEventListener<T>(E_EventDef eventName, UnityAction<T> func)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as GmgmEvent<T>).actions -= func;
#if UNITY_EDITOR
            (eventDic[eventName] as GmgmEvent).RemoveListenerInfo(func.Target.GetType(), func.Method);
            //刷新编辑器UI
            evtTreeView?.Reload();
#endif
        }
        else
            CmgmLog.FrameLogWarning($"试图从{eventName}中移除不存在的监听函数： {func.Target.GetType()} -> {func.Method.Name}");
    }

    /// <summary>
    /// 完全移除一个事件
    /// </summary>
    /// <param name="eventName"></param>
    public void RemoveEvent(E_EventDef eventName)
    {
        //断开某一事件的引用，使其进入GC流程
        if (eventDic.ContainsKey(eventName))
        {
            eventDic.Remove(eventName);
#if UNITY_EDITOR
            //刷新编辑器UI
            evtTreeView?.Reload();
#endif
        }
        else
            CmgmLog.FrameLogWarning($"试图移除不存在的事件{eventName}");

    }
    /// <summary>
    /// 清空所有事件
    /// </summary>
    public void Clear()
    {
        //断开所有事件的引用，使他们进入GC流程
        eventDic.Clear();

#if UNITY_EDITOR
        //刷新编辑器UI
        evtTreeView?.Reload();
#endif
    }

    /// <summary>
    /// 触发事件
    /// </summary>
    /// <typeparam name="T">事件参数类型</typeparam>
    /// <param name="eventName">事件名</param>
    /// <param name="info">事件参数</param>
    public void EventInvoke<T>(E_EventDef eventName, T info)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as GmgmEvent<T>).actions?.Invoke(info);
        }
        else
        {
            CmgmLog.FrameLogWarning($"试图调用不存在的事件{eventName}");
        }
    }
    /// <summary>
    /// 触发事件
    /// </summary>
    /// <param name="eventName">事件名</param>
    public void EventInvoke(E_EventDef eventName)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as GmgmEvent).actions?.Invoke();
        }
        else
        {
            CmgmLog.FrameLogWarning($"试图调用不存在的事件{eventName}");
        }
    }



    //----------下列是当前仅有TreeView调用的函数，后续有必要可以开启外部访问权限-------------

    private List<E_EventDef> GetRigistedEventDef()
    {
        return eventDic.Keys.ToList();
    }
    private List<E_EventDef> GetAllDefEvents()
    {
        return Enum.GetValues(typeof(E_EventDef)).Cast<E_EventDef>().ToList();
    }








    //事件监听信息 存储读取相关

#if UNITY_EDITOR

    //------------------------------------TreeView显示数据-----------------------------------------

    private class Edt_EventTreeView : UnityEditor.IMGUI.Controls.TreeView
    {
        public Edt_EventTreeView(UnityEditor.IMGUI.Controls.TreeViewState state) : base(state)
        {
            Reload();
        }

        protected override UnityEditor.IMGUI.Controls.TreeViewItem BuildRoot()
        {
            var root = new UnityEditor.IMGUI.Controls.TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            int autoId = 1;

            List<E_EventDef> allDefEvents = Instance.GetAllDefEvents();//所有事件定义
            List<E_EventDef> rigistedEvents = Instance.GetRigistedEventDef();//注册的事件
            int allCount = allDefEvents.Count;
            int rigistedCount = rigistedEvents.Count;

            var rigistedRoot = new UnityEditor.IMGUI.Controls.TreeViewItem { displayName = $"当前注册的事件----------事件数目：{rigistedCount}", id = autoId++ };
            root.AddChild(rigistedRoot);
            var unrigistedRoot = new UnityEditor.IMGUI.Controls.TreeViewItem { displayName = $"声明但未注册的事件--------事件数目：{allCount - rigistedCount}", id = autoId++ };
            root.AddChild(unrigistedRoot);

            //每一个事件
            foreach (var evtDef in allDefEvents)
            {
                if (rigistedEvents.Contains(evtDef))  //若注册了
                {
                    GmgmEventBase evt = Instance.eventDic[evtDef];

                    List<(Type, MethodInfo)> infos = evt.GetEventInfoList();//事件中的action信息列表
                    var evtItem = new UnityEditor.IMGUI.Controls.TreeViewItem { displayName = $"事件：{evt.eventName}---------监听数目：{evt.ListenerCount}", id = autoId++ };

                    //如果注册的事件下已有分类的子节点，那么在子节点下新增事件节点
                    if (rigistedRoot.children != null &&
                        rigistedRoot.children.Exists(node => node.displayName == evt.eventType))
                    {
                        rigistedRoot.children.Find(node => node.displayName == evt.eventType).AddChild(evtItem);
                    }
                    else//不然新增子节点并新增事件节点
                    {
                        var evtTypeItem = new UnityEditor.IMGUI.Controls.TreeViewItem { displayName = evt.eventType, id = autoId++ };
                        rigistedRoot.AddChild(evtTypeItem); //类型节点的添加
                        evtTypeItem.AddChild(evtItem);  //事件节点的添加
                    }

                    //为事件添加一堆监听
                    foreach ((Type, MethodInfo) info in infos)
                    {
                        var actionItem = new UnityEditor.IMGUI.Controls.TreeViewItem { displayName = $"监听注册路径为-> 函数类名：{info.Item1.Name}，函数名：{info.Item2.Name}", id = autoId++ };
                        evtItem.AddChild(actionItem);
                    }
                }
                else
                {
                    //若未注册 就不分类了
                    unrigistedRoot.AddChild(new UnityEditor.IMGUI.Controls.TreeViewItem { displayName = $"事件：{evtDef}", id = autoId++ });
                }
            }

            SetupDepthsFromParentsAndChildren(root);

            return root;
        }
    }
    private Edt_EventTreeView evtTreeView;

    /// <summary>
    /// 获得一个所有事件注册信息的TreeView对象
    /// </summary>
    /// <param name="treeViewState"></param>
    /// <returns></returns>
    public UnityEditor.IMGUI.Controls.TreeView GetEventTreeView(UnityEditor.IMGUI.Controls.TreeViewState treeViewState)
    {
        if (evtTreeView == null)
        {
            evtTreeView = new Edt_EventTreeView(treeViewState);
        }
        return evtTreeView;
    }

#endif
}
