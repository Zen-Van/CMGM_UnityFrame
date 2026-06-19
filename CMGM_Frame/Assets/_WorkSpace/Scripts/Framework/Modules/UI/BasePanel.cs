using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CMGM.Core;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CMGM.UI
{
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
    /// <summary>
    /// 该列表内的类，才会被扫描到
    /// </summary>
    private static readonly HashSet<System.Type> NeedBindTypes = new()
    {
        typeof(Button),
        typeof(Slider),
        typeof(Toggle),
        typeof(InputField),
        typeof(ScrollRect),
        typeof(Dropdown),
        typeof(Text),
        typeof(TMP_Text),
        typeof(Image),
        typeof(TMP_InputField)   // 按需继续加，所有希望扫描到的类型
    };

    /*//值可以进一步改成List<UIBehaviour>列表控制，这样可以存储一个对象的多个控件，但更加耗费性能
    protected Dictionary<string, UIBehaviour> controlDic = new Dictionary<string, UIBehaviour>();*/


    protected virtual void Awake()
    {
        /*//不用List<UIBehaviour>列表控制时，因GameObj与Control唯一映射，应优先查找有事件监听的“主要控件”
        FindValidChildrenControls<Button>();
        FindValidChildrenControls<Toggle>();
        FindValidChildrenControls<Slider>();
        FindValidChildrenControls<InputField>();
        FindValidChildrenControls<ScrollRect>();
        FindValidChildrenControls<Dropdown>();
        //不用List<UIBehaviour>列表控制时，上述"主要控件"在使用过程中应保证不要在一个GameObj上挂载两个
        //一般需要新建子物体来挂载新的具有“主要控件”的对象
        FindValidChildrenControls<Text>();
        FindValidChildrenControls<TMP_Text>();
        FindValidChildrenControls<Image>();*/
        ScanOnceAndSplit();
        BindEvents();
    }
    protected virtual void OnDestroy()
    {
        UnbindEvents();   //删除注册事件
    }
    protected virtual void OnButtonClick(string btnName) { }
    protected virtual void OnSliderValueChanged(string sliderName, float value) { }
    protected virtual void OnToggleValueChanged(string sliderName, bool value) { }


    /*private void FindValidChildrenControls<T>() where T : UIBehaviour
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
    }*/


    /// <summary>
    /// 在面板下根据名称和控件类型获取其主控件
    /// </summary>
    /// <typeparam name="T">控件类型</typeparam>
    /// <param name="name">对象名称</param>
  /*  public T GetControl<T>(string name) where T : UIBehaviour
    {
        if (controlDic.ContainsKey(name))
        {
            T control = controlDic[name] as T;
            if (control == null)
                CmgmLog.fError($"不存在对应名字{name}类型为{typeof(T)}的组件");
            return control;
        }
        else
        {
            CmgmLog.fError($"不存在对应名字{name}的组件");
            return null;
        }
    }*/

    public T GetControl<T>(string name) where T : UIBehaviour
    {
        var type = typeof(T);
        if (!_cache.TryGetValue(type, out var array) || array == null)
        {
            CmgmLog.fError($"未扫描到类型 {type} 的控件");
            return null;
        }

        if (!_nameToIndex.TryGetValue((type, name), out var idx) || idx >= array.Length)
        {
            CmgmLog.fError($"不存在名字 {name} 的 {type}");
            return null;
        }

        return array[idx] as T;
    }
    #region 获取当前面板下的UI组件信息
    private readonly Dictionary<System.Type, UIBehaviour[]> _cache = new();//缓存，一个类型桶，每个类型可以在这里直接获取
    private readonly Dictionary<(System.Type, string), int> _nameToIndex = new();//存储每个组件的名字在上

    private readonly List<UnityAction> _toUnsubscribe = new();//将绑定的监听也缓存，方便在面板销毁时清空


    private void ScanOnceAndSplit()
    {
        //只扫描一次所有的组件
        UIBehaviour[] all = GetComponentsInChildren<UIBehaviour>(true);
        //
        var buckets = new Dictionary<System.Type, List<UIBehaviour>>();//按类型分桶，把不同class作为key，存储对应的class对象
        var ignore = new HashSet<string>(ignoreNameList, System.StringComparer.OrdinalIgnoreCase);//不区分大小写转为哈希，直接剔除不需要的组件

        foreach (var ui in all)
        {
            if (ignore.Contains(ui.gameObject.name)) continue;
            if (!NeedBindTypes.Contains(ui.GetType())) continue;//如果扫描到的类型不在预期内，跳过

            var type = ui.GetType();
            if (!buckets.TryGetValue(type, out var list))
            {
                list = new List<UIBehaviour>();
                buckets[type] = list;
            }
            list.Add(ui);

            // 为了 GetControl<T>(name) 也能 O(1),并且同类型的组件才会有重名的可能
            _nameToIndex[(ui.GetType(), ui.gameObject.name)] = list.Count - 1;
        }
        foreach (var kv in buckets)//把扫描的内容更新到缓存里，方便后续读取
            _cache[kv.Key] = kv.Value.ToArray();
    }

    private void BindEvents()//根据UI类型绑定事件，可以再添加
    {
        foreach (var btn in GetArray<Button>())
        {
            UnityAction action = () => OnButtonClick(btn.gameObject.name);
            btn.onClick.AddListener(action);
            _toUnsubscribe.Add(() => btn.onClick.RemoveListener(action));
        }

        foreach (var slider in GetArray<Slider>())
        {
            UnityAction<float> action = v => OnSliderValueChanged(slider.gameObject.name, v);
            slider.onValueChanged.AddListener(action);
            _toUnsubscribe.Add(() => slider.onValueChanged.RemoveListener(action));
        }

        foreach (var toggle in GetArray<Toggle>())
        {
            UnityAction<bool> action = v => OnToggleValueChanged(toggle.gameObject.name, v);
            toggle.onValueChanged.AddListener(action);
            _toUnsubscribe.Add(() => toggle.onValueChanged.RemoveListener(action));
        }
    }

    private void UnbindEvents()//注销所有绑定事件
    {
        foreach (var u in _toUnsubscribe) u.Invoke();
        _toUnsubscribe.Clear();
    }
    /// <summary>
    /// 获取对应UI类型的组件数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private T[] GetArray<T>() where T : UIBehaviour
    {
        if (!_cache.TryGetValue(typeof(T), out var arr) || arr == null || arr.Length == 0)
            return System.Array.Empty<T>();

        var result = new T[arr.Length];
        for (int i = 0; i < arr.Length; i++)
            result[i] = arr[i] as T;
        return result;
    }
    #endregion
    /// <summary>面板显示时调用</summary>
    public virtual void OnShow() { }
    /// <summary>面板隐藏时调用</summary>
    public virtual void OnHide() { }

}
}
