﻿using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmgmFrameSettings : ScriptableObject
{
    private static CmgmFrameSettings instance;
    public static CmgmFrameSettings Instance
        => instance ??= Resources.Load<CmgmFrameSettings>(nameof(CmgmFrameSettings));

    [BoxGroup("AB包加载模式")]
    [LabelText("通过AB包加载资源")]
    public bool IS_TRUE_AB_LOAD_IN_EDITOR;

    [BoxGroup("AB包加载模式")]
    [LabelText("通过AB包加载LUA")]
    public bool IS_LUA_LOAD_FROM_AB_IN_EDITOR;

    [BoxGroup("LUA根文件")]
    [LabelText("Lua引导文件路径")]
    [InfoBox("这里定义了一些公用函数和所有的lua到C#的公共绑定")]
    public string ROOT_FILE_URI;


    [BoxGroup("UI相关")]
    [LabelText("打印机是否自动打印")]
    public bool TEXT_PRINTER_AUTO;

    [BoxGroup("UI相关")]
    [LabelText("打印机每秒字数")]
    [Range(1.0f, 10.0f)]
    public float TEXT_PRINTER_SPEED;

    [BoxGroup("UI相关")]
    [LabelText("打印完毕停留时间")]
    [Range(0.5f, 2.5f)]
    public float TEXT_PRINTER_STAY;
}