using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class MapInteractable : MapTriggerBase
{
    /// <summary>
    /// 对该可交互物触发交互时的执行函数
    /// </summary>
    [NonSerialized]
    public UnityAction OnInteracted;
}