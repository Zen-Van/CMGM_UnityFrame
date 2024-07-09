using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : BasePanel
{
    public Image background;
    public Slider slider;

    AsyncOperation operation;

    protected override void Awake()
    {
        //这个面板就不找控件了，一共就两个东西，手动绑做成预制体更节省性能
        //base.Awake();
    }

    private void Update()
    {
        if (GameSystem.isloading)
        {
            slider.value = operation.progress;
        }
    }

    /// <summary>
    /// 更新slider进度
    /// </summary>
    /// <param name="value">进度：【0~1】之间的数</param>
    public void StartLoading(AsyncOperation operation, Sprite sprite = default)
    {
        if (sprite != default) background.sprite = sprite;

        this.operation = operation;
        GameSystem.isloading = true;
    }


    private void OnDisable()
    {
        GameSystem.isloading = false;
        slider.value = 0f;
        operation = null;
    }
}