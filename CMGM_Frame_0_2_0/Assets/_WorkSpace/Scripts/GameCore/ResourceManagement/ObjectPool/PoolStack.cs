using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolStack
{
    private Stack<GameObject> stack = new Stack<GameObject>();

    private GameObject objStackRoot;

    public int Count => stack.Count;

    public PoolStack(GameObject poolRoot, string stackName)
    {
        if (!PoolManager.AUTO_LAYOUT_IN_HIERACHY) return;
        objStackRoot = new GameObject(stackName);
        objStackRoot.transform.SetParent(poolRoot.transform);
    }

    public GameObject Pop()
    {
        GameObject obj = stack.Pop();

        obj.SetActive(true);
        if (PoolManager.AUTO_LAYOUT_IN_HIERACHY)
            obj.transform.SetParent(null);

        return obj;
    }

    public void Push(GameObject obj)
    {
        //失活而不是销毁节约性能，还可以采取将其放置至摄像机外的地方
        obj.SetActive(false);
        if (PoolManager.AUTO_LAYOUT_IN_HIERACHY)
            obj.transform.SetParent(objStackRoot.transform);

        stack.Push(obj);
    }
}
