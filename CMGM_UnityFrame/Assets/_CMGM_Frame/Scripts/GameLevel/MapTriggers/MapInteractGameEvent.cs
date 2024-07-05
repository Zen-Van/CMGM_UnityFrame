using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

//共有两种游戏事件，交互触发/进入触发,这是交互触发
public class MapGameEvent : MapInteractable
{
    public enum TriggerType
    {
        Entry,
        Interact
    }
    public TriggerType triggerType;

    [BoxGroup("状态与事件")]
    [LabelText("事件触发器状态")]
    [ShowInInspector]
    [ReadOnly]
    private int curState = 0;

    [InfoBox("根据状态触发事件")]
    [BoxGroup("状态与事件")]
    [DictionaryDrawerSettings(KeyLabel = "状态编号", ValueLabel = "事件名")]
    public Dictionary<int, string> EventList = new Dictionary<int, string>();




    private void OnEnable()
    {
        if (triggerType == TriggerType.Interact)
            OnInteracted += OnTriggered;
    }
    private void OnDisable()
    {
        if (triggerType == TriggerType.Interact)
            OnInteracted -= OnTriggered;
    }

    private void OnTriggered()
    {
        if (EventList.ContainsKey(curState))
            LuaManager.Instance.ExecuteLua($"GameEvents/{EventList[curState]}.lua.txt").Forget();
        else
            LuaManager.Instance.ExecuteLua($"GameEvents/{EventList[0]}.lua.txt").Forget();
    }

}
