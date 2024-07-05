using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BasePanel : MonoBehaviour
{
    [Description("动态面板指可以随时被呼出或关闭的面板，按ESC会关闭顶层的动态面板")]
    public bool isDynamic = false;

    /// <summary>
    /// 以该列表中名称命名的GameObject，不会被代码所控制和管理，只起到显示作用，以节约程序操作与内存占用
    /// </summary>
    private static List<string> ignoreNameList = new List<string>()
    {
        "Image","Text (TMP)","RawImage","Background","Checkmark","Label",
        "Text (Legacy)","Arrow","Placeholder","Fill","Temp","Handle",
        "Viewport","Scrollbar Horizontal","Scrollbar Vertical"
    };

    //值可以改成List<UIBehaviour>控制，这样可以存储一个对象的多个控件，但更加耗费性能
    protected Dictionary<string, UIBehaviour> controlDic = new Dictionary<string, UIBehaviour>();


    protected virtual void Awake()
    {
        //不用List<UIBehaviour>控制时，因GameObj与Control唯一映射，应优先查找有事件监听的“主要控件”
        FindValidChildrenControls<Button>();
        FindValidChildrenControls<Toggle>();
        FindValidChildrenControls<Slider>();
        FindValidChildrenControls<InputField>();
        FindValidChildrenControls<ScrollRect>();
        FindValidChildrenControls<Dropdown>();
        //不用List<UIBehaviour>控制时，上述"主要控件"在使用过程中应保证不要在一个GameObj上挂载两个
        //一般需要新建子物体来挂载新的具有“主要控件”的对象
        FindValidChildrenControls<Text>();
        FindValidChildrenControls<TMP_Text>();
        FindValidChildrenControls<Image>();
    }
    protected virtual void OnButtonClick(string btnName) { }
    protected virtual void OnSliderValueChanged(string sliderName, float value) { }
    protected virtual void OnToggleValueChanged(string sliderName, bool value) { }


    private void FindValidChildrenControls<T>() where T : UIBehaviour
    {
        T[] controls = GetComponentsInChildren<T>(true);
        for (int i = 0; i < controls.Length; i++)
        {
            string controlName = controls[i].gameObject.name;

            //该对象被忽略或该对象已经纳入管理，则不做操作
            if (ignoreNameList.Contains(controlName)) continue;
            if (controlDic.ContainsKey(controlName)) continue;

            controlDic.Add(controlName, controls[i]);

            #region 批量添加事件监听的操作，后续可新增...
            if (controls[i] is Button)
            {
                (controls[i] as Button).onClick.AddListener(() =>
                {
                    OnButtonClick(controlName);
                });
            }
            else if (controls[i] is Slider)
            {
                (controls[i] as Slider).onValueChanged.AddListener(value =>
                {
                    OnSliderValueChanged(controlName, value);
                });
            }
            else if (controls[i] is Toggle)
            {
                (controls[i] as Toggle).onValueChanged.AddListener(value =>
                {
                    OnToggleValueChanged(controlName, value);
                });
            }
            #endregion
        }
    }


    /// <summary>
    /// 在面板下根据名称和控件类型获取其主控件
    /// </summary>
    /// <typeparam name="T">控件类型</typeparam>
    /// <param name="name">对象名称</param>
    public T GetControl<T>(string name) where T : UIBehaviour
    {
        if (controlDic.ContainsKey(name))
        {
            T control = controlDic[name] as T;
            if (control == null)
                CmgmLog.FrameLogError($"不存在对应名字{name}类型为{typeof(T)}的组件");
            return control;
        }
        else
        {
            CmgmLog.FrameLogError($"不存在对应名字{name}的组件");
            return null;
        }
    }

    /// <summary>面板显示时调用</summary>
    public virtual void OnShow() { }
    /// <summary>面板隐藏时调用</summary>
    public virtual void OnHide() { }
}
