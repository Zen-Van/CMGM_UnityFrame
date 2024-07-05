using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Custom.Collections;

public class MonoManager : SingletonAutoMono<MonoManager>
{
    private List<(UnityAction, int)> updateActions = new List<(UnityAction, int)>();
    private PriorityQueue<UnityAction, int> updateActionsCopy = new PriorityQueue<UnityAction, int>();

    /// <summary>
    /// 将函数注册入Update,并赋予执行的优先级
    /// </summary>
    /// <param name="priority">无额外要求时为0</param>
    public void AddUpdateListener(UnityAction updateFun, int priority = 0)
    {
        updateActions.Add((updateFun, priority));
    }

    /// <summary>
    /// 从Update中删除函数，相同的函数每次只会删除一个
    /// </summary>
    /// <param name="updateFun"></param>
    /// <param name="priority"></param>
    public void RemoveUpdateListener(UnityAction updateFun, int priority)
    {
        updateActions.Remove((updateFun, priority));
    }

    private void Update()
    {
        //将列表里的内容放进优先队列
        for (int i = 0; i < updateActions.Count; i++)
        {
            updateActionsCopy.Enqueue(updateActions[i].Item1, updateActions[i].Item2);//按照优先级添加委托
        }

        if (updateActions.Count > 0)
        {
            //按照优先级的先后顺序依次调用
            while (updateActionsCopy.TryDequeue(out var action, out var priority))
            {
                action.Invoke();
            }
        }
    }
}
