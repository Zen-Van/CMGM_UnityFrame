using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//加载不同情景的内存环境的类，根据玩家的位置和关卡进度加载卸载不同内存中的包体
public static class RuntimeEnvSetup 
{
    //进入GamePlay前就要载入内存的管理器、数据和包
    /*在Initializer中已经初始化的内容：
        UI管理器（同时初始化了所有UI组件，注册了UI按键监听）
        存档管理器（同时载入了存档元数据）

      在Initializer中已经加载的AB包：
        ui
        scenes
    */



    /// <summary>
    /// 进入GamePlay时就要载入的基础内容（内存的管理器、数据和AB包）
    /// </summary>
    public static void SetupBasic()
    {
        //进游戏时初始化所有lua相关
        //(初始化LuaEnv，添加Loader，所有Lua脚本载入内存，执行Lua根文件)
        LuaManager.Instance.Init();

        //所有基础配置表载入内存
        GameConfigManager.Instance.LoadTable<ItemInfoContainer, ItemInfo>();
        GameConfigManager.Instance.LoadTable<RoleInfoContainer, RoleInfo>();

        //游戏内基础AB包载入内存
        //已随管理器一并载入的ab包：lua、

    }

}
