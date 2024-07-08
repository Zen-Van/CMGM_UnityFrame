using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public enum E_UILayer : byte
{
    /// <summary>
    /// 最底层
    /// </summary>
    Bottom,
    /// <summary>
    /// 中层
    /// </summary>
    Middle,
    /// <summary>
    /// 高层
    /// </summary>
    Top,
    /// <summary>
    /// 系统层 最高层
    /// </summary>
    System,
}


/// <summary>
/// UI管理器，该管理器须在游戏初始化时预热
/// </summary>
public class UIManager : Singleton<UIManager>
{
    #region UI管理器基础部分
    //因为管理器初始化时执行的同步方法和操作逻辑较多，所以要在GameStart的时候初始化该管理器
    private UIManager()
    {
        //创建UI摄像机
        uiCamera = GameObject.Instantiate
            (ResManager.Instance.Load<GameObject>("UI/UICamera", E_ResLoaderType.Resources))
            .GetComponent<Camera>();
        GameObject.DontDestroyOnLoad(uiCamera.gameObject);

        //创建UI面板
        uiCanvas = GameObject.Instantiate
            (ResManager.Instance.Load<GameObject>("UI/Canvas", E_ResLoaderType.Resources))
            .GetComponent<Canvas>();
        uiCanvas.worldCamera = uiCamera;
        GameObject.DontDestroyOnLoad(uiCanvas);

        //创建EventSystem
        uiEventSystem = GameObject.Instantiate
            (ResManager.Instance.Load<GameObject>("UI/EventSystem", E_ResLoaderType.Resources))
            .GetComponent<EventSystem>();
        GameObject.DontDestroyOnLoad(uiEventSystem);

        //设置层级节点
        bottomLayer = uiCanvas.transform.Find("Bottom");
        middleLayer = uiCanvas.transform.Find("Middle");
        topLayer = uiCanvas.transform.Find("Top");
        systemLayer = uiCanvas.transform.Find("System");

        InputManager.Instance.UI.Cancel.started += (ctx) =>
        {
            if (GetTopDynamicPanel() != null)
                HidePanel(GetTopDynamicPanel().name, false);
        };
    }

    private Camera uiCamera;
    private Canvas uiCanvas;
    private EventSystem uiEventSystem;

    //层级对象
    private Transform bottomLayer;
    private Transform middleLayer;
    private Transform topLayer;
    private Transform systemLayer;
    /// <summary>
    /// 获取层级对象
    /// </summary>
    public Transform GetLayerNode(E_UILayer layer)
    {
        switch (layer)
        {
            case E_UILayer.Bottom:
                return bottomLayer;
            case E_UILayer.Middle:
                return middleLayer;
            case E_UILayer.Top:
                return topLayer;
            case E_UILayer.System:
                return systemLayer;
            default:
                return null;
        }
    }

    /// <summary>
    /// 若主摄像机未赋值UI摄像机，则给主相机赋值UI摄像机
    /// </summary>
    public void SetUICameraToMain()
    {
        List<Camera> overlayCameras = Camera.main.GetUniversalAdditionalCameraData().cameraStack;
        if (!overlayCameras.Contains(uiCamera))
            overlayCameras.Add(uiCamera);
    }
    #endregion

    //需要解决的问题：
    //1、HidePanel支持销毁和失活两种操作，并针对两种不同的状态修改ShowPanel逻辑
    //2、异步加载需解决，加载到一半就执行了Hide的情况，要使用加载操作的cancellationToken标记了
    #region 面板管理相关

    #region 面板加载过程中的信息容器(提升在资源载入过程中反复操作同一面板时代码的容错性)
    //该模块用于统一管理面板异步加载过程中的数据，避免异步加载过程中对一面板反复进行显隐操作时导致逻辑混乱和代码崩溃

    /// <summary>依赖倒置，用来兼容不同的泛型类</summary>
    private abstract class PanelWrapperBase
    {
        /// <summary>加载的目标面板</summary>
        public BasePanel panel;
        /// <summary>加载所开启的伪线程</summary>
        public UniTask<GameObject> loadTask;
        /// <summary>伪线程的取消标记</summary>
        public CancellationTokenSource cancellationTokenSource;
        /// <summary>加载后面板的GameObject状态</summary>
        public bool isActive;
        /// <summary>面板的层级</summary>
        public E_UILayer layer;
    }

    private class PanelWrapper<T> : PanelWrapperBase where T : BasePanel
    {
        public PanelWrapper(T panel, bool isActive, E_UILayer layer)
        {
            this.panel = panel;
            this.isActive = isActive;
            cancellationTokenSource = new CancellationTokenSource();
            this.layer = layer;
        }
    }
    #endregion

    private Dictionary<string, PanelWrapperBase> panelDic = new Dictionary<string, PanelWrapperBase>();


    /// <summary>
    /// 通过类型显示面板
    /// </summary>
    /// <typeparam name="T">面板类型（与面板名相同）</typeparam>
    /// <param name="layer">显示层级</param>
    /// <param name="OnProgressChanged">面板加载进度回调，默认为空</param>
    /// <returns></returns>
    public async UniTask<T> ShowPanel<T>(E_UILayer layer = E_UILayer.Middle,
        UnityAction<float> OnProgressChanged = null) where T : BasePanel
    {
        string panelName = typeof(T).Name;
        PanelWrapper<T> panelInfo;

        //字典里有
        if (panelDic.ContainsKey(panelName))
        {
            panelInfo = panelDic[panelName] as PanelWrapper<T>; //取出字典里的
            if (panelInfo.panel == null) //还在加载，字典里只是空的占位
            {
                panelInfo.isActive = true; //直接改状态后返回就行了

                return (await panelInfo.loadTask).GetComponent<T>();
            }
            else //字典里已经有它的实例了
            {
                if (panelInfo.panel.gameObject.activeSelf == false)
                    panelInfo.panel.gameObject.SetActive(true);

                panelInfo.panel?.OnShow();
                return panelInfo.panel as T;
            }
        }

        //字典里没，先占位置，再加载
        panelInfo = new PanelWrapper<T>(null, true, layer);   //初始化一个面板为空的加载信息
        panelDic.Add(panelName, panelInfo);     //占位置
        //赋值且启动伪线程
        panelInfo.loadTask = ResManager.Instance.LoadAsync<GameObject>
            (Consts.Paths.Ab_UI_Panel_Path + "/" + panelName + ".prefab",
            OnProgressChanged: OnProgressChanged,
            cancellationToken: panelInfo.cancellationTokenSource.Token);
        GameObject panelObj = null;
        try
        {
            panelObj = await panelInfo.loadTask;    //等待伪线程执行完毕
        }
        catch (Exception ex) when (ex is OperationCanceledException)
        {
            CmgmLog.FrameLog_Negative($"<color=#b5a642>UI面板加载的UniTask被取消</color>：{ex}");
            return null;
        }
        if (panelObj == null || !panelDic.ContainsKey(panelName)) return null;
        //----------------------------------面板对象加载完后的操作--------------------------------------

        //等待后要重新取出一遍字典里的，因为有可能字典里的已经被修改了，与私有域的panelInfo不同
        panelInfo = panelDic[panelName] as PanelWrapper<T>;

        //并重新赋值panelObj
        panelObj = GameObject.Instantiate(panelObj, GetLayerNode(layer), false);
        //如果对象上没有脚本，自动绑一下，避免UI同学忘了，我真贴心
        if (panelObj.GetComponent<T>() == null) panelObj.AddComponent<T>();

        //将panel存进字典
        T panel = panelObj.GetComponent<T>();
        panelInfo.panel = panel;

        //设置实例化对象属性
        panelObj.name = panelName;
        panelObj.SetActive(panelInfo.isActive); //根据加载信息中的激活状态设置panel的激活状态
        if (panelInfo.isActive) //如果是激活状态再执行OnShow
            panel?.OnShow();

        return panel;
    }
    /// <summary>
    /// 通过面板名显示面板
    /// </summary>
    /// <param name="panelName">面板名</param>
    /// <param name="layer">层级</param>
    /// <param name="OnProgressChanged">面板加载进度回调，默认为空</param>
    /// <returns></returns>
    public async UniTask<BasePanel> ShowPanel(string panelName, E_UILayer layer = E_UILayer.Middle,
    UnityAction<float> OnProgressChanged = null)
    {
        PanelWrapperBase panelInfo;

        //字典里有
        if (panelDic.ContainsKey(panelName))
        {
            panelInfo = panelDic[panelName]; //取出字典里的
            if (panelInfo.panel == null) //还在加载，字典里只是空的占位
            {
                panelInfo.isActive = true; //直接改状态后返回就行了
                return (await panelInfo.loadTask).GetComponent<BasePanel>();
            }
            else //字典里已经有它的实例了
            {
                if (panelInfo.panel.gameObject.activeSelf == false)
                    panelInfo.panel.gameObject.SetActive(true);

                panelInfo.panel?.OnShow();
                return panelInfo.panel;
            }
        }

        //字典里没，先占位置，再加载
        panelInfo = new PanelWrapper<BasePanel>(null, true, layer);   //初始化一个面板为空的加载信息
        panelDic.Add(panelName, panelInfo);     //占位置
        //赋值且启动伪线程
        panelInfo.loadTask = ResManager.Instance.LoadAsync<GameObject>
            (Consts.Paths.Ab_UI_Panel_Path + "/" + panelName + ".prefab",
            OnProgressChanged: OnProgressChanged,
            cancellationToken: panelInfo.cancellationTokenSource.Token);
        GameObject panelObj = null;
        try
        {
            panelObj = await panelInfo.loadTask;    //等待伪线程执行完毕
        }
        catch (Exception ex) when (ex is OperationCanceledException)
        {
            CmgmLog.FrameLog_Negative($"<color=#b5a642>UI面板加载的UniTask被取消</color>：{ex}");
            return null;
        }
        if (panelObj == null || !panelDic.ContainsKey(panelName)) return null;
        //----------------------------------面板对象加载完后的操作--------------------------------------

        //等待后要重新取出一遍字典里的，因为有可能字典里的已经被修改了，与私有域的panelInfo不同
        panelInfo = panelDic[panelName] as PanelWrapper<BasePanel>;

        //并重新赋值panelObj
        panelObj = GameObject.Instantiate(panelObj, GetLayerNode(layer), false);
        //如果对象上没有脚本，自动绑一下，避免UI同学忘了，我真贴心
        if (panelObj.GetComponent(Type.GetType(panelName)) == null) panelObj.AddComponent(Type.GetType(panelName));

        //将panel存进字典
        BasePanel panel = panelObj.GetComponent<BasePanel>();
        panelInfo.panel = panel;

        //设置实例化对象属性
        panelObj.name = panelName;
        panelObj.SetActive(panelInfo.isActive); //根据加载信息中的激活状态设置panel的激活状态
        if (panelInfo.isActive) //如果是激活状态再执行OnShow
            panel?.OnShow();
        return panel;
    }


    /// <summary>
    /// 通过类型隐藏面板
    /// </summary>
    /// <typeparam name="T">面板类型（与面板名相同）</typeparam>
    /// <param name="isDestroy">是否销毁对象（销毁的话内存也释放掉）</param>
    public void HidePanel<T>(bool isDestroy = false) where T : BasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            PanelWrapper<T> panelInfo = panelDic[panelName] as PanelWrapper<T>; //取出字典里的
            if (panelInfo.panel == null) //还在加载，字典里只是空的占位
            {
                if (isDestroy)
                {
                    panelInfo.cancellationTokenSource.Cancel(); //不干了！打断加载资源等待的UniTask
                    panelDic.Remove(panelName);
                }
                else
                    panelInfo.isActive = false; //等加载完再失活
            }
            else //字典里已经有它的实例了
            {
                panelInfo.panel?.OnHide();
                if (isDestroy)
                {
                    GameObject.Destroy(panelInfo.panel.gameObject);
                    panelDic.Remove(panelName);
                }
                else
                {
                    panelInfo.panel.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            //如果字典里找不到就啥也不干
        }
    }
    /// <summary>
    /// 通过面板名隐藏面板
    /// </summary>
    /// <param name="panelName">面板名</param>
    /// <param name="isDestroy">是否销毁对象（销毁的话内存也释放掉）</param>
    public void HidePanel(string panelName, bool isDestroy = false)
    {
        if (panelDic.ContainsKey(panelName))
        {
            PanelWrapperBase panelInfo = panelDic[panelName]; //取出字典里的
            if (panelInfo.panel == null) //还在加载，字典里只是空的占位
            {
                if (isDestroy)
                {
                    panelInfo.cancellationTokenSource.Cancel(); //不干了！打断加载资源等待的UniTask
                    panelDic.Remove(panelName);
                }
                else
                    panelInfo.isActive = false; //等加载完再失活
            }
            else //字典里已经有它的实例了
            {
                panelInfo.panel?.OnHide();
                if (isDestroy)
                {
                    GameObject.Destroy(panelInfo.panel.gameObject);
                    panelDic.Remove(panelName);
                }
                else
                {
                    panelInfo.panel.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            //如果字典里找不到就啥也不干
        }
    }

    /// <summary>
    /// 清空所有面板
    /// </summary>
    public void ClearPanel()
    {
        foreach (var panelInfo in panelDic.Values)
        {
            GameObject.Destroy(panelInfo.panel.gameObject);
        }
        panelDic.Clear();
    }

    /// <summary>
    /// 得到顶部的动态面板
    /// </summary>
    /// <returns></returns>
    public BasePanel GetTopDynamicPanel()
    {
        var allActiveDynamicPanle = panelDic.Values
            .Where(target => target.panel.gameObject.activeInHierarchy)  //激活的面板中
            .Where(target => target.panel.isDynamic);   //动态的面板

        var topPanel = allActiveDynamicPanle
            .Where(target => (int)target.layer == allActiveDynamicPanle.Max(target => (int)target.layer))  //所在层级最高的那些
            .OrderBy(target => target.panel.transform.GetSiblingIndex()) //按照监视面板中同级对象的索引进行排序
            .LastOrDefault();    //在同级对象上索引最大的

        if (topPanel == null)
        {
            CmgmLog.FrameLog_Negative("没有找到激活且动态的顶层面板，请检查是否正确设置面板的动态性");
        }
        return topPanel?.panel;
    }
    /// <summary>
    /// 得到面板
    /// </summary>
    /// <typeparam name="T">面板类型（与面板名相同）</typeparam>
    /// <returns>为空则未找到当前面板</returns>
    public async UniTask<T> GetPanel<T>() where T : BasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            PanelWrapper<T> panelInfo = panelDic[panelName] as PanelWrapper<T>;
            if (panelInfo.panel == null)
            {
                return (await panelInfo.loadTask).GetComponent<T>();
            }
            else if (panelInfo.isActive)
            {
                return panelInfo.panel as T;
            }
        }
        return null;
    }
    #endregion

    #region 控件管理相关
    /// <summary>
    /// 为控件添加自定义事件
    /// </summary>
    /// <param name="control">对应的控件</param>
    /// <param name="type">事件的类型</param>
    /// <param name="callBack">响应的函数</param>
    public static void AddCustomEventListener(UIBehaviour control, EventTriggerType type, UnityAction<BaseEventData> callback)
    {
        EventTrigger trigger = control.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = control.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(callback);

        trigger.triggers.Add(entry);
    }
    #endregion
}
