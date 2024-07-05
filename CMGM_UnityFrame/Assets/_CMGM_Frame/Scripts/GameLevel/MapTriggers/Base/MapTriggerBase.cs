using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


[RequireComponent(typeof(Collider2D))]
public abstract class MapTriggerBase : SerializedMonoBehaviour
{
    [ShowInInspector]
    [ReadOnly]
    /// <summary> 用于标识地图事件的ID，全局唯一 </summary>
    public string UUID = System.Guid.NewGuid().ToString();

    /// <summary>
    /// 地图事件的触发优先级
    /// </summary>
    public int Priority;
}
