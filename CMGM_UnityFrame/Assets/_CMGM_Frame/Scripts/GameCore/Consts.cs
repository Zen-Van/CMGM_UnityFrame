using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Consts
{
    /// <summary>
    /// 木蝉Unity框架的路径常量
    /// </summary>
    public static class Paths
    {
        // 注：Editor等不会打包到最终游戏包中的目录不在此脚本中声明（在各自Editor脚本中声明），以节约性能 
        //需要检查正确性的Editor路径直接在FramePathCheck中声明

        /// <summary>
        /// 木蝉Unity框架结构的根目录，一般情况下为Assets
        /// </summary>
        public const string RootPath = "Assets/_CMGM_Frame";

        //Scripts下路径
        public const string ScriptsPath = RootPath + "/Scripts";
        public const string Script_UI_Panel_Path = ScriptsPath + "/GameUI/Panels";


        //AB包存放路径
        public static string Ab_Bundle_Path = Application.streamingAssetsPath + "/AssetBundles";
        //AbRes下路径
        public const string AbResPath = RootPath + "/AbRes";
        public const string ReuseableResPath = AbResPath + "/ReuseableObject";
        public const string Ab_UI_Panel_Path = AbResPath + "/UI/Panels";
        public const string Lua_Path = AbResPath + "/Lua";




        /// <summary>
        /// 游戏包基础Scenes资源路径
        /// <para>Nohot前缀的资源，皆为Resources下的相对路径，需使用Resources类加载</para>
        /// </summary>
        public const string Nohot_ScenesPath = "/Scenes";
    }


}
