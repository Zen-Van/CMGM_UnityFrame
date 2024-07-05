using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    private InputManager()
    {
        inputActions.Enable();
    }
    ~InputManager()
    { 
        inputActions.Disable();
    }

    private InputActions_Main inputActions = new InputActions_Main();
    /// <summary> 所有玩家输入集 </summary>
    public InputActions_Main.GamePlayActions Gameplay => inputActions.GamePlay;
    /// <summary> 所有UI输入集 </summary>
    public InputActions_Main.UIActions UI=>inputActions.UI;
}
