using UnityEngine;

public static partial class Consts
{
    public static partial class Paths
    {
        /// <summary>
        /// 工作区根目录,决定了所有路径的检索  默认为Assets/_WorkSpace
        /// </summary>
        public const string WorkSpace = "Assets/_WorkSpace";


        public const string HotRes = WorkSpace + "/HotRes";

        //Scripts下路径
        public const string ScriptsPath = WorkSpace + "/Scripts";
        public const string Script_UI_Panel_Path = ScriptsPath + "/GameUI/Panels";

    }
}
