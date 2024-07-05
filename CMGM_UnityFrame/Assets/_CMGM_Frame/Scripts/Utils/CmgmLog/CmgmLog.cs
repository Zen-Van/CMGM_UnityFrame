using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制台信息打印的封装
/// （方便管理是否开启控制台打印、方便管理编辑器模式和运行时模式下控制台打印的区别）
/// </summary>
public class CmgmLog
{
    /// <summary>
    /// 游戏发布后可以关闭，节约性能并防止玩家通过一些手段看到控制台信息
    /// </summary>
    public static bool IS_LOG_ACTIVE = true;

    public static void FrameLogPositive(string log)
    {
        if (!IS_LOG_ACTIVE) return;

        Debug.Log($"<color=#00FFFF>[蝉的框架]</color>：{log}");
    }
    public static void FrameLog_Negative(string log)
    {
        if(!IS_LOG_ACTIVE) return;

        Debug.Log($"<color=#e0c110>[蝉的框架]</color>：{log}");
    }
    public static void FrameLogWarning(string log)
    {
        if (!IS_LOG_ACTIVE) return;

        Debug.LogWarning($"<color=#d69509>[蝉的框架]</color>：{log}");
    }
    
    public static void FrameLogError(string log)
    {
        if(!IS_LOG_ACTIVE) return;

        Debug.LogError($"<color=#FF7F00>[蝉的框架]</color>：{log}");
    }

    public static void TODO(string log)
    {
        if (!IS_LOG_ACTIVE) return;

        Debug.Log($"<color=#d4eb07>[TODO]</color>：{log}");
    }
}
