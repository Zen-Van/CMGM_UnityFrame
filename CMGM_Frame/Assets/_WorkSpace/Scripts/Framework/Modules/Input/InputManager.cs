using CMGM.Core;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CMGM.Input
{

public class InputManager : SingletonAutoMono<InputManager>
{
    protected override void Awake()
    {
        base.Awake();

        inputActions = new InputActions_Main();
        inputActions.Enable();
    }

    private InputActions_Main inputActions;
    /// <summary> 所有玩家输入集 </summary>
    public InputActions_Main.GamePlayActions Gameplay => inputActions.GamePlay;
    /// <summary> 所有UI输入集 </summary>
    public InputActions_Main.UIActions UI => inputActions.UI;

    /// <summary>绑定 UI Cancel 输入（屏蔽 Unity.InputSystem 类型，避免调用方程序集引用 Input System）。</summary>
    public void BindUiCancel(Action callback)
    {
        UI.Cancel.started += _ => callback();
    }
}
}
